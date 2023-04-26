using Microsoft.EntityFrameworkCore;
using ReviewApi.Models;

namespace ReviewApi.Data {
    public class DbInitializer : IDbInitializer {
        // This method will create and seed the database.
        public async void Initialize(ReviewApiContext context) {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Look for any Reviews
            if (await context.Reviews.AnyAsync()) {
                return; // DB has been seeded
            }

            List<Review> reviews = new List<Review> {
                new() { ProductId = 1, CustomerId = 1, Title = "Great product!", Rating = 0.9, Description = "I love this product. It does exactly what it's supposed to do." },
                new() { ProductId = 2, CustomerId = 2, Title = "Disappointed", Rating = 0.4, Description = "I had high hopes for this product, but it didn't live up to my expectations." },
                new() { ProductId = 3, CustomerId = 3, Title = "Average", Rating = 0.6, Description = "This product was okay. Nothing special, but it got the job done." }
            };

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
        }
    }
}