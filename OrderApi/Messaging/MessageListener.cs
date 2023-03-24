using EasyNetQ;
using OrderApi.Data;
using OrderApi.Models;
using SharedModels;

namespace OrderApi.Messaging; 

public class MessageListener {
        private readonly string _connectionString;
        private readonly IServiceProvider _provider;
        private IBus _bus;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider appServices, string cloudAmqpConnectionString) {
            _provider = appServices;
            _connectionString = cloudAmqpConnectionString;
        }

        public void Start() {
            using (_bus = RabbitHutch.CreateBus(_connectionString))
            {

                _bus.PubSub.SubscribeAsync<OrderTransactionMessage>("OrderAcceptedorRejected",
                    message => Task.Factory.StartNew(() => HandleOrderTransaction(message))
                    );
                lock (this) {
                    Monitor.Wait(this);
                }
            }
        }

        private async Task HandleOrderTransaction(OrderTransactionMessage message)
        {
            using (IServiceScope scope = _provider.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<Order>? orderRepository = services.GetService<IRepository<Order>>();

                Order? order = await orderRepository.Get(message.OrderId);
                if (order != null)
                {
                    if (message.Successful)
                    {
                        order.Status = OrderStatus.Completed;
                        Console.WriteLine($"Set order id: {message.OrderId} as completed");
                        await orderRepository.Edit(order);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to edit order id: {message.OrderId}");
                        await orderRepository.Remove(message.OrderId);
                    }
                }
                // Throw exception if not found
                else
                {
                    Console.WriteLine("Attempted to edit a nonexistent order");
                }
            }
        }
}