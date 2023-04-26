namespace SharedModels {
    public class ReviewDto {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; }
        public DateTime AddedAt { get; set; }
        public double Rating { get; set; } // rating is between 0.0 and 1.0
        public string Description { get; set; }
    }
}
