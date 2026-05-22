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
