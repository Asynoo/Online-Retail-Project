using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ReviewApi.Models;
namespace ReviewApi.Data {
    public class ReviewRepository : IRepository<Review>{
        private readonly ReviewApiContext _db;

        public ReviewRepository(ReviewApiContext context) {
            _db = context;
        }

        async Task<Review?> IRepository<Review>.Add(Review entity) {
            entity.AddedAt = DateTime.Now;
            EntityEntry<Review> newReviewEntry = await _db.Reviews.AddAsync(entity);
            await _db.SaveChangesAsync();
            return newReviewEntry.Entity;
        }

        async Task<bool> IRepository<Review>.Edit(Review entity) {
            _db.Entry(entity).State = EntityState.Modified;
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        async Task<Review?> IRepository<Review>.Get(int id) {
            Review? review = await _db.Reviews.FirstOrDefaultAsync(o => o.Id == id);
            return review;
        }

        async Task<IEnumerable<Review>> IRepository<Review>.GetAll(int? productId, int? customerId) {
            IQueryable<Review> query = _db.Reviews;
            if (productId is not null) {
                query = query.Where(r => r.ProductId == productId);
            }
            if (customerId is not null) {
                query = query.Where(r => r.CustomerId == customerId);
            }
            return await query.ToListAsync();
        }

        async Task<bool> IRepository<Review>.Remove(int id) {
            Review? review = await _db.Reviews.FirstOrDefaultAsync(p => p.Id == id);
            if (review is null) {
                return false;
            }
            _db.Reviews.Remove(review);
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }
    }
}
