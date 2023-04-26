using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Messaging;
using OrderApi.Models;
using Prometheus;
using SharedModels;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Base URL for the product service when the solution is executed using docker-compose.
// The product service (running as a container) listens on this URL for HTTP requests
// from other services specified in the docker compose file (which in this solution is
// the order service).
// string productServiceBaseUrl = "http://productapi/products/";
string productServiceBaseUrl = "http://localhost:5298/products/";
// string customerServiceBaseUrl = "http://customerapi/customer/";
string customerServiceBaseUrl = "http://localhost:5057/customer/";

// RabbitMQ connection string (I use CloudAMQP as a RabbitMQ server).
// Remember to replace this connectionstring with your own.
string cloudAMQPConnectionString =
    "host=cow.rmq2.cloudamqp.com;virtualHost=lylmzobc;username=lylmzobc;password=Uvoj_K_jYaPmfMZ3xVn1a4hWfXgee2Od";

// Add services to the container.
builder.Services.AddDbContext<OrderApiContext>(opt => opt.UseInMemoryDatabase("OrdersDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Order>, OrderRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();

// Register product service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<ProductDto>>(new
    ProductServiceGateway(productServiceBaseUrl));

// Register customer service gateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<CustomerDto>>(new
    CustomerServiceGateway(customerServiceBaseUrl));

// Register MessagePublisher (a messaging gateway) for dependency injection
builder.Services.AddSingleton<IMessagePublisher>(new
    MessagePublisher(cloudAMQPConnectionString));


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

// Create a message listener in a separate thread.
Task.Factory.StartNew(() =>
    new MessageListener(app.Services, cloudAMQPConnectionString ).Start());

// Initialize the database.
using (IServiceScope scope = app.Services.CreateScope()) {
    IServiceProvider services = scope.ServiceProvider;
    OrderApiContext? dbContext = services.GetService<OrderApiContext>();
    IDbInitializer? dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

//app.UseHttpsRedirection();
app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.Run();
