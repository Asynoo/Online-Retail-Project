using SharedModels;
namespace OrderApi.Infrastructure {
    public interface IMessagePublisher {
        Task PublishOrderStatusChangedMessage(int? customerId,
            IList<OrderLine> orderLines, string topic);
    }
}
