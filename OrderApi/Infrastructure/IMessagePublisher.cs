using SharedModels;
namespace OrderApi.Infrastructure {
    public interface IMessagePublisher {
        Task PublishOrderStatusChangedMessage(
            int? customerId,
            int orderId,
            IList<OrderLineDto> orderLines, string topic);
        Task CreditStandingChangedMessage(int customerId,
            int creditStanding, string topic);
    }
}
