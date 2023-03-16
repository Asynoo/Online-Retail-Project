using EasyNetQ;
using OrderApi.Data;
using SharedModels;

namespace OrderApi.Infrastructure; 

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
            using (_bus = RabbitHutch.CreateBus(_connectionString)) {
                _bus.PubSub.SubscribeAsync<OrderTransactionMessage>(
                    "productApiCompleted",
                    message => Task.Factory.StartNew(() => HandleOrderCompleted(message)),
                    x => x.WithTopic("completed")
                );
                _bus.PubSub.SubscribeAsync<OrderTransactionMessage>("productApiShipped",
                    message => Task.Factory.StartNew(() => HandleOrderShipped(message)),
                    x => x.WithTopic("shipped")
                );
                _bus.PubSub.SubscribeAsync<OrderTransactionMessage>("productApiCancelled",
                    message => Task.Factory.StartNew(() => HandleOrderCancelled(message)),
                    x => x.WithTopic("cancelled")
                );
                // Block the thread so that it will not exit and stop subscribing.
                lock (this) {
                    Monitor.Wait(this);
                }
            }
        }

        private async Task HandleOrderCompleted(OrderTransactionMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<OrderDto>? orderRepository = services.GetService<IRepository<OrderDto>>();

                // Check if the transaction was successful
                if (message.Successful) {
                    OrderDto? order = await orderRepository.Get(message.OrderId);
                    if (order != null) {
                        order.Status = OrderStatus.Completed;
                        await orderRepository.Edit(order);
                    }
                    // Throw exception if not found
                    else {
                        throw new TaskCanceledException("Attempted to edit a nonexistent order");
                    }
                }
                else {
                    throw new TaskCanceledException("Failed to update products");
                }
            }
        }

        private async Task HandleOrderShipped(OrderTransactionMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<OrderDto>? orderRepository = services.GetService<IRepository<OrderDto>>();

                // Check if the transaction was successful
                if (message.Successful) {
                    OrderDto? order = await orderRepository.Get(message.OrderId);
                    if (order != null) {
                        order.Status = OrderStatus.Shipped;
                        await orderRepository.Edit(order);
                    }
                    // Throw exception if not found
                    else {
                        throw new TaskCanceledException("Attempted to edit a nonexistent order");
                    }
                }
                else {
                    throw new TaskCanceledException("Failed to update products");
                }
            }
        }

        private async Task HandleOrderCancelled(OrderTransactionMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<OrderDto>? orderRepository = services.GetService<IRepository<OrderDto>>();

                // Check if the transaction was successful
                if (message.Successful) {
                    OrderDto? order = await orderRepository.Get(message.OrderId);
                    if (order != null) {
                        order.Status = OrderStatus.Cancelled;
                        await orderRepository.Edit(order);
                    }
                    // Throw exception if not found
                    else {
                        throw new TaskCanceledException("Attempted to edit a nonexistent order");
                    }
                }
                else {
                    throw new TaskCanceledException("Failed to update products");
                }
            }
        }
    }