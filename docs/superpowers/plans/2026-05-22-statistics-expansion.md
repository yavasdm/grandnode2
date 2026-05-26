# Statistics Tab Expansion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add three new admin reports — Revenue by Category, New vs. Returning Customers, and Vendor Performance — to the GrandNode2 Reports section, using only existing MongoDB data.

**Architecture:** Each report follows the established 4-layer pattern: utility model in `Grand.Business.Core/Utilities/`, service method on an existing or new interface, view model in `Grand.Web.AdminShared/Models/`, and a controller action + Razor view in `Grand.Web.Admin`. The new reports integrate seamlessly into the existing Reports navigation.

**Tech Stack:** C# / .NET 10, MongoDB via `IRepository<T>` LINQ, ASP.NET Core MVC, Kendo UI Grid, MSTest

---

## File Map

### New files
| Path | Purpose |
|---|---|
| `src/Business/Grand.Business.Core/Utilities/System/CategoryRevenueReportLine.cs` | Report utility model |
| `src/Business/Grand.Business.Core/Utilities/System/NewVsReturningReportLine.cs` | Report utility model |
| `src/Business/Grand.Business.Core/Utilities/System/VendorPerformanceReportLine.cs` | Report utility model |
| `src/Business/Grand.Business.Core/Enums/ReportGroupBy.cs` | Grouping enum |
| `src/Business/Grand.Business.Core/Interfaces/System/Reports/IVendorReportService.cs` | Service interface |
| `src/Business/Grand.Business.Catalog/Services/Products/VendorReportService.cs` | Service implementation |
| `src/Web/Grand.Web.AdminShared/Models/Reports/CategoryRevenueReportModel.cs` | Filter + line view models |
| `src/Web/Grand.Web.AdminShared/Models/Reports/NewVsReturningReportModel.cs` | Filter + line view models |
| `src/Web/Grand.Web.AdminShared/Models/Reports/VendorPerformanceReportModel.cs` | Filter + line view models |
| `src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/CategoryRevenueReport.cshtml` | Razor view |
| `src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/NewVsReturningReport.cshtml` | Razor view |
| `src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/VendorPerformanceReport.cshtml` | Razor view |

### Modified files
| Path | Change |
|---|---|
| `src/Business/Grand.Business.Core/Interfaces/System/Reports/IOrderReportService.cs` | Add `GetCategoryRevenueReport` |
| `src/Business/Grand.Business.Checkout/Services/Orders/OrderReportService.cs` | Implement `GetCategoryRevenueReport` |
| `src/Business/Grand.Business.Core/Interfaces/System/Reports/ICustomerReportService.cs` | Add `GetNewVsReturningReport` |
| `src/Business/Grand.Business.Customers/Services/CustomerReportService.cs` | Implement `GetNewVsReturningReport` |
| `src/Business/Grand.Business.Catalog/Startup/StartupApplication.cs` | Register `IVendorReportService` |
| `src/Web/Grand.Web.Admin/Controllers/ReportsController.cs` | Add 6 new actions + new DI params |
| `src/Web/Grand.Web.Admin/Areas/Admin/Views/_ViewImports.cshtml` | Add `@using Grand.Web.AdminShared.Models.Reports` |
| `src/Modules/Grand.Module.Installer/Utilities/StandardAdminSiteMap.cs` | Add 3 navigation entries |

---

## Task 1: Domain utility models and enum

**Files:**
- Create: `src/Business/Grand.Business.Core/Utilities/System/CategoryRevenueReportLine.cs`
- Create: `src/Business/Grand.Business.Core/Utilities/System/NewVsReturningReportLine.cs`
- Create: `src/Business/Grand.Business.Core/Utilities/System/VendorPerformanceReportLine.cs`
- Create: `src/Business/Grand.Business.Core/Enums/ReportGroupBy.cs`

- [ ] **Step 1: Create CategoryRevenueReportLine**

```csharp
// src/Business/Grand.Business.Core/Utilities/System/CategoryRevenueReportLine.cs
namespace Grand.Business.Core.Utilities.System;

public class CategoryRevenueReportLine
{
    public string CategoryId { get; set; }
    public double TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
}
```

- [ ] **Step 2: Create NewVsReturningReportLine**

```csharp
// src/Business/Grand.Business.Core/Utilities/System/NewVsReturningReportLine.cs
namespace Grand.Business.Core.Utilities.System;

public class NewVsReturningReportLine
{
    public string TimePeriod { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
}
```

- [ ] **Step 3: Create VendorPerformanceReportLine**

```csharp
// src/Business/Grand.Business.Core/Utilities/System/VendorPerformanceReportLine.cs
namespace Grand.Business.Core.Utilities.System;

public class VendorPerformanceReportLine
{
    public string VendorId { get; set; }
    public double TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public double AverageOrderValue { get; set; }
}
```

- [ ] **Step 4: Create ReportGroupBy enum**

The `Enums/` directory already exists at `src/Business/Grand.Business.Core/Enums/`. Create a new file directly in it (no subdirectory needed).

```csharp
// src/Business/Grand.Business.Core/Enums/ReportGroupBy.cs
namespace Grand.Business.Core.Enums;

public enum ReportGroupBy
{
    Day = 0,
    Week = 1,
    Month = 2
}
```

- [ ] **Step 5: Build to verify**

Run: `dotnet build src/Business/Grand.Business.Core/Grand.Business.Core.csproj`
Expected: Build succeeded, 0 errors

- [ ] **Step 6: Commit**

```bash
git add src/Business/Grand.Business.Core/Utilities/System/CategoryRevenueReportLine.cs \
        src/Business/Grand.Business.Core/Utilities/System/NewVsReturningReportLine.cs \
        src/Business/Grand.Business.Core/Utilities/System/VendorPerformanceReportLine.cs \
        src/Business/Grand.Business.Core/Enums/ReportGroupBy.cs
git commit -m "feat: add report utility models and ReportGroupBy enum"
```

---

## Task 2: Service interfaces

**Files:**
- Modify: `src/Business/Grand.Business.Core/Interfaces/System/Reports/IOrderReportService.cs`
- Modify: `src/Business/Grand.Business.Core/Interfaces/System/Reports/ICustomerReportService.cs`
- Create: `src/Business/Grand.Business.Core/Interfaces/System/Reports/IVendorReportService.cs`

- [ ] **Step 1: Add GetCategoryRevenueReport to IOrderReportService**

Open `src/Business/Grand.Business.Core/Interfaces/System/Reports/IOrderReportService.cs`.

Add this using at the top (after existing usings):
```csharp
using Grand.Business.Core.Enums;
```

Add this method to the interface body (before the closing `}`):
```csharp
/// <summary>
///     Get revenue grouped by product category
/// </summary>
/// <param name="storeId">Store identifier; "" to load all records</param>
/// <param name="startTimeUtc">Start date</param>
/// <param name="endTimeUtc">End date</param>
/// <param name="os">Order status</param>
/// <param name="ps">Payment status</param>
/// <returns>Result</returns>
Task<IList<CategoryRevenueReportLine>> GetCategoryRevenueReport(
    string storeId = "",
    DateTime? startTimeUtc = null,
    DateTime? endTimeUtc = null,
    int? os = null,
    PaymentStatus? ps = null);
```

- [ ] **Step 2: Add GetNewVsReturningReport to ICustomerReportService**

Open `src/Business/Grand.Business.Core/Interfaces/System/Reports/ICustomerReportService.cs`.

Add these usings at the top:
```csharp
using Grand.Business.Core.Enums;
```

Add this method to the interface body:
```csharp
/// <summary>
///     Get new vs returning customers report
/// </summary>
/// <param name="storeId">Store identifier; "" to load all records</param>
/// <param name="startTimeUtc">Start date</param>
/// <param name="endTimeUtc">End date</param>
/// <param name="groupBy">Grouping period</param>
/// <returns>Result</returns>
Task<IList<NewVsReturningReportLine>> GetNewVsReturningReport(
    string storeId = "",
    DateTime? startTimeUtc = null,
    DateTime? endTimeUtc = null,
    ReportGroupBy groupBy = ReportGroupBy.Month);
```

- [ ] **Step 3: Create IVendorReportService**

```csharp
// src/Business/Grand.Business.Core/Interfaces/System/Reports/IVendorReportService.cs
using Grand.Business.Core.Utilities.System;
using Grand.Domain.Payments;

namespace Grand.Business.Core.Interfaces.System.Reports;

public interface IVendorReportService
{
    /// <summary>
    ///     Get vendor performance report
    /// </summary>
    /// <param name="startTimeUtc">Start date</param>
    /// <param name="endTimeUtc">End date</param>
    /// <param name="os">Order status</param>
    /// <param name="ps">Payment status</param>
    /// <returns>Result</returns>
    Task<IList<VendorPerformanceReportLine>> GetVendorPerformanceReport(
        DateTime? startTimeUtc = null,
        DateTime? endTimeUtc = null,
        int? os = null,
        PaymentStatus? ps = null);
}
```

- [ ] **Step 4: Build to verify interfaces compile**

Run: `dotnet build src/Business/Grand.Business.Core/Grand.Business.Core.csproj`
Expected: Build succeeded, 0 errors

Note: `OrderReportService` and `CustomerReportService` will now fail to build (unimplemented interface methods). That is expected — fix in Tasks 3 and 4.

- [ ] **Step 5: Commit**

```bash
git add src/Business/Grand.Business.Core/Interfaces/System/Reports/IOrderReportService.cs \
        src/Business/Grand.Business.Core/Interfaces/System/Reports/ICustomerReportService.cs \
        src/Business/Grand.Business.Core/Interfaces/System/Reports/IVendorReportService.cs
git commit -m "feat: add report service interface methods and IVendorReportService"
```

---

## Task 3: Implement GetCategoryRevenueReport in OrderReportService

**Files:**
- Modify: `src/Business/Grand.Business.Checkout/Services/Orders/OrderReportService.cs`

- [ ] **Step 1: Add the implementation method**

Open `src/Business/Grand.Business.Checkout/Services/Orders/OrderReportService.cs`.

Add missing usings at the top if not already present:
```csharp
using Grand.Business.Core.Enums;
```

Add the following method inside the `#region Methods` block (after the last existing method, before `#endregion`):

```csharp
/// <inheritdoc />
public virtual async Task<IList<CategoryRevenueReportLine>> GetCategoryRevenueReport(
    string storeId = "",
    DateTime? startTimeUtc = null,
    DateTime? endTimeUtc = null,
    int? os = null,
    PaymentStatus? ps = null)
{
    var query = from p in _orderRepository.Table select p;
    query = query.Where(o => !o.Deleted);
    if (!string.IsNullOrEmpty(storeId))
        query = query.Where(o => o.StoreId == storeId);
    if (os.HasValue)
        query = query.Where(o => o.OrderStatusId == os.Value);
    if (ps.HasValue)
        query = query.Where(o => o.PaymentStatusId == ps.Value);
    if (startTimeUtc.HasValue)
        query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
    if (endTimeUtc.HasValue)
        query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

    var orderItems = (from order in query
        from item in order.OrderItems
        select new { item.ProductId, item.PriceExclTax })
        .ToList();

    if (!orderItems.Any())
        return new List<CategoryRevenueReportLine>();

    var productIds = orderItems.Select(x => x.ProductId).Distinct().ToList();
    var products = (from p in _productRepository.Table
        where productIds.Contains(p.Id)
        select new { p.Id, p.CategoryIds })
        .ToList();

    var productCategoryMap = products.ToDictionary(
        p => p.Id,
        p => p.CategoryIds ?? new List<string>());

    var categoryRevenue = new Dictionary<string, (double Revenue, int Orders)>();
    foreach (var item in orderItems)
    {
        if (!productCategoryMap.TryGetValue(item.ProductId, out var categoryIds))
            continue;
        foreach (var categoryId in categoryIds)
        {
            if (!categoryRevenue.ContainsKey(categoryId))
                categoryRevenue[categoryId] = (0, 0);
            categoryRevenue[categoryId] = (
                categoryRevenue[categoryId].Revenue + item.PriceExclTax,
                categoryRevenue[categoryId].Orders + 1);
        }
    }

    return await Task.FromResult(categoryRevenue
        .Select(kvp => new CategoryRevenueReportLine {
            CategoryId = kvp.Key,
            TotalRevenue = kvp.Value.Revenue,
            TotalOrders = kvp.Value.Orders
        })
        .OrderByDescending(x => x.TotalRevenue)
        .ToList());
}
```

Note: `PriceExclTax` is the line-item total (price × quantity already factored in by the order creation process). It is of type `double`, matching `OrderByCountryReportLine.SumOrders`.

- [ ] **Step 2: Build to verify**

Run: `dotnet build src/Business/Grand.Business.Checkout/Grand.Business.Checkout.csproj`
Expected: Build succeeded, 0 errors

- [ ] **Step 3: Commit**

```bash
git add src/Business/Grand.Business.Checkout/Services/Orders/OrderReportService.cs
git commit -m "feat: implement GetCategoryRevenueReport in OrderReportService"
```

---

## Task 4: Implement GetNewVsReturningReport in CustomerReportService

**Files:**
- Modify: `src/Business/Grand.Business.Customers/Services/CustomerReportService.cs`

- [ ] **Step 1: Add missing using**

Open `src/Business/Grand.Business.Customers/Services/CustomerReportService.cs`.

Add this using at the top if not present:
```csharp
using Grand.Business.Core.Enums;
using System.Globalization;
```

- [ ] **Step 2: Add the implementation method**

Add the following inside the `#region Methods` block (after the last existing method, before `#endregion`):

```csharp
/// <inheritdoc />
public virtual async Task<IList<NewVsReturningReportLine>> GetNewVsReturningReport(
    string storeId = "",
    DateTime? startTimeUtc = null,
    DateTime? endTimeUtc = null,
    ReportGroupBy groupBy = ReportGroupBy.Month)
{
    var query = from o in _orderRepository.Table select o;
    query = query.Where(o => !o.Deleted);
    if (!string.IsNullOrEmpty(storeId))
        query = query.Where(o => o.StoreId == storeId);
    if (startTimeUtc.HasValue)
        query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
    if (endTimeUtc.HasValue)
        query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

    var ordersInPeriod = query
        .Select(o => new { o.CustomerId, o.CreatedOnUtc })
        .ToList();

    if (!ordersInPeriod.Any())
        return new List<NewVsReturningReportLine>();

    // Customers who placed any order before the start of the period are "returning"
    var priorQuery = from o in _orderRepository.Table select o;
    priorQuery = priorQuery.Where(o => !o.Deleted);
    if (!string.IsNullOrEmpty(storeId))
        priorQuery = priorQuery.Where(o => o.StoreId == storeId);
    if (startTimeUtc.HasValue)
        priorQuery = priorQuery.Where(o => o.CreatedOnUtc < startTimeUtc.Value);

    var priorCustomerIds = priorQuery
        .Select(o => o.CustomerId)
        .Distinct()
        .ToHashSet();

    var result = ordersInPeriod
        .GroupBy(o => GetPeriodKey(o.CreatedOnUtc, groupBy))
        .OrderBy(g => g.Key)
        .Select(g =>
        {
            var uniqueCustomers = g.Select(o => o.CustomerId).Distinct().ToList();
            return new NewVsReturningReportLine {
                TimePeriod = g.Key,
                NewCustomers = uniqueCustomers.Count(cid => !priorCustomerIds.Contains(cid)),
                ReturningCustomers = uniqueCustomers.Count(cid => priorCustomerIds.Contains(cid))
            };
        })
        .ToList();

    return await Task.FromResult(result);
}

private static string GetPeriodKey(DateTime date, ReportGroupBy groupBy)
{
    return groupBy switch {
        ReportGroupBy.Day => date.ToString("yyyy-MM-dd"),
        ReportGroupBy.Week => $"{date.Year}-W{ISOWeek.GetWeekOfYear(date):D2}",
        _ => date.ToString("yyyy-MM")
    };
}
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Business/Grand.Business.Customers/Grand.Business.Customers.csproj`
Expected: Build succeeded, 0 errors

- [ ] **Step 4: Commit**

```bash
git add src/Business/Grand.Business.Customers/Services/CustomerReportService.cs
git commit -m "feat: implement GetNewVsReturningReport in CustomerReportService"
```

---

## Task 5: Create VendorReportService and register it

**Files:**
- Create: `src/Business/Grand.Business.Catalog/Services/Products/VendorReportService.cs`
- Modify: `src/Business/Grand.Business.Catalog/Startup/StartupApplication.cs`

- [ ] **Step 1: Create VendorReportService**

```csharp
// src/Business/Grand.Business.Catalog/Services/Products/VendorReportService.cs
using Grand.Business.Core.Interfaces.System.Reports;
using Grand.Business.Core.Utilities.System;
using Grand.Data;
using Grand.Domain.Orders;
using Grand.Domain.Payments;

namespace Grand.Business.Catalog.Services.Products;

public class VendorReportService : IVendorReportService
{
    private readonly IRepository<Order> _orderRepository;

    public VendorReportService(IRepository<Order> orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public virtual async Task<IList<VendorPerformanceReportLine>> GetVendorPerformanceReport(
        DateTime? startTimeUtc = null,
        DateTime? endTimeUtc = null,
        int? os = null,
        PaymentStatus? ps = null)
    {
        var query = from o in _orderRepository.Table select o;
        query = query.Where(o => !o.Deleted);
        if (os.HasValue)
            query = query.Where(o => o.OrderStatusId == os.Value);
        if (ps.HasValue)
            query = query.Where(o => o.PaymentStatusId == ps.Value);
        if (startTimeUtc.HasValue)
            query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
        if (endTimeUtc.HasValue)
            query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

        var vendorItems = (from order in query
            from item in order.OrderItems
            where !string.IsNullOrEmpty(item.VendorId)
            select new { item.VendorId, item.PriceExclTax, OrderId = order.Id })
            .ToList();

        if (!vendorItems.Any())
            return new List<VendorPerformanceReportLine>();

        var result = vendorItems
            .GroupBy(x => x.VendorId)
            .Select(g =>
            {
                var totalRevenue = g.Sum(x => x.PriceExclTax);
                var totalOrders = g.Select(x => x.OrderId).Distinct().Count();
                return new VendorPerformanceReportLine {
                    VendorId = g.Key,
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0
                };
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();

        return await Task.FromResult(result);
    }
}
```

- [ ] **Step 2: Register in StartupApplication**

Open `src/Business/Grand.Business.Catalog/Startup/StartupApplication.cs`.

Find the existing `RegisterProductsService` (or similar) private method and add:
```csharp
serviceCollection.AddScoped<IVendorReportService, VendorReportService>();
```

Also ensure these usings are present at the top of the file:
```csharp
using Grand.Business.Catalog.Services.Products;
using Grand.Business.Core.Interfaces.System.Reports;
```

- [ ] **Step 3: Build to verify**

Run: `dotnet build src/Business/Grand.Business.Catalog/Grand.Business.Catalog.csproj`
Expected: Build succeeded, 0 errors

- [ ] **Step 4: Commit**

```bash
git add src/Business/Grand.Business.Catalog/Services/Products/VendorReportService.cs \
        src/Business/Grand.Business.Catalog/Startup/StartupApplication.cs
git commit -m "feat: add VendorReportService and register in DI"
```

---

## Task 6: View models and ViewImports update

**Files:**
- Create: `src/Web/Grand.Web.AdminShared/Models/Reports/CategoryRevenueReportModel.cs`
- Create: `src/Web/Grand.Web.AdminShared/Models/Reports/NewVsReturningReportModel.cs`
- Create: `src/Web/Grand.Web.AdminShared/Models/Reports/VendorPerformanceReportModel.cs`
- Modify: `src/Web/Grand.Web.Admin/Areas/Admin/Views/_ViewImports.cshtml`

- [ ] **Step 1: Create Models/Reports directory and CategoryRevenueReportModel**

```csharp
// src/Web/Grand.Web.AdminShared/Models/Reports/CategoryRevenueReportModel.cs
using Grand.Business.Core.Enums;
using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.AdminShared.Models.Reports;

public class CategoryRevenueReportModel : BaseModel
{
    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.StartDate")]
    [UIHint("DateNullable")]
    public DateTime? StartDate { get; set; }

    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.EndDate")]
    [UIHint("DateNullable")]
    public DateTime? EndDate { get; set; }

    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.OrderStatus")]
    public int OrderStatusId { get; set; }

    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.PaymentStatus")]
    public int PaymentStatusId { get; set; }

    public string StoreId { get; set; }

    public IList<SelectListItem> AvailableOrderStatuses { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailablePaymentStatuses { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();
}

public class CategoryRevenueReportLineModel : BaseModel
{
    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.Fields.CategoryName")]
    public string CategoryName { get; set; }

    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.Fields.TotalOrders")]
    public int TotalOrders { get; set; }

    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.Fields.TotalRevenue")]
    public string TotalRevenue { get; set; }

    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.Fields.RevenuePercent")]
    public string RevenuePercent { get; set; }
}
```

- [ ] **Step 2: Create NewVsReturningReportModel**

```csharp
// src/Web/Grand.Web.AdminShared/Models/Reports/NewVsReturningReportModel.cs
using Grand.Business.Core.Enums;
using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.AdminShared.Models.Reports;

public class NewVsReturningReportModel : BaseModel
{
    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.StartDate")]
    [UIHint("DateNullable")]
    public DateTime? StartDate { get; set; }

    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.EndDate")]
    [UIHint("DateNullable")]
    public DateTime? EndDate { get; set; }

    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.GroupBy")]
    public int GroupById { get; set; } = (int)ReportGroupBy.Month;

    public string StoreId { get; set; }

    public IList<SelectListItem> AvailableGroupByOptions { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();
}

public class NewVsReturningReportLineModel : BaseModel
{
    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.Fields.TimePeriod")]
    public string TimePeriod { get; set; }

    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.Fields.NewCustomers")]
    public int NewCustomers { get; set; }

    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.Fields.ReturningCustomers")]
    public int ReturningCustomers { get; set; }

    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.Fields.NewPercent")]
    public string NewPercent { get; set; }
}
```

- [ ] **Step 3: Create VendorPerformanceReportModel**

```csharp
// src/Web/Grand.Web.AdminShared/Models/Reports/VendorPerformanceReportModel.cs
using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Grand.Web.AdminShared.Models.Reports;

public class VendorPerformanceReportModel : BaseModel
{
    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.StartDate")]
    [UIHint("DateNullable")]
    public DateTime? StartDate { get; set; }

    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.EndDate")]
    [UIHint("DateNullable")]
    public DateTime? EndDate { get; set; }

    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.OrderStatus")]
    public int OrderStatusId { get; set; }

    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.PaymentStatus")]
    public int PaymentStatusId { get; set; }

    public IList<SelectListItem> AvailableOrderStatuses { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailablePaymentStatuses { get; set; } = new List<SelectListItem>();
}

public class VendorPerformanceReportLineModel : BaseModel
{
    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.Fields.VendorName")]
    public string VendorName { get; set; }

    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.Fields.TotalOrders")]
    public int TotalOrders { get; set; }

    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.Fields.TotalRevenue")]
    public string TotalRevenue { get; set; }

    [GrandResourceDisplayName("Admin.Reports.VendorPerformance.Fields.AverageOrderValue")]
    public string AverageOrderValue { get; set; }
}
```

- [ ] **Step 4: Add using to _ViewImports.cshtml**

Open `src/Web/Grand.Web.Admin/Areas/Admin/Views/_ViewImports.cshtml`.

After the last `@using Grand.Web.AdminShared.Models.*` line (currently `@using Grand.Web.AdminShared.Models.Vendors`), add:

```cshtml
@using Grand.Web.AdminShared.Models.Reports
```

- [ ] **Step 5: Build AdminShared to verify**

Run: `dotnet build src/Web/Grand.Web.AdminShared/Grand.Web.AdminShared.csproj`
Expected: Build succeeded, 0 errors

- [ ] **Step 6: Commit**

```bash
git add src/Web/Grand.Web.AdminShared/Models/Reports/ \
        src/Web/Grand.Web.Admin/Areas/Admin/Views/_ViewImports.cshtml
git commit -m "feat: add view models for three new reports and update ViewImports"
```

---

## Task 7: ReportsController — add new actions

**Files:**
- Modify: `src/Web/Grand.Web.Admin/Controllers/ReportsController.cs`

The controller already injects many services. You need to add `IVendorReportService`, `IVendorService` (for vendor name lookup), and `ICategoryService` (for category name lookup). It likely already injects `IStoreService` for store dropdowns.

- [ ] **Step 1: Add new constructor parameters**

Open `src/Web/Grand.Web.Admin/Controllers/ReportsController.cs`.

Find the constructor and add three new parameters. Also add the corresponding private fields and assignments. Add these using statements if missing:
```csharp
using Grand.Business.Core.Interfaces.System.Reports;
using Grand.Business.Core.Interfaces.Catalog.Categories;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Business.Core.Enums;
using Grand.Web.AdminShared.Models.Reports;
```

In the constructor signature, add:
```csharp
IVendorReportService vendorReportService,
IVendorService vendorService,
ICategoryService categoryService
```

Add corresponding private fields:
```csharp
private readonly IVendorReportService _vendorReportService;
private readonly IVendorService _vendorService;
private readonly ICategoryService _categoryService;
```

And in the constructor body:
```csharp
_vendorReportService = vendorReportService;
_vendorService = vendorService;
_categoryService = categoryService;
```

Note: `ICategoryService` is in `Grand.Business.Core.Interfaces.Catalog.Categories`. `IVendorService` is in `Grand.Business.Core.Interfaces.Customers`. Verify exact namespace by checking an existing using in the file.

- [ ] **Step 2: Add CategoryRevenueReport GET action**

Add inside the controller class:

```csharp
public async Task<IActionResult> CategoryRevenueReport()
{
    if (!await _permissionService.Authorize(StandardPermission.ManageOrders))
        return AccessDeniedView();

    var status = await _orderStatusService.GetAll();
    var model = new CategoryRevenueReportModel {
        AvailableOrderStatuses =
            status.Select(x => new SelectListItem { Value = x.StatusId.ToString(), Text = x.Name }).ToList()
    };
    model.AvailableOrderStatuses.Insert(0,
        new SelectListItem { Text = _translationService.GetResource("Admin.Common.All"), Value = "" });

    model.AvailablePaymentStatuses = _enumTranslationService.ToSelectList(PaymentStatus.Pending, false).ToList();
    model.AvailablePaymentStatuses.Insert(0,
        new SelectListItem { Text = _translationService.GetResource("Admin.Common.All"), Value = "" });

    var stores = await _storeService.GetAllStores();
    model.AvailableStores = stores.Select(s => new SelectListItem { Value = s.Id, Text = s.Shortcut }).ToList();
    model.AvailableStores.Insert(0,
        new SelectListItem { Text = _translationService.GetResource("Admin.Common.All"), Value = "" });

    return View(model);
}
```

Note: `_storeService` is already injected in the controller (it uses it elsewhere). Confirm by searching for `_storeService` in the file.

- [ ] **Step 3: Add CategoryRevenueReportList POST action**

```csharp
[HttpPost]
public async Task<IActionResult> CategoryRevenueReportList(DataSourceRequest command, CategoryRevenueReportModel model)
{
    DateTime? startDateValue = model.StartDate == null
        ? null
        : _dateTimeService.ConvertToUtcTime(model.StartDate.Value, _dateTimeService.CurrentTimeZone);
    DateTime? endDateValue = model.EndDate == null
        ? null
        : _dateTimeService.ConvertToUtcTime(model.EndDate.Value, _dateTimeService.CurrentTimeZone).AddDays(1);

    int? orderStatus = model.OrderStatusId > 0 ? model.OrderStatusId : null;
    var paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)model.PaymentStatusId : null;

    var items = await _orderReportService.GetCategoryRevenueReport(
        storeId: model.StoreId ?? "",
        startTimeUtc: startDateValue,
        endTimeUtc: endDateValue,
        os: orderStatus,
        ps: paymentStatus);

    var totalRevenue = items.Sum(x => x.TotalRevenue);
    var currency = await _currencyService.GetPrimaryStoreCurrency();

    var result = new List<CategoryRevenueReportLineModel>();
    foreach (var x in items)
    {
        var category = await _categoryService.GetCategoryById(x.CategoryId);
        result.Add(new CategoryRevenueReportLineModel {
            CategoryName = category?.Name ?? _translationService.GetResource("Admin.Common.Unknown"),
            TotalOrders = x.TotalOrders,
            TotalRevenue = _priceFormatter.FormatPrice(x.TotalRevenue, currency),
            RevenuePercent = totalRevenue > 0
                ? $"{(x.TotalRevenue / totalRevenue * 100):F1}%"
                : "0.0%"
        });
    }

    return Json(new DataSourceResult { Data = result, Total = result.Count });
}
```

- [ ] **Step 4: Add NewVsReturningReport GET action**

```csharp
public async Task<IActionResult> NewVsReturningReport()
{
    if (!await _permissionService.Authorize(StandardPermission.ManageCustomers))
        return AccessDeniedView();

    var model = new NewVsReturningReportModel {
        AvailableGroupByOptions = new List<SelectListItem> {
            new() { Value = "0", Text = _translationService.GetResource("Admin.Reports.NewVsReturning.GroupBy.Day") },
            new() { Value = "1", Text = _translationService.GetResource("Admin.Reports.NewVsReturning.GroupBy.Week") },
            new() { Value = "2", Text = _translationService.GetResource("Admin.Reports.NewVsReturning.GroupBy.Month"), Selected = true }
        }
    };

    var stores = await _storeService.GetAllStores();
    model.AvailableStores = stores.Select(s => new SelectListItem { Value = s.Id, Text = s.Shortcut }).ToList();
    model.AvailableStores.Insert(0,
        new SelectListItem { Text = _translationService.GetResource("Admin.Common.All"), Value = "" });

    return View(model);
}
```

- [ ] **Step 5: Add NewVsReturningReportList POST action**

```csharp
[HttpPost]
public async Task<IActionResult> NewVsReturningReportList(DataSourceRequest command, NewVsReturningReportModel model)
{
    DateTime? startDateValue = model.StartDate == null
        ? null
        : _dateTimeService.ConvertToUtcTime(model.StartDate.Value, _dateTimeService.CurrentTimeZone);
    DateTime? endDateValue = model.EndDate == null
        ? null
        : _dateTimeService.ConvertToUtcTime(model.EndDate.Value, _dateTimeService.CurrentTimeZone).AddDays(1);

    var groupBy = (ReportGroupBy)(model.GroupById);

    var items = await _customerReportService.GetNewVsReturningReport(
        storeId: model.StoreId ?? "",
        startTimeUtc: startDateValue,
        endTimeUtc: endDateValue,
        groupBy: groupBy);

    var result = items.Select(x =>
    {
        var total = x.NewCustomers + x.ReturningCustomers;
        return new NewVsReturningReportLineModel {
            TimePeriod = x.TimePeriod,
            NewCustomers = x.NewCustomers,
            ReturningCustomers = x.ReturningCustomers,
            NewPercent = total > 0 ? $"{(x.NewCustomers * 100.0 / total):F1}%" : "0.0%"
        };
    }).ToList();

    return Json(new DataSourceResult { Data = result, Total = result.Count });
}
```

- [ ] **Step 6: Add VendorPerformanceReport GET action**

```csharp
public async Task<IActionResult> VendorPerformanceReport()
{
    if (!await _permissionService.Authorize(StandardPermission.ManageOrders))
        return AccessDeniedView();

    var status = await _orderStatusService.GetAll();
    var model = new VendorPerformanceReportModel {
        AvailableOrderStatuses =
            status.Select(x => new SelectListItem { Value = x.StatusId.ToString(), Text = x.Name }).ToList()
    };
    model.AvailableOrderStatuses.Insert(0,
        new SelectListItem { Text = _translationService.GetResource("Admin.Common.All"), Value = "" });

    model.AvailablePaymentStatuses = _enumTranslationService.ToSelectList(PaymentStatus.Pending, false).ToList();
    model.AvailablePaymentStatuses.Insert(0,
        new SelectListItem { Text = _translationService.GetResource("Admin.Common.All"), Value = "" });

    return View(model);
}
```

- [ ] **Step 7: Add VendorPerformanceReportList POST action**

```csharp
[HttpPost]
public async Task<IActionResult> VendorPerformanceReportList(DataSourceRequest command, VendorPerformanceReportModel model)
{
    DateTime? startDateValue = model.StartDate == null
        ? null
        : _dateTimeService.ConvertToUtcTime(model.StartDate.Value, _dateTimeService.CurrentTimeZone);
    DateTime? endDateValue = model.EndDate == null
        ? null
        : _dateTimeService.ConvertToUtcTime(model.EndDate.Value, _dateTimeService.CurrentTimeZone).AddDays(1);

    int? orderStatus = model.OrderStatusId > 0 ? model.OrderStatusId : null;
    var paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)model.PaymentStatusId : null;

    var items = await _vendorReportService.GetVendorPerformanceReport(
        startTimeUtc: startDateValue,
        endTimeUtc: endDateValue,
        os: orderStatus,
        ps: paymentStatus);

    var currency = await _currencyService.GetPrimaryStoreCurrency();

    var result = new List<VendorPerformanceReportLineModel>();
    foreach (var x in items)
    {
        var vendor = await _vendorService.GetVendorById(x.VendorId);
        result.Add(new VendorPerformanceReportLineModel {
            VendorName = vendor?.Name ?? _translationService.GetResource("Admin.Common.Unknown"),
            TotalOrders = x.TotalOrders,
            TotalRevenue = _priceFormatter.FormatPrice(x.TotalRevenue, currency),
            AverageOrderValue = _priceFormatter.FormatPrice(x.AverageOrderValue, currency)
        });
    }

    return Json(new DataSourceResult { Data = result, Total = result.Count });
}
```

- [ ] **Step 8: Build to verify**

Run: `dotnet build src/Web/Grand.Web.Admin/Grand.Web.Admin.csproj`
Expected: Build succeeded, 0 errors

If `ICategoryService` namespace is wrong, search the file for existing catalog service usings and adjust accordingly.

- [ ] **Step 9: Commit**

```bash
git add src/Web/Grand.Web.Admin/Controllers/ReportsController.cs
git commit -m "feat: add controller actions for three new reports"
```

---

## Task 8: Razor views

**Files:**
- Create: `src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/CategoryRevenueReport.cshtml`
- Create: `src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/NewVsReturningReport.cshtml`
- Create: `src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/VendorPerformanceReport.cshtml`

All views follow the `CountryReport.cshtml` pattern exactly — same HTML structure, filter section, Kendo Grid, and JS.

- [ ] **Step 1: Create CategoryRevenueReport.cshtml**

```cshtml
@model CategoryRevenueReportModel
@{
    ViewBag.Title = Loc["Admin.Reports.CategoryRevenue"];
}

<div class="row">
    <div class="col-md-12">
        <div class="x_panel light form-fit popup-window">
            <div class="x_title">
                <div class="caption level-caption">
                    <i class="fa fa-list-alt"></i>
                    @Loc["Admin.Reports.CategoryRevenue"]
                </div>
            </div>
            <div class="x_content form">
                <div class="form-horizontal">
                    <div class="form-body">
                        <div class="x_content">
                            <div class="form-horizontal">
                                <div class="form-body">
                                    <div class="main-header col-12 px-0">
                                        <div class="row align-items-end">
                                            <div class="col-md-3 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="StartDate" class="control-label"/>
                                                    <admin-input asp-for="StartDate"/>
                                                </div>
                                            </div>
                                            <div class="col-md-3 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="EndDate" class="control-label"/>
                                                    <admin-input asp-for="EndDate"/>
                                                </div>
                                            </div>
                                            <div class="col-md-3 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="StoreId" class="control-label"/>
                                                    <admin-select asp-for="StoreId" asp-items="Model.AvailableStores"/>
                                                </div>
                                            </div>
                                            <div class="col-md-3 col-sm-12 col-12">
                                                <div class="form-actions">
                                                    <div class="btn-group">
                                                        <button class="btn btn-success filter-submit" id="search-categoryrevenue">
                                                            <i class="fa fa-search"></i> @Loc["Admin.Reports.CategoryRevenue.RunReport"]
                                                        </button>
                                                        <button class="btn btn-info" type="button" data-toggle="collapse" data-target="#filterCollapse" aria-expanded="false" aria-controls="filterCollapse">
                                                            <i class="fa fa-filter"></i>&nbsp; @Loc["Admin.Common.Filters"]
                                                        </button>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="collapse" id="filterCollapse">
                                        <div class="drop-filters-container w-100">
                                            <div class="form-group">
                                                <admin-label asp-for="OrderStatusId" class="control-label col-md-3 col-sm-3"/>
                                                <div class="col-md-9 col-sm-9">
                                                    <admin-select asp-for="OrderStatusId" asp-items="Model.AvailableOrderStatuses"/>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <admin-label asp-for="PaymentStatusId" class="control-label col-md-3 col-sm-3"/>
                                                <div class="col-md-9 col-sm-9">
                                                    <admin-select asp-for="PaymentStatusId" asp-items="Model.AvailablePaymentStatuses"/>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="x_content">
                                    <div id="categoryrevenue-grid"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<script>
    $(document).ready(function () {
        $("#categoryrevenue-grid").kendoGrid({
            dataSource: {
                transport: {
                    read: {
                        url: "@Html.Raw(Url.Action("CategoryRevenueReportList", "Reports", new { area = Constants.AreaAdmin }))",
                        type: "POST",
                        dataType: "json",
                        data: additionalData
                    }
                },
                schema: { data: "Data", total: "Total", errors: "Errors" },
                error: function(e) { display_kendoui_grid_error(e); this.cancelChanges(); },
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true
            },
            pageable: { refresh: true, numeric: false, previousNext: false, info: false },
            scrollable: false,
            columns: [
                { field: "CategoryName", title: "@Loc["Admin.Reports.CategoryRevenue.Fields.CategoryName"]" },
                { field: "TotalOrders", title: "@Loc["Admin.Reports.CategoryRevenue.Fields.TotalOrders"]" },
                { field: "TotalRevenue", title: "@Loc["Admin.Reports.CategoryRevenue.Fields.TotalRevenue"]" },
                { field: "RevenuePercent", title: "@Loc["Admin.Reports.CategoryRevenue.Fields.RevenuePercent"]" }
            ]
        });

        $('#search-categoryrevenue').click(function () {
            $('#categoryrevenue-grid').data('kendoGrid').dataSource.read();
            return false;
        });
    });

    function additionalData() {
        var data = {
            StartDate: $('#@Html.IdFor(model => model.StartDate)').val(),
            EndDate: $('#@Html.IdFor(model => model.EndDate)').val(),
            StoreId: $('#StoreId').val(),
            OrderStatusId: $('#OrderStatusId').val(),
            PaymentStatusId: $('#PaymentStatusId').val()
        };
        addAntiForgeryToken(data);
        return data;
    }
</script>
```

- [ ] **Step 2: Create NewVsReturningReport.cshtml**

```cshtml
@model NewVsReturningReportModel
@{
    ViewBag.Title = Loc["Admin.Reports.NewVsReturning"];
}

<div class="row">
    <div class="col-md-12">
        <div class="x_panel light form-fit popup-window">
            <div class="x_title">
                <div class="caption level-caption">
                    <i class="fa fa-list-alt"></i>
                    @Loc["Admin.Reports.NewVsReturning"]
                </div>
            </div>
            <div class="x_content form">
                <div class="form-horizontal">
                    <div class="form-body">
                        <div class="x_content">
                            <div class="form-horizontal">
                                <div class="form-body">
                                    <div class="main-header col-12 px-0">
                                        <div class="row align-items-end">
                                            <div class="col-md-3 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="StartDate" class="control-label"/>
                                                    <admin-input asp-for="StartDate"/>
                                                </div>
                                            </div>
                                            <div class="col-md-3 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="EndDate" class="control-label"/>
                                                    <admin-input asp-for="EndDate"/>
                                                </div>
                                            </div>
                                            <div class="col-md-2 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="GroupById" class="control-label"/>
                                                    <admin-select asp-for="GroupById" asp-items="Model.AvailableGroupByOptions"/>
                                                </div>
                                            </div>
                                            <div class="col-md-2 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="StoreId" class="control-label"/>
                                                    <admin-select asp-for="StoreId" asp-items="Model.AvailableStores"/>
                                                </div>
                                            </div>
                                            <div class="col-md-2 col-sm-12 col-12">
                                                <div class="form-actions">
                                                    <button class="btn btn-success filter-submit" id="search-newvsreturning">
                                                        <i class="fa fa-search"></i> @Loc["Admin.Reports.NewVsReturning.RunReport"]
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="x_content">
                                    <div id="newvsreturning-grid"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<script>
    $(document).ready(function () {
        $("#newvsreturning-grid").kendoGrid({
            dataSource: {
                transport: {
                    read: {
                        url: "@Html.Raw(Url.Action("NewVsReturningReportList", "Reports", new { area = Constants.AreaAdmin }))",
                        type: "POST",
                        dataType: "json",
                        data: additionalData
                    }
                },
                schema: { data: "Data", total: "Total", errors: "Errors" },
                error: function(e) { display_kendoui_grid_error(e); this.cancelChanges(); },
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true
            },
            pageable: { refresh: true, numeric: false, previousNext: false, info: false },
            scrollable: false,
            columns: [
                { field: "TimePeriod", title: "@Loc["Admin.Reports.NewVsReturning.Fields.TimePeriod"]" },
                { field: "NewCustomers", title: "@Loc["Admin.Reports.NewVsReturning.Fields.NewCustomers"]" },
                { field: "ReturningCustomers", title: "@Loc["Admin.Reports.NewVsReturning.Fields.ReturningCustomers"]" },
                { field: "NewPercent", title: "@Loc["Admin.Reports.NewVsReturning.Fields.NewPercent"]" }
            ]
        });

        $('#search-newvsreturning').click(function () {
            $('#newvsreturning-grid').data('kendoGrid').dataSource.read();
            return false;
        });
    });

    function additionalData() {
        var data = {
            StartDate: $('#@Html.IdFor(model => model.StartDate)').val(),
            EndDate: $('#@Html.IdFor(model => model.EndDate)').val(),
            GroupById: $('#GroupById').val(),
            StoreId: $('#StoreId').val()
        };
        addAntiForgeryToken(data);
        return data;
    }
</script>
```

- [ ] **Step 3: Create VendorPerformanceReport.cshtml**

```cshtml
@model VendorPerformanceReportModel
@{
    ViewBag.Title = Loc["Admin.Reports.VendorPerformance"];
}

<div class="row">
    <div class="col-md-12">
        <div class="x_panel light form-fit popup-window">
            <div class="x_title">
                <div class="caption level-caption">
                    <i class="fa fa-list-alt"></i>
                    @Loc["Admin.Reports.VendorPerformance"]
                </div>
            </div>
            <div class="x_content form">
                <div class="form-horizontal">
                    <div class="form-body">
                        <div class="x_content">
                            <div class="form-horizontal">
                                <div class="form-body">
                                    <div class="main-header col-12 px-0">
                                        <div class="row align-items-end">
                                            <div class="col-md-4 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="StartDate" class="control-label"/>
                                                    <admin-input asp-for="StartDate"/>
                                                </div>
                                            </div>
                                            <div class="col-md-4 col-ms-12 col-12">
                                                <div class="form-group mb-0">
                                                    <admin-label asp-for="EndDate" class="control-label"/>
                                                    <admin-input asp-for="EndDate"/>
                                                </div>
                                            </div>
                                            <div class="col-md-4 col-sm-12 col-12">
                                                <div class="form-actions">
                                                    <div class="btn-group">
                                                        <button class="btn btn-success filter-submit" id="search-vendorperformance">
                                                            <i class="fa fa-search"></i> @Loc["Admin.Reports.VendorPerformance.RunReport"]
                                                        </button>
                                                        <button class="btn btn-info" type="button" data-toggle="collapse" data-target="#filterCollapse" aria-expanded="false" aria-controls="filterCollapse">
                                                            <i class="fa fa-filter"></i>&nbsp; @Loc["Admin.Common.Filters"]
                                                        </button>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="collapse" id="filterCollapse">
                                        <div class="drop-filters-container w-100">
                                            <div class="form-group">
                                                <admin-label asp-for="OrderStatusId" class="control-label col-md-3 col-sm-3"/>
                                                <div class="col-md-9 col-sm-9">
                                                    <admin-select asp-for="OrderStatusId" asp-items="Model.AvailableOrderStatuses"/>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <admin-label asp-for="PaymentStatusId" class="control-label col-md-3 col-sm-3"/>
                                                <div class="col-md-9 col-sm-9">
                                                    <admin-select asp-for="PaymentStatusId" asp-items="Model.AvailablePaymentStatuses"/>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="x_content">
                                    <div id="vendorperformance-grid"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<script>
    $(document).ready(function () {
        $("#vendorperformance-grid").kendoGrid({
            dataSource: {
                transport: {
                    read: {
                        url: "@Html.Raw(Url.Action("VendorPerformanceReportList", "Reports", new { area = Constants.AreaAdmin }))",
                        type: "POST",
                        dataType: "json",
                        data: additionalData
                    }
                },
                schema: { data: "Data", total: "Total", errors: "Errors" },
                error: function(e) { display_kendoui_grid_error(e); this.cancelChanges(); },
                serverPaging: true,
                serverFiltering: true,
                serverSorting: true
            },
            pageable: { refresh: true, numeric: false, previousNext: false, info: false },
            scrollable: false,
            columns: [
                { field: "VendorName", title: "@Loc["Admin.Reports.VendorPerformance.Fields.VendorName"]" },
                { field: "TotalOrders", title: "@Loc["Admin.Reports.VendorPerformance.Fields.TotalOrders"]" },
                { field: "TotalRevenue", title: "@Loc["Admin.Reports.VendorPerformance.Fields.TotalRevenue"]" },
                { field: "AverageOrderValue", title: "@Loc["Admin.Reports.VendorPerformance.Fields.AverageOrderValue"]" }
            ]
        });

        $('#search-vendorperformance').click(function () {
            $('#vendorperformance-grid').data('kendoGrid').dataSource.read();
            return false;
        });
    });

    function additionalData() {
        var data = {
            StartDate: $('#@Html.IdFor(model => model.StartDate)').val(),
            EndDate: $('#@Html.IdFor(model => model.EndDate)').val(),
            OrderStatusId: $('#OrderStatusId').val(),
            PaymentStatusId: $('#PaymentStatusId').val()
        };
        addAntiForgeryToken(data);
        return data;
    }
</script>
```

- [ ] **Step 4: Build to verify views compile**

Run: `dotnet build src/Web/Grand.Web.Admin/Grand.Web.Admin.csproj`
Expected: Build succeeded, 0 errors

- [ ] **Step 5: Commit**

```bash
git add src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/CategoryRevenueReport.cshtml \
        src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/NewVsReturningReport.cshtml \
        src/Web/Grand.Web.Admin/Areas/Admin/Views/Reports/VendorPerformanceReport.cshtml
git commit -m "feat: add Razor views for three new reports"
```

---

## Task 9: Navigation menu entries

**Files:**
- Modify: `src/Modules/Grand.Module.Installer/Utilities/StandardAdminSiteMap.cs`

- [ ] **Step 1: Add three navigation entries**

Open `src/Modules/Grand.Module.Installer/Utilities/StandardAdminSiteMap.cs`.

Find the Reports `ChildNodes` list. It currently ends with a `Customer reports` entry at `DisplayOrder = 4`. Add three new entries after it (adjust the closing list syntax as needed):

```csharp
new() {
    SystemName = "Category revenue report",
    ResourceName = "Admin.Reports.CategoryRevenue",
    PermissionNames = new List<string> { PermissionSystemName.Reports },
    ControllerName = "Reports",
    ActionName = "CategoryRevenueReport",
    DisplayOrder = 5,
    IconClass = "fa fa-dot-circle-o"
},
new() {
    SystemName = "New vs returning customers report",
    ResourceName = "Admin.Reports.NewVsReturning",
    PermissionNames = new List<string> { PermissionSystemName.Reports },
    ControllerName = "Reports",
    ActionName = "NewVsReturningReport",
    DisplayOrder = 6,
    IconClass = "fa fa-dot-circle-o"
},
new() {
    SystemName = "Vendor performance report",
    ResourceName = "Admin.Reports.VendorPerformance",
    PermissionNames = new List<string> { PermissionSystemName.Reports },
    ControllerName = "Reports",
    ActionName = "VendorPerformanceReport",
    DisplayOrder = 7,
    IconClass = "fa fa-dot-circle-o"
},
```

- [ ] **Step 2: Build the full solution to verify everything compiles**

Run: `dotnet build GrandNode.sln`
Expected: Build succeeded, 0 errors, 0 warnings (or only pre-existing warnings)

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Grand.Module.Installer/Utilities/StandardAdminSiteMap.cs
git commit -m "feat: add navigation entries for three new reports"
```

---

## Done

All three reports — Revenue by Category, New vs. Returning Customers, and Vendor Performance — are now accessible from the admin Reports menu. No new tracking infrastructure was added; all data comes from existing MongoDB collections.
