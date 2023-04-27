using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using Prometheus;
using ReviewApi.Data;
using ReviewApi.Infrastructure;
using ReviewApi.Models;
using SharedModels;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//Change these URLs when you run it yourself, i might have different ports depending on HTTP/HTTPS
const string productServiceBaseUrl = "http://productApi/products/";
// const string productServiceBaseUrl = "http://localhost:5298/products/";
const string customerServiceBaseUrl = "http://customerApi/products/";
// const string customerServiceBaseUrl = "http://localhost:5057/customer/";
const string orderServiceBaseUrl = "http://orderApi/products/";
// const string orderServiceBaseUrl = "http://localhost:5197/customer/";

// Add services to the container.
builder.Services.AddDbContext<ReviewApiContext>(opt => opt.UseInMemoryDatabase("ReviewsDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Review>, ReviewRepository>();

// Register converter for dependency injection
builder.Services.AddScoped<IConverter<Review, ReviewDto>, ReviewConverter>();

// Register order service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<OrderDto>>(new OrderServiceGateway(orderServiceBaseUrl));

// Register product service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<ProductDto>>(new ProductServiceGateway(productServiceBaseUrl));

// Register customer service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<CustomerDto>>(new CustomerServiceGateway(customerServiceBaseUrl));

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Initialize the database.
using (IServiceScope scope = app.Services.CreateScope()) {
    IServiceProvider services = scope.ServiceProvider;
    ReviewApiContext? dbContext = services.GetService<ReviewApiContext>();
    IDbInitializer? dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.Run();
