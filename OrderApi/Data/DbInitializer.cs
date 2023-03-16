using OrderApi.Models;
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

            List<Order> orders = new() {
                new() {
                    Date = DateTime.Today, CustomerId = 1, Status = OrderStatus.Pending, OrderLines = new List<OrderLine> {
                        new() { ProductId = 1, Quantity = 2 }
                    }
                },
                new() {
                    Date = DateTime.Today, CustomerId = 1, Status = OrderStatus.Completed, OrderLines = new List<OrderLine> {
                        new() { ProductId = 1, Quantity = 1 }
                    }
                },
                new() {
                    Date = DateTime.Today, CustomerId = 1, Status = OrderStatus.Shipped, OrderLines = new List<OrderLine> {
                        new() { ProductId = 2, Quantity = 1 }
                    }
                },
                new() {
                    Date = DateTime.Today, CustomerId = 1, Status = OrderStatus.Paid, OrderLines = new List<OrderLine> {
                        new() { ProductId = 2, Quantity = 1 }
                    }
                }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
