using Microsoft.EntityFrameworkCore;
using OrderApi.Models;

namespace OrderApi.Data
{
    public class OrderApiContext : DbContext
    {
        public OrderApiContext(DbContextOptions<OrderApiContext> options)
            : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
    }
}
