using EasyNetQ;
using SharedModels;
namespace OrderApi.Infrastructure {
    public class MessagePublisher : IMessagePublisher, IDisposable {
        private readonly IBus _bus;

        public MessagePublisher(string connectionString) {
            _bus = RabbitHutch.CreateBus(connectionString);
        }

        public void Dispose() {
            _bus.Dispose();
        }

        public async Task PublishOrderStatusChangedMessage(int? customerId, int orderId, IList<OrderLineDto> orderLines, string topic) {
            OrderStatusChangedMessage message = new() {
                CustomerId = customerId,
                OrderId = orderId,
                OrderLines = orderLines
            };

            await _bus.PubSub.PublishAsync(message, topic);
        }
        
        public async Task CreditStandingChangedMessage(int customerId, int creditStanding, string topic) {
            CreditStandingChangedMessage message = new() {
                CustomerId = customerId,
                CreditStanding = creditStanding
            };
            await _bus.PubSub.PublishAsync(message, topic);
        }
    }
}
