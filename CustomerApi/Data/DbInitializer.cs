using CustomerApi.Models;

namespace CustomerApi.Data;

public class DbInitializer : IDbInitializer
{
    public void Initialize(CustomerApiContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        if (context.Customers.Any())
        {
            return;
        }

        List<Customer> customers = new List<Customer>
        {
            new Customer{Name = "Testysen", Email = "testysen@email.com", Phone = "+4512345678", BillingAddress = "123 Esbjerg Street", ShippingAddress = "123 Esbjerg Street", creditStanding = 666}
        };
        
        context.Customers.AddRange(customers);
        context.SaveChanges();
    }
    
}