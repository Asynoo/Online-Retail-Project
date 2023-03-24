using SharedModels;

namespace OrderApi.Models {
    public class OrderConverter : IConverter<Order, OrderDto> {
        private readonly IConverter<OrderLine, OrderLineDto> _orderLineConverter = new OrderLineConverter();
        
        public Order Convert(OrderDto sharedOrder) {
            return new Order {
                Id = sharedOrder.Id,
                Date = sharedOrder.Date,
                CustomerId = sharedOrder.CustomerId,
                Status = sharedOrder.Status,
                OrderLines = sharedOrder.OrderLines.Select(x => _orderLineConverter.Convert(x)).ToList()
            };
        }

        public OrderDto Convert(Order hiddenOrder) {
            return new OrderDto {
                Id = hiddenOrder.Id,
                Date = hiddenOrder.Date,
                CustomerId = hiddenOrder.CustomerId,
                Status = hiddenOrder.Status,
                OrderLines = hiddenOrder.OrderLines.Select(x => _orderLineConverter.Convert(x)).ToList()
            };
        }
    }
}
