using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedModels;
namespace OrderApi.Data {
    public class OrderRepository : IRepository<Order> {
        private readonly OrderApiContext _db;

        public OrderRepository(OrderApiContext context) {
            _db = context;
        }

        async Task<Order> IRepository<Order>.Add(Order entity) {
            entity.Date ??= DateTime.Now;
            EntityEntry<Order> newOrderEntry;
            newOrderEntry = await _db.Orders.AddAsync(entity);
            await _db.SaveChangesAsync();
            return newOrderEntry.Entity;
        }

        async Task<bool> IRepository<Order>.Edit(Order entity) {
            _db.Entry(entity).State = EntityState.Modified;
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        async Task<Order?> IRepository<Order>.Get(int id) {
            return await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        }

        async Task<IEnumerable<Order>> IRepository<Order>.GetAll() {
            return await _db.Orders.ToListAsync();
        }

        async Task<bool> IRepository<Order>.Remove(int id) {
            Order? order = await _db.Orders.FirstOrDefaultAsync(p => p.Id == id);
            if (order is null) {
                return false;
            }
            _db.Orders.Remove(order);
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }
    }
}
