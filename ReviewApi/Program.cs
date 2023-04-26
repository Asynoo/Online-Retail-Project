using Microsoft.EntityFrameworkCore;
using ReviewApi.Data;
using ReviewApi.Infrastructure;
using ReviewApi.Models;
using SharedModels;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

const string productServiceBaseUrl = "http://localhost:5298/products/";
const string customerServiceBaseUrl = "http://localhost:5057/customer/";

// Add services to the container.
builder.Services.AddDbContext<ReviewApiContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Review>, ReviewRepository>();

// Register product service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<ProductDto>>(new
    ProductServiceGateway(productServiceBaseUrl));

// Register customer service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<CustomerDto>>(new
    CustomerServiceGateway(customerServiceBaseUrl));

WebApplication app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();