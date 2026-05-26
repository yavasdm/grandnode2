using Grand.Data;
using Grand.Domain.Orders;
using Grand.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grand.Module.Migration.Migrations._2._5;

public class MigrationSeedReportDataV5 : IMigration
{
    public int Priority => 50;
    public DbVersion Version => new(2, 5);
    public Guid Identity => new("A0B1C2D3-E4F5-4A6B-7C8D-9E0F1A2B3C4D");
    public string Name => "Fix seed report orders: set CurrencyRate and Rate to 1";

    public bool UpgradeProcess(IServiceProvider serviceProvider)
    {
        var orderRepository = serviceProvider.GetRequiredService<IRepository<Order>>();
        var logService = serviceProvider.GetRequiredService<ILogger<MigrationSeedReportDataV5>>();

        try
        {
            var seedOrders = orderRepository.Table
                .Where(o => o.Code == "SEED-REPORT" && o.CurrencyRate == 0)
                .ToList();

            if (!seedOrders.Any())
                return true;

            foreach (var order in seedOrders)
            {
                order.CurrencyRate = 1;
                order.Rate = 1;
                orderRepository.Update(order);
            }

            logService.LogInformation("MigrationSeedReportDataV5: fixed {Count} seed orders", seedOrders.Count);
        }
        catch (Exception ex)
        {
            logService.LogError(ex, "UpgradeProcess - MigrationSeedReportDataV5");
        }

        return true;
    }
}
