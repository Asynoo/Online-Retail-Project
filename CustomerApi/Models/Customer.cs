namespace CustomerApi.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string BillingAddress { get; set; }
    public string ShippingAddress { get; set; }
    public int creditStanding { get; set; }
}