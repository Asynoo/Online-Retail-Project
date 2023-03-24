using SharedModels;

namespace OrderApi.Models {
    public class OrderLineConverter : IConverter<OrderLine, OrderLineDto> {
        public OrderLine Convert(OrderLineDto sharedOrderLine) {
            return new OrderLine {
                Id = sharedOrderLine.Id,
                OrderId = sharedOrderLine.OrderId,
                ProductId = sharedOrderLine.ProductId,
                Quantity = sharedOrderLine.Quantity
            };
        }

        public OrderLineDto Convert(OrderLine hiddenOrderLine) {
            return new OrderLineDto {
                Id = hiddenOrderLine.Id,
                OrderId = hiddenOrderLine.OrderId,
                ProductId = hiddenOrderLine.ProductId,
                Quantity = hiddenOrderLine.Quantity
            };
        }
    }
}
