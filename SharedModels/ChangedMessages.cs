namespace SharedModels {
    public class OrderStatusChangedMessage {
        public int? CustomerId { get; set; }
        public int OrderId { get; set; }
        public IList<OrderLine> OrderLines { get; set; }
    }

    public class OrderAcceptMessage
    {
        public int OrderId { get; set; }
    }
    public class OrderRejectMessage
    {
        public int OrderId { get; set; }
    }

    public class CreditStandingChangedMessage
    {
        public int CustomerId { get; set; }
        
        public  int CreditStanding { get; set; }

    }
}
