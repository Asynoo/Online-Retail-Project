using CustomerApi.Models;
namespace CustomerApi.Data {
    public class DbInitializer : IDbInitializer {
        public void Initialize(CustomerApiContext context) {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (context.Customers.Any()) {
                return;
            }

            List<Customer> customers = new() {
                new() { Name = "Testysen", Email = "testysen@email.com", Phone = "+4512345678", BillingAddress = "123 Esbjerg Street", ShippingAddress = "123 Esbjerg Street", creditStanding = 666 },
                new() { Name = "Testensen", Email = "Testensen@email.com", Phone = "+4532445612", BillingAddress = "345 Brande Street", ShippingAddress = "345 Brande Street", creditStanding = 420 },
                new() { Name = "Testusen", Email = "Testusen@email.com", Phone = "+4512675395", BillingAddress = "456 Denmark Street", ShippingAddress = "456 Denmark Street", creditStanding = 22 },
                new() { Name = "Joe Mama", Email = "joe.mama@email.com", Phone = "+4595475512", BillingAddress = "234 Varde Street", ShippingAddress = "234 Varde Street", creditStanding = 143 },
                new() { Name = "Brandon Piers", Email = "brand1536@email.com", Phone = "+4512452341", BillingAddress = "345 Aalborg Street", ShippingAddress = "345 Aalborg Street", creditStanding = -20 },
                new() { Name = "xXxCustomerxXx", Email = "money1337@email.com", Phone = "+4598214852", BillingAddress = "32 Freedom Street", ShippingAddress = "32 Freedom Street", creditStanding = 77 },
                new() { Name = "Seedman", Email = "seedadress@seedemail.com", Phone = "+4516546542", BillingAddress = "534 Shopping Street", ShippingAddress = "534 Shopping Street", creditStanding = 912340 },
                new() { Name = "IAmOutOfIdeas", Email = "nameyes@email.com", Phone = "+4532133769", BillingAddress = "Middle of nowhere", ShippingAddress = "1600 Pennsylvania Avenue", creditStanding = 1337 }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
