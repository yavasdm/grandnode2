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

public class MigrationSeedReportDataV2 : IMigration
{
    public int Priority => 20;
    public DbVersion Version => new(2, 5);
    public Guid Identity => new("D4E5F6A7-B8C9-4D0E-1F2A-3B4C5D6E7F81");
    public string Name => "Seed sample orders for reports V2";

    public bool UpgradeProcess(IServiceProvider serviceProvider)
    {
        var orderRepository = serviceProvider.GetRequiredService<IRepository<Order>>();
        var productRepository = serviceProvider.GetRequiredService<IRepository<Product>>();
        var categoryRepository = serviceProvider.GetRequiredService<IRepository<Category>>();
        var vendorRepository = serviceProvider.GetRequiredService<IRepository<Vendor>>();
        var customerRepository = serviceProvider.GetRequiredService<IRepository<Customer>>();
        var logService = serviceProvider.GetRequiredService<ILogger<MigrationSeedReportDataV2>>();

        try
        {
            if (orderRepository.Table.Any(o => o.Code == "SEED-REPORT"))
                return true;

            var storeId = "";
            // Relaxed filters: accept any category/vendor/customer that is not deleted
            var categories = categoryRepository.Table.Take(5).ToList();
            var vendors = vendorRepository.Table.Where(v => !v.Deleted).Take(3).ToList();
            var customers = customerRepository.Table
                .Where(c => !c.Deleted && !c.IsSystemAccount)
                .Take(10).ToList();

            // Fall back to any non-deleted customer if no regular ones found
            if (!customers.Any())
                customers = customerRepository.Table.Where(c => !c.Deleted).Take(10).ToList();

            var products = productRepository.Table
                .Where(p => p.Published && p.Price > 0)
                .Take(20).ToList();

            if (!products.Any() || !customers.Any())
                return true;

            foreach (var product in products)
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

            var rng = new Random(42);
            var now = DateTime.UtcNow;
            var maxOrderNumber = orderRepository.Table.Select(o => (int?)o.OrderNumber).Max();
            var orderNumber = (maxOrderNumber ?? 0) + 1;

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
            logService.LogError(ex, "UpgradeProcess - MigrationSeedReportDataV2");
        }

        return true;
    }
}
