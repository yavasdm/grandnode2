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

    [GrandResourceDisplayName("Admin.Reports.NewVsReturning.Store")]
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
