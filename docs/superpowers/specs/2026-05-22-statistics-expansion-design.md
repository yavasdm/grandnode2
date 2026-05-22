# Statistics Tab Expansion — Design Spec

**Date:** 2026-05-22  
**Status:** Approved  
**Scope:** Enrich the GrandNode2 open-source admin Reports section with three new reports built entirely from existing MongoDB data.

---

## Goal

The existing Reports section covers order operations (bestsellers, country breakdown, averages) and basic customer counts. Three meaningful gaps exist:

1. No visibility into **which product categories** drive revenue
2. No signal on **customer loyalty** (new vs. returning)
3. No **vendor-level accountability** despite the platform being multi-vendor

All three can be built from data already stored in MongoDB — no new tracking infrastructure required.

---

## Reports to Add

### 1. Revenue by Category

**Purpose:** Show admins which product categories generate the most revenue and orders, so they can make informed merchandising and promotion decisions.

**Data source:** `Order` collection → line items → `Product.CategoryIds` → `Category.Name`

**Filters:**
- Store (dropdown, default: all)
- Date range (StartDate / EndDate)
- Order status (dropdown: all / pending / processing / complete / cancelled)
- Payment status (dropdown: all / pending / paid / etc.)

**Output columns:**
| Column | Type |
|---|---|
| Category Name | string |
| Number of Orders | int |
| Total Revenue | formatted price |
| % of Total Revenue | decimal |

**Service method** (add to `IOrderReportService`):
```csharp
Task<IList<CategoryRevenueReportLine>> GetCategoryRevenueReport(
    string storeId = "",
    DateTime? startTimeUtc = null,
    DateTime? endTimeUtc = null,
    int? os = null,
    PaymentStatus? ps = null);
```

**New utility model** (`Grand.Business.Core/Utilities/System/CategoryRevenueReportLine.cs`):
```csharp
public class CategoryRevenueReportLine
{
    public string CategoryId { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
}
```

**Implementation note:** Aggregate `Order.OrderItems` where `Order.StoreId`, `Order.OrderStatusId`, `Order.PaymentStatusId`, and `Order.CreatedOnUtc` match filters. Join each `OrderItem.ProductId` to its `Product.CategoryIds`. Group by category, sum `OrderItem.PriceExclTax * Quantity`.

---

### 2. New vs. Returning Customers

**Purpose:** Show the ratio of first-time buyers vs. repeat buyers over a time period — the primary signal of customer loyalty and retention.

**Data source:** `Order` collection → `CustomerId` → check if customer has prior orders before the period

**Logic:** For each order in the date range, a customer is "new" if they have no completed orders before `StartDate`; otherwise "returning".

**Filters:**
- Store (dropdown, default: all)
- Date range (StartDate / EndDate)
- Group by: Day / Week / Month (radio)

**Output columns:**
| Column | Type |
|---|---|
| Period | string (date label) |
| New Customers | int |
| Returning Customers | int |
| New % | decimal |

**Service method** (add to `ICustomerReportService`):
```csharp
Task<IList<NewVsReturningReportLine>> GetNewVsReturningReport(
    string storeId = "",
    DateTime? startTimeUtc = null,
    DateTime? endTimeUtc = null,
    ReportGroupBy groupBy = ReportGroupBy.Month);
```

**New utility model** (`Grand.Business.Core/Utilities/Customers/NewVsReturningReportLine.cs`):
```csharp
public class NewVsReturningReportLine
{
    public string TimePeriod { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
}
```

**New enum** (`Grand.Business.Core/Enums/ReportGroupBy.cs`):
```csharp
public enum ReportGroupBy { Day = 0, Week = 1, Month = 2 }
```

---

### 3. Vendor Performance

**Purpose:** Give admins a side-by-side view of how each vendor is performing — revenue, volume, and average order value — essential for a multi-vendor platform.

**Data source:** `Order` collection → `OrderItems.VendorId` → `Vendor.Name`

**Filters:**
- Date range (StartDate / EndDate)
- Order status (dropdown)
- Payment status (dropdown)

**Output columns:**
| Column | Type |
|---|---|
| Vendor Name | string |
| Number of Orders | int |
| Total Revenue | formatted price |
| Average Order Value | formatted price |

**New service interface** (`Grand.Business.Core/Interfaces/System/Reports/IVendorReportService.cs`):
```csharp
public interface IVendorReportService
{
    Task<IList<VendorPerformanceReportLine>> GetVendorPerformanceReport(
        DateTime? startTimeUtc = null,
        DateTime? endTimeUtc = null,
        int? os = null,
        PaymentStatus? ps = null);
}
```

**New service implementation:** `Grand.Business.Catalog/Services/Products/VendorReportService.cs`  
Registered in `Grand.Business.Catalog`'s `StartupApplication`.

**New utility model** (`Grand.Business.Core/Utilities/Catalog/VendorPerformanceReportLine.cs`):
```csharp
public class VendorPerformanceReportLine
{
    public string VendorId { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}
```

---

## File Checklist

### New files

| File | Purpose |
|---|---|
| `Grand.Business.Core/Utilities/System/CategoryRevenueReportLine.cs` | Domain model |
| `Grand.Business.Core/Utilities/Customers/NewVsReturningReportLine.cs` | Domain model |
| `Grand.Business.Core/Enums/ReportGroupBy.cs` | Enum for grouping |
| `Grand.Business.Core/Utilities/Catalog/VendorPerformanceReportLine.cs` | Domain model |
| `Grand.Business.Core/Interfaces/System/Reports/IVendorReportService.cs` | Service interface |
| `Grand.Business.Catalog/Services/Products/VendorReportService.cs` | Service implementation |
| `Grand.Web.AdminShared/Models/Reports/CategoryRevenueReportModel.cs` | Filter + line view models |
| `Grand.Web.AdminShared/Models/Reports/NewVsReturningReportModel.cs` | Filter + line view models |
| `Grand.Web.AdminShared/Models/Reports/VendorPerformanceReportModel.cs` | Filter + line view models |
| `Grand.Web.Admin/Areas/Admin/Views/Reports/CategoryRevenueReport.cshtml` | Razor view |
| `Grand.Web.Admin/Areas/Admin/Views/Reports/NewVsReturningReport.cshtml` | Razor view |
| `Grand.Web.Admin/Areas/Admin/Views/Reports/VendorPerformanceReport.cshtml` | Razor view |

### Modified files

| File | Change |
|---|---|
| `Grand.Business.Core/Interfaces/System/Reports/IOrderReportService.cs` | Add `GetCategoryRevenueReport` method |
| `Grand.Business.Checkout/Services/Orders/OrderReportService.cs` | Implement `GetCategoryRevenueReport` |
| `Grand.Business.Core/Interfaces/System/Reports/ICustomerReportService.cs` | Add `GetNewVsReturningReport` method |
| `Grand.Business.Customers/Services/CustomerReportService.cs` | Implement `GetNewVsReturningReport` |
| `Grand.Business.Catalog/StartupApplication.cs` | Register `IVendorReportService` |
| `Grand.Web.Admin/Controllers/ReportsController.cs` | Add 6 new actions (GET + POST list for each report) |

---

## Conventions to Follow

- All view models use `[GrandResourceDisplayName("Admin.Reports.X")]` on properties
- Date inputs use `[UIHint("DateNullable")]`
- Controller actions follow the `[ReportName]()` / `[HttpPost] [ReportName]List()` pattern
- All service methods accept optional `storeId`, `startTimeUtc`, `endTimeUtc` parameters
- Prices formatted via `IPriceFormatter.FormatPrice()` before returning to view
- Times converted to UTC via `IDateTimeService.ConvertToUtcTime()` in controller
- DI registration in the owning project's `StartupApplication`, not from another layer

---

## Out of Scope

- Dashboard widgets (view components) for these reports — can be added later
- Export to CSV/Excel — follows separately if needed
- Pagination on vendor report — vendor count is typically small, full list is fine
- Chart visualizations — the existing Kendo Grid tabular format is sufficient for v1
