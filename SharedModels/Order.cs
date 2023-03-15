namespace SharedModels {
    public class Order {

        public enum OrderStatus {
            Cancelled,
            Pending,
            Completed,
            Shipped,
            Paid
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public IList<OrderLine> OrderLines { get; set; }
    }

    public class OrderLine {
        public int id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
