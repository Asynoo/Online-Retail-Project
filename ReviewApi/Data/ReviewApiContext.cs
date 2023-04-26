using Microsoft.EntityFrameworkCore;
using ReviewApi.Models;
namespace ReviewApi.Data {
    public class ReviewApiContext : DbContext {
        public ReviewApiContext(DbContextOptions<ReviewApiContext> options)
            : base(options) {
        }

        public DbSet<Review> Reviews { get; set; }
    }
}
