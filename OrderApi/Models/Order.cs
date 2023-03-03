using System;
namespace OrderApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public List<OrderLine> OrderLines { get; set; }
    }

    public class OrderLine {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime? Date { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    enum OrderStatus {
        Completed,
        Canceled,
        Shipped,
        Paid,
    }
}
