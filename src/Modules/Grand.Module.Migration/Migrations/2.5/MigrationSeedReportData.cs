using Grand.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Customers;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Vendors;
using Grand.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grand.Module.Migration.Migrations._2._5;

public class MigrationSeedReportData : IMigration
{
    public int Priority => 10;
    public DbVersion Version => new(2, 5);
    public Guid Identity => new("F2A85C34-7E19-4B62-DC08-93A1E74F6B20");
    public string Name => "Seed sample orders for reports";

    public bool UpgradeProcess(IServiceProvider serviceProvider)
    {
        var orderRepository = serviceProvider.GetRequiredService<IRepository<Order>>();
        var productRepository = serviceProvider.GetRequiredService<IRepository<Product>>();
        var categoryRepository = serviceProvider.GetRequiredService<IRepository<Category>>();
        var vendorRepository = serviceProvider.GetRequiredService<IRepository<Vendor>>();
        var customerRepository = serviceProvider.GetRequiredService<IRepository<Customer>>();
        var logService = serviceProvider.GetRequiredService<ILogger<MigrationSeedReportData>>();

        try
        {
            // Skip if sample orders already exist
            if (orderRepository.Table.Any(o => o.Code == "SEED-REPORT"))
                return true;

            var storeId = "";
            var categories = categoryRepository.Table.Where(c => c.Published).Take(5).ToList();
            var vendors = vendorRepository.Table.Where(v => v.Active && !v.Deleted).Take(3).ToList();
            var customers = customerRepository.Table
                .Where(c => !c.Deleted && c.Active && !c.IsSystemAccount && c.Email != null)
                .Take(10).ToList();

            if (!customers.Any() || !categories.Any())
                return true;

            // Ensure we have products with categories and vendors assigned
            var products = EnsureProducts(productRepository, categories, vendors);

            var rng = new Random(42);
            var now = DateTime.UtcNow;
            var maxOrderNumber = orderRepository.Table.Select(o => (int?)o.OrderNumber).Max();
            var orderNumber = (maxOrderNumber ?? 0) + 1;

            // Spread 60 orders across the last 6 months to give reports meaningful data
            for (var i = 0; i < 60; i++)
            {
                var daysBack = rng.Next(1, 180);
                var createdOn = now.AddDays(-daysBack);
                var customer = customers[rng.Next(customers.Count)];
                var itemCount = rng.Next(1, 4);
                var orderItems = new List<OrderItem>();

                for (var j = 0; j < itemCount; j++)
                {
                    var product = products[rng.Next(products.Count)];
                    var qty = rng.Next(1, 4);
                    var price = Math.Round(product.Price * qty, 2);
                    orderItems.Add(new OrderItem {
                        ProductId = product.Id,
                        VendorId = product.VendorId ?? "",
                        Quantity = qty,
                        UnitPriceExclTax = product.Price,
                        UnitPriceInclTax = product.Price,
                        PriceExclTax = price,
                        PriceInclTax = price,
                        CreatedOnUtc = createdOn
                    });
                }

                var orderTotal = orderItems.Sum(x => x.PriceExclTax);
                var order = new Order {
                    Code = "SEED-REPORT",
                    StoreId = storeId,
                    CustomerId = customer.Id,
                    OrderGuid = Guid.NewGuid(),
                    OrderNumber = orderNumber++,
                    OrderStatusId = (int)OrderStatusSystem.Complete,
                    PaymentStatusId = PaymentStatus.Paid,
                    ShippingStatusId = ShippingStatus.Shipped,
                    CustomerCurrencyCode = "USD",
                    PrimaryCurrencyCode = "USD",
                    OrderSubtotalExclTax = orderTotal,
                    OrderSubtotalInclTax = orderTotal,
                    OrderTotal = orderTotal,
                    BillingAddress = new Address {
                        FirstName = "Sample",
                        LastName = "Customer",
                        Email = customer.Email ?? "sample@example.com"
                    },
                    CreatedOnUtc = createdOn
                };

                foreach (var item in orderItems)
                    order.OrderItems.Add(item);

                orderRepository.Insert(order);
            }
        }
        catch (Exception ex)
        {
            logService.LogError(ex, "UpgradeProcess - MigrationSeedReportData");
        }

        return true;
    }

    private static List<Product> EnsureProducts(
        IRepository<Product> productRepository,
        List<Category> categories,
        List<Vendor> vendors)
    {
        var existing = productRepository.Table
            .Where(p => p.Published && p.Price > 0)
            .Take(20).ToList();

        // Assign categories and vendors to existing products that lack them
        foreach (var product in existing)
        {
            var changed = false;

            if (!product.ProductCategories.Any() && categories.Any())
            {
                product.ProductCategories.Add(new ProductCategory {
                    CategoryId = categories[Math.Abs(product.Id.GetHashCode()) % categories.Count].Id,
                    DisplayOrder = 1
                });
                changed = true;
            }

            if (string.IsNullOrEmpty(product.VendorId) && vendors.Any())
            {
                product.VendorId = vendors[Math.Abs(product.Id.GetHashCode()) % vendors.Count].Id;
                changed = true;
            }

            if (changed)
                productRepository.Update(product);
        }

        return existing.Any() ? existing : new List<Product>();
    }
}
