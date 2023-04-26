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

            List<Review> reviews = new() {
                new() { ProductId = 1, CustomerId = 1, AddedAt = new DateTime(2023,3,22) ,Title = "Great product!", Rating = 0.9, Description = "I love this product. It does exactly what it's supposed to do." },
                new() { ProductId = 2, CustomerId = 2, AddedAt = new DateTime(2023,3,21) , Title = "Disappointed", Rating = 0.4, Description = "I had high hopes for this product, but it didn't live up to my expectations." },
                new() { ProductId = 3, CustomerId = 3, AddedAt = new DateTime(2023,3,25) , Title = "Average", Rating = 0.6, Description = "This product was okay. Nothing special, but it got the job done." },
                new() { ProductId = 4, CustomerId = 4, AddedAt = new DateTime(2023,4,6) , Title = "Waste of money", Rating = 0.2, Description = "I regret buying this product. It didn't work at all and was a complete waste of money." },
                new() { ProductId = 5, CustomerId = 5, AddedAt = new DateTime(2023,4,11) , Title = "Excellent product", Rating = 0.9, Description = "This product exceeded my expectations. It's well-made and works perfectly." },
                new() { ProductId = 6, CustomerId = 6, AddedAt = new DateTime(2023,4,15) , Title = "Not worth it", Rating = 0.3, Description = "I wouldn't recommend this product. It's not worth the price and didn't work as well as I had hoped." },
                new() { ProductId = 7, CustomerId = 7, AddedAt = new DateTime(2023,4,22) , Title = "Decent", Rating = 0.6, Description = "This product was decent. It did what it was supposed to, but I wasn't blown away by it." },
                new() { ProductId = 8, CustomerId = 8, AddedAt = new DateTime(2023,4,25) , Title = "Terrible", Rating = 0.1, Description = "I had a terrible experience with this product. It broke within a week and the customer service was unhelpful." }
            };

            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
        }
    }
}