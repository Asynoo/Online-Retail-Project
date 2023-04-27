using System.ComponentModel.DataAnnotations;
namespace ReviewApi.Models; 

public class Review {
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public string Title { get; set; }
    public double Rating { get; set; } // rating is between 0.0 and 5.0
    public string Description { get; set; }
    public DateTime AddedAt { get; set; }
}

public class ReviewPostBindingModel {
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public string Title { get; set; }
    [Range(0,5)]
    public double Rating { get; set; } // rating is between 0.0 and 5.0
    public string Description { get; set; }
}