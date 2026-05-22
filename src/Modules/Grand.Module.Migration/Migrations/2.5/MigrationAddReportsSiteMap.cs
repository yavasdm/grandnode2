using Grand.Data;
using Grand.Domain.Admin;
using Grand.Domain.Permissions;
using Grand.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grand.Module.Migration.Migrations._2._5;

public class MigrationAddReportsSiteMap : IMigration
{
    public int Priority => 0;
    public DbVersion Version => new(2, 5);
    public Guid Identity => new("B7E4C912-3A1F-4D28-9E6B-05F8A2C73D41");
    public string Name => "Add Category Revenue, New vs Returning, and Vendor Performance report menu entries";

    public bool UpgradeProcess(IServiceProvider serviceProvider)
    {
        var repository = serviceProvider.GetRequiredService<IRepository<AdminSiteMap>>();
        var logService = serviceProvider.GetRequiredService<ILogger<MigrationAddReportsSiteMap>>();

        try
        {
            var sitemapReports = repository.Table.FirstOrDefault(x => x.SystemName == "Reports");
            if (sitemapReports == null)
                return true;

            if (sitemapReports.ChildNodes.All(x => x.SystemName != "Category revenue report"))
                sitemapReports.ChildNodes.Add(new AdminSiteMap {
                    SystemName = "Category revenue report",
                    ResourceName = "Admin.Reports.CategoryRevenue",
                    PermissionNames = new List<string> { PermissionSystemName.Reports },
                    ControllerName = "Reports",
                    ActionName = "CategoryRevenueReport",
                    DisplayOrder = 5,
                    IconClass = "fa fa-dot-circle-o"
                });

            if (sitemapReports.ChildNodes.All(x => x.SystemName != "New vs returning customers report"))
                sitemapReports.ChildNodes.Add(new AdminSiteMap {
                    SystemName = "New vs returning customers report",
                    ResourceName = "Admin.Reports.NewVsReturning",
                    PermissionNames = new List<string> { PermissionSystemName.Reports },
                    ControllerName = "Reports",
                    ActionName = "NewVsReturningReport",
                    DisplayOrder = 6,
                    IconClass = "fa fa-dot-circle-o"
                });

            if (sitemapReports.ChildNodes.All(x => x.SystemName != "Vendor performance report"))
                sitemapReports.ChildNodes.Add(new AdminSiteMap {
                    SystemName = "Vendor performance report",
                    ResourceName = "Admin.Reports.VendorPerformance",
                    PermissionNames = new List<string> { PermissionSystemName.Reports },
                    ControllerName = "Reports",
                    ActionName = "VendorPerformanceReport",
                    DisplayOrder = 7,
                    IconClass = "fa fa-dot-circle-o"
                });

            repository.Update(sitemapReports);
        }
        catch (Exception ex)
        {
            logService.LogError(ex, "UpgradeProcess - MigrationAddReportsSiteMap");
        }

        return true;
    }
}
