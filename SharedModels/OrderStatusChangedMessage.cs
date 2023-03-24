namespace SharedModels {
    public class OrderStatusChangedMessage {
        public int? CustomerId { get; set; }
        public int OrderId { get; set; }
        public IList<OrderLineDto> OrderLines { get; set; }
    }

    public class OrderTransactionMessage {
        public int OrderId { get; set; }
        public bool Successful { get; set; }
    }

    public class CreditStandingChangedMessage {
        public int CustomerId { get; set; }
        public int CreditStanding { get; set; }
    }
    
}
