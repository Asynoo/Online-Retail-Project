using ProductApi.Models;
namespace ProductApi.Data {
    public class DbInitializer : IDbInitializer {
        // This method will create and seed the database.
        public void Initialize(ProductApiContext context) {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Products.Any()) {
                return; // DB has been seeded
            }

            var products = new List<Product> {
                new() { Name = "Hammer", Price = 100, ItemsInStock = 10, ItemsReserved = 3 },
                new() { Name = "Screwdriver", Price = 70, ItemsInStock = 20, ItemsReserved = 2 },
                new() { Name = "Drill", Price = 500, ItemsInStock = 20, ItemsReserved = 0 },
                new() { Name = "Wrench", Price = 120, ItemsInStock = 11, ItemsReserved = 0 },
                new() { Name = "Bandsaw", Price = 1200, ItemsInStock = 5, ItemsReserved = 0 },
                new() { Name = "Nail", Price = 1, ItemsInStock = 200000, ItemsReserved = 7000 },
                new() { Name = "Hacksaw", Price = 250, ItemsInStock = 25, ItemsReserved = 7 },
                new() { Name = "Nailgun", Price = 2100, ItemsInStock = 0, ItemsReserved = 4 }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
