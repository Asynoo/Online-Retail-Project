using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProductApi.Models;

namespace ProductApi.Data
{
    public class ProductRepository : IRepository<Product> {
        private readonly ProductApiContext _db;

        public ProductRepository(ProductApiContext context) {
            _db = context;
        }

        async Task<Product> IRepository<Product>.Add(Product entity) {
            EntityEntry<Product> newProductEntry = await _db.Products.AddAsync(entity);
            await _db.SaveChangesAsync();
            return newProductEntry.Entity;
        }

        async Task<bool> IRepository<Product>.Edit(Product entity) {
            _db.Entry(entity).State = EntityState.Modified;
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        async Task<Product?> IRepository<Product>.Get(int id) {
            return await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        async Task<IEnumerable<Product>> IRepository<Product>.GetAll() {
            return await _db.Products.ToListAsync();
        }

        async Task<bool> IRepository<Product>.Remove(int id) {
            Product? product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product is null) {
                return false;
            }
            _db.Products.Remove(product);
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }
    }
}
