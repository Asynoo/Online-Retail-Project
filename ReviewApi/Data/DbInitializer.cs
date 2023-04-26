using ReviewApi.Models;
namespace ReviewApi.Data {
    public class DbInitializer : IDbInitializer {
        // This method will create and seed the database.
        public void Initialize(ReviewApiContext context) {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Reviews.Any()) {
                return; // DB has been seeded
            }

            List<Review> reviews = new List<Review> {
                //TODO: Seed some reviews
                //new() { Name = "Hammer", Price = 100, ItemsInStock = 10, ItemsReserved = 3 },
                //new() { Name = "Screwdriver", Price = 70, ItemsInStock = 20, ItemsReserved = 2 },
                //new() { Name = "Drill", Price = 500, ItemsInStock = 20, ItemsReserved = 0 }
            };

            context.Reviews.AddRange(reviews);
            context.SaveChanges();
        }
    }
}
