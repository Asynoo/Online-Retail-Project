namespace SharedModels {
    public enum OrderStatus {
        Cancelled,
        Pending,
        Completed,
        Shipped,
        Paid
    }
    
    public class OrderDto {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public IList<OrderLineDto> OrderLines { get; set; }
    }

    public class OrderLineDto {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
