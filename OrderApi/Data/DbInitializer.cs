using SharedModels;
namespace OrderApi.Data {
    public class DbInitializer : IDbInitializer {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context) {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Orders.Any()) {
                return; // DB has been seeded
            }

            var orders = new List<Order> {
                new() {
                    Date = DateTime.Today, OrderLines = new List<OrderLine> {
                        new() { ProductId = 1, Quantity = 2 }
                    }
                }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
