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

    [GrandResourceDisplayName("Admin.Reports.CategoryRevenue.Store")]
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
