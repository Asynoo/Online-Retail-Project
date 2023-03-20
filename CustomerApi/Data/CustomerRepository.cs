using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
namespace CustomerApi.Data {
    public class CustomerRepository : IRepository<Customer> {
        private readonly CustomerApiContext _db;

        public CustomerRepository(CustomerApiContext context) {
            _db = context;
        }

        async Task<IEnumerable<Customer>> IRepository<Customer>.GetAll() {
            return await _db.Customers.ToListAsync();
        }

        async Task<Customer?> IRepository<Customer>.Get(int id) {
            Customer customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            _db.Entry<Customer>(customer).Reload();
            return customer;
        }

        async Task<Customer> IRepository<Customer>.Add(Customer entity) {
            Customer newCustomer = (await _db.Customers.AddAsync(entity)).Entity;
            await _db.SaveChangesAsync();
            return newCustomer;
        }

        async Task<bool> IRepository<Customer>.Edit(Customer entity) {
            _db.Entry(entity).State = EntityState.Modified;
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        async Task<bool> IRepository<Customer>.Remove(int id) {
            Customer? customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer is null) {
                return false;
            }
            _db.Customers.Remove(customer);
            int changes = await _db.SaveChangesAsync();
            return changes > 0;
        }
    }
}
