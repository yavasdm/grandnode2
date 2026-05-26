using Grand.Data;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using Grand.Domain.Shipping;
using Grand.Domain.Vendors;
using Grand.Infrastructure.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Grand.Module.Migration.Migrations._2._5;

public class MigrationSeedReportDataV3 : IMigration
{
    public int Priority => 30;
    public DbVersion Version => new(2, 5);
    public Guid Identity => new("C2D3E4F5-A6B7-4C8D-9E0F-1A2B3C4D5E6F");
    public string Name => "Seed sample orders for reports V3";

    private const string SeedCode = "SEED-REPORT";
    private const string SeedMarker = "seed-report-marker";

    public bool UpgradeProcess(IServiceProvider serviceProvider)
    {
        var orderRepository = serviceProvider.GetRequiredService<IRepository<Order>>();
        var productRepository = serviceProvider.GetRequiredService<IRepository<Product>>();
        var categoryRepository = serviceProvider.GetRequiredService<IRepository<Category>>();
        var vendorRepository = serviceProvider.GetRequiredService<IRepository<Vendor>>();
        var logService = serviceProvider.GetRequiredService<ILogger<MigrationSeedReportDataV3>>();

        try
        {
            if (orderRepository.Table.Any(o => o.Code == SeedCode))
                return true;

            // Ensure seed categories exist
            var cats = EnsureSeedCategories(categoryRepository);
            var vens = EnsureSeedVendors(vendorRepository);
            var prods = EnsureSeedProducts(productRepository, cats, vens);

            var rng = new Random(42);
            var now = DateTime.UtcNow;
            var maxOrderNumber = orderRepository.Table.Select(o => (int?)o.OrderNumber).Max();
            var orderNumber = (maxOrderNumber ?? 0) + 1;

            // Synthetic customer IDs — 8 distinct buyers
            var customerIds = Enumerable.Range(1, 8)
                .Select(i => $"seed-customer-{i:D3}")
                .ToList();

            for (var i = 0; i < 60; i++)
            {
                var daysBack = rng.Next(1, 180);
                var createdOn = now.AddDays(-daysBack);
                var customerId = customerIds[rng.Next(customerIds.Count)];
                var itemCount = rng.Next(1, 4);
                var orderItems = new List<OrderItem>();

                for (var j = 0; j < itemCount; j++)
                {
                    var product = prods[rng.Next(prods.Count)];
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
                    Code = SeedCode,
                    StoreId = "",
                    CustomerId = customerId,
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
                        FirstName = "Seed",
                        LastName = "Customer",
                        Email = $"{customerId}@seed.example"
                    },
                    CreatedOnUtc = createdOn
                };

                foreach (var item in orderItems)
                    order.OrderItems.Add(item);

                orderRepository.Insert(order);
            }

            logService.LogInformation("MigrationSeedReportDataV3: inserted 60 seed orders");
        }
        catch (Exception ex)
        {
            logService.LogError(ex, "UpgradeProcess - MigrationSeedReportDataV3");
        }

        return true;
    }

    private static List<Category> EnsureSeedCategories(IRepository<Category> repo)
    {
        var names = new[] { "Seed-Electronics", "Seed-Clothing", "Seed-Books", "Seed-Home", "Seed-Sports" };
        var existing = repo.Table.Where(c => c.Name.StartsWith("Seed-")).ToList();
        var result = new List<Category>(existing);

        foreach (var name in names)
        {
            if (result.Any(c => c.Name == name)) continue;
            var cat = new Category { Name = name, Published = true, DisplayOrder = 99, SeName = name.ToLower().Replace("-", "") };
            repo.Insert(cat);
            result.Add(cat);
        }

        return result;
    }

    private static List<Vendor> EnsureSeedVendors(IRepository<Vendor> repo)
    {
        var names = new[] { "Seed-Vendor-Alpha", "Seed-Vendor-Beta", "Seed-Vendor-Gamma" };
        var existing = repo.Table.Where(v => v.Name.StartsWith("Seed-Vendor-")).ToList();
        var result = new List<Vendor>(existing);

        foreach (var name in names)
        {
            if (result.Any(v => v.Name == name)) continue;
            var vendor = new Vendor { Name = name, Active = true, SeName = name.ToLower().Replace("-", "") };
            repo.Insert(vendor);
            result.Add(vendor);
        }

        return result;
    }

    private static List<Product> EnsureSeedProducts(
        IRepository<Product> repo,
        List<Category> categories,
        List<Vendor> vendors)
    {
        var productDefs = new[] {
            ("Seed-Laptop Pro", 999.0, 0),
            ("Seed-Wireless Headphones", 149.0, 0),
            ("Seed-Cotton T-Shirt", 29.0, 1),
            ("Seed-Running Shoes", 89.0, 1),
            ("Seed-Python Programming Book", 49.0, 2),
            ("Seed-Coffee Maker", 79.0, 3),
            ("Seed-Yoga Mat", 39.0, 4),
            ("Seed-Smart Watch", 299.0, 0),
            ("Seed-Desk Lamp", 45.0, 3),
            ("Seed-Novel Collection", 35.0, 2),
        };

        var existing = repo.Table.Where(p => p.Name.StartsWith("Seed-")).ToList();
        var result = new List<Product>(existing);

        for (var i = 0; i < productDefs.Length; i++)
        {
            var (name, price, catIndex) = productDefs[i];
            if (result.Any(p => p.Name == name)) continue;

            var product = new Product {
                Name = name,
                Price = price,
                Published = true,
                ProductTypeId = ProductType.SimpleProduct,
                SeName = name.ToLower().Replace(" ", "-").Replace("-seed-", "seed-"),
                VendorId = vendors.Count > 0 ? vendors[i % vendors.Count].Id : "",
            };

            if (categories.Count > 0)
            {
                product.ProductCategories.Add(new ProductCategory {
                    CategoryId = categories[catIndex % categories.Count].Id,
                    DisplayOrder = 1
                });
            }

            repo.Insert(product);
            result.Add(product);
        }

        return result;
    }
}
