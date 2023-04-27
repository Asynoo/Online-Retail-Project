using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Messaging;
using ProductApi.Models;
using Prometheus;
using SharedModels;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

const string reviewServiceBaseUrl = "http://reviewApi/reviews/";
// const string reviewServiceBaseUrl = "http://localhost:5019/reviews/";


// RabbitMQ connection string (I use CloudAMQP as a RabbitMQ server).
const string cloudAmqpConnectionString = "host=cow.rmq2.cloudamqp.com;virtualHost=lylmzobc;username=lylmzobc;password=Uvoj_K_jYaPmfMZ3xVn1a4hWfXgee2Od";


// Use this connection string if you want to run RabbitMQ server as a container
// (see docker-compose.yml)
//string cloudAMQPConnectionString = "host=rabbitmq";

// Add services to the container.

builder.Services.AddDbContext<ProductApiContext>(opt => opt.UseInMemoryDatabase("ProductsDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();
 
// Register ProductConverter for dependency injection
builder.Services.AddSingleton<IConverter<Product, ProductDto>, ProductConverter>();

// Register ReviewGateway for dependency injection
builder.Services.AddSingleton<IServiceGateway<ReviewDto>>(new ReviewServiceGateway(reviewServiceBaseUrl));

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
    ProductApiContext? dbContext = services.GetService<ProductApiContext>();
    IDbInitializer? dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

// Create a message listener in a separate thread.
Task.Factory.StartNew(() =>
    new MessageListener(app.Services, cloudAmqpConnectionString ).Start());


//app.UseHttpsRedirection();
app.UseHttpMetrics();


app.UseAuthorization();

app.MapControllers();

app.MapMetrics();


app.Run();
