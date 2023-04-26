using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using Prometheus;
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

// Register converter for dependency injection
builder.Services.AddScoped<IConverter<Review, ReviewDto>, ReviewConverter>();

// Register product service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<ProductDto>>(new
    ProductServiceGateway(productServiceBaseUrl));

// Register customer service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<CustomerDto>>(new
    CustomerServiceGateway(customerServiceBaseUrl));

builder.Services.AddScoped<IDbInitializer, DbInitializer>();

// Register product service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<ProductDto>>(new
    ProductServiceGateway(productServiceBaseUrl));

// Register customer service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<CustomerDto>>(new
    CustomerServiceGateway(customerServiceBaseUrl));

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
    dbInitializer.Initialize(dbContext); //This don't work for some reason, I'm too tired sorry
}

app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.Run();
