namespace SharedModels {
    public class ReviewDto {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public DateTime AddedAt { get; set; }
        public decimal Rating { get; set; } // rating is between 0.0 and 5.0
        public string Description { get; set; }
    }
}
