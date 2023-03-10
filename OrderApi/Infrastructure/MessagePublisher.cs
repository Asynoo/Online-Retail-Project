using EasyNetQ;
using SharedModels;

namespace OrderApi.Infrastructure;

public class MessagePublisher : IMessagePublisher, IDisposable
{
    IBus bus;

    public MessagePublisher(string connectionString)
    {
        bus = RabbitHutch.CreateBus(connectionString);
    }

    public void Dispose()
    {
        bus.Dispose();
    }

    public async Task PublishOrderStatusChangedMessage(int? customerId, IList<OrderLine> orderLines, string topic)
    {
        var message = new OrderStatusChangedMessage
        { 
            CustomerId = customerId,
            OrderLines = orderLines 
        };

        await bus.PubSub.PublishAsync(message, topic);
    }

}