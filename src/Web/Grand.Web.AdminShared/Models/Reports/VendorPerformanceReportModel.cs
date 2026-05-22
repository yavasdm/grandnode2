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

    public string StoreId { get; set; }

    public IList<SelectListItem> AvailableOrderStatuses { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailablePaymentStatuses { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableStores { get; set; } = new List<SelectListItem>();
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
