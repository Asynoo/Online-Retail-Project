using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ProductApi.Models;
namespace ProductApi.Data {
    public class ProductRepository : IRepository<Product> {
        private readonly ProductApiContext _db;

        public ProductRepository(ProductApiContext context) {
            _db = context;
        }

        #region CREATE
        async Task<Product> IRepository<Product>.Add(Product entity) {
            EntityEntry<Product> newProductEntry = await _db.Products.AddAsync(entity);
            await _db.SaveChangesAsync();
            return newProductEntry.Entity;
        }
        #endregion

        #region DELETE
        async Task<bool> IRepository<Product>.Remove(int id) {
            Product? product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product is null) {
                return false;
            }
            _db.Products.Remove(product);
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }
        #endregion
        #region READ
        async Task<Product?> IRepository<Product>.Get(int id) {
            return await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        async Task<IEnumerable<Product>> IRepository<Product>.GetAll() {
            return await _db.Products.ToListAsync();
        }
        #endregion

        #region UPDATE
        async Task<bool> IRepository<Product>.Edit(Product entity) {
            _db.Entry(entity).State = EntityState.Modified;
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Edit(List<Product> entityList) {
            foreach (Product product in entityList) { //TODO: Use bulk update or something, this is slow in big systems
                _db.Entry(product).State = EntityState.Modified;
            }
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }
        #endregion
    }
}
