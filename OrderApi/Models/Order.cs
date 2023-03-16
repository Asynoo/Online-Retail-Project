using System.ComponentModel.DataAnnotations;
using SharedModels;

namespace OrderApi.Models;
public class Order {
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public IList<OrderLine> OrderLines { get; set; } = new List<OrderLine>();

    public Order(OrderPostBindingModel postBindingModel) {
        Date = postBindingModel.Date;
        CustomerId = postBindingModel.CustomerId;
        OrderLines = postBindingModel.OrderLines.Select(x => new OrderLine(x)).ToList();
    }

    public Order(){}
}

public class OrderLine {
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    
    public OrderLine() {}
    
    public OrderLine(OrderLinePostBindingModel postBindingModel) {
        ProductId = postBindingModel.ProductId;
        Quantity = postBindingModel.Quantity;
    }
}

public class OrderPostBindingModel {
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public IList<OrderLinePostBindingModel> OrderLines { get; set; }
}

public class OrderLinePostBindingModel {
    public int ProductId { get; set; }
    [Range(1, 50)]
    public int Quantity { get; set; }
}