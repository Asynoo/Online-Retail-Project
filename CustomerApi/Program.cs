using CustomerApi.Data;
using CustomerApi.Messaging;
using CustomerApi.Models;
using Microsoft.EntityFrameworkCore;
using SharedModels;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// RabbitMQ connection string (I use CloudAMQP as a RabbitMQ server).
const string cloudAmqpConnectionString = "host=cow.rmq2.cloudamqp.com;virtualHost=lylmzobc;username=lylmzobc;password=Uvoj_K_jYaPmfMZ3xVn1a4hWfXgee2Od";

// Add services to the container.1

builder.Services.AddDbContext<CustomerApiContext>(opt => opt.UseInMemoryDatabase("CustomerDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Customer>, CustomerRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();

// Register ProductConverter for dependency injection
builder.Services.AddSingleton<IConverter<Customer, CustomerDto>, CustomerConverter>();

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
    CustomerApiContext? dbContext = services.GetService<CustomerApiContext>();
    IDbInitializer? dbInitializer = services.GetService<IDbInitializer>();
    dbInitializer.Initialize(dbContext);
}

// Create a message listener in a separate thread.
Task.Factory.StartNew(() =>
    new MessageListener(app.Services, cloudAmqpConnectionString ).Start());

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
