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
                    
                
                
                // _bus.PubSub.SubscribeAsync<OrderTransactionMessage>("productApiCompleted",
                //     message => Task.Factory.StartNew(() => HandleOrderCompleted(message))
                //     , x => x.WithTopic("completed")
                // );
                // _bus.PubSub.SubscribeAsync<OrderTransactionMessage>("productApiShipped",
                //     message => Task.Factory.StartNew(() => HandleOrderShipped(message))
                //     , x => x.WithTopic("shipped")
                // );
                // _bus.PubSub.SubscribeAsync<OrderTransactionMessage>("productApiCancelled",
                //     message => Task.Factory.StartNew(() => HandleOrderCancelled(message))
                //     ,x => x.WithTopic("cancelled")
                // );
                // Block the thread so that it will not exit and stop subscribing.
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


        // private async Task HandleOrderCompleted(OrderTransactionMessage message) {
        //     // A service scope is created to get an instance of the product repository.
        //     // When the service scope is disposed, the product repository instance will
        //     // also be disposed.
        //     using (IServiceScope scope = _provider.CreateScope()) {
        //         IServiceProvider services = scope.ServiceProvider;
        //         IRepository<Order>? orderRepository = services.GetService<IRepository<Order>>();
        //
        //
        //         Order? order = await orderRepository.Get(message.OrderId);
        //         if (order != null) {
        //             if (message.Successful) {
        //                 order.Status = OrderStatus.Completed;
        //                 Console.WriteLine($"Set order id: {message.OrderId} as completed");
        //                 await orderRepository.Edit(order);
        //             }
        //             else {
        //                 Console.WriteLine($"Failed to create order id: {message.OrderId}");
        //                 await orderRepository.Remove(message.OrderId);
        //             }
        //         }
        //         // Throw exception if not found
        //         else {
        //             Console.WriteLine("Attempted to edit a nonexistent order");
        //         }
        //     }
        // }
        //
        // private async Task HandleOrderShipped(OrderTransactionMessage message) {
        //     // A service scope is created to get an instance of the product repository.
        //     // When the service scope is disposed, the product repository instance will
        //     // also be disposed.
        //     using (IServiceScope scope = _provider.CreateScope()) {
        //         IServiceProvider services = scope.ServiceProvider;
        //         IRepository<Order>? orderRepository = services.GetService<IRepository<Order>>();
        //
        //         // Check if the transaction was successful
        //         if (message.Successful) {
        //             Order? order = await orderRepository.Get(message.OrderId);
        //             if (order != null) {
        //                 order.Status = OrderStatus.Shipped;
        //                 await orderRepository.Edit(order);
        //             }
        //             // Throw exception if not found
        //             else {
        //                 throw new TaskCanceledException("Attempted to edit a nonexistent order");
        //             }
        //         }
        //         else {
        //             throw new TaskCanceledException("Failed to update products");
        //         }
        //     }
        // }
        //
        // private async Task HandleOrderCancelled(OrderTransactionMessage message) {
        //     // A service scope is created to get an instance of the product repository.
        //     // When the service scope is disposed, the product repository instance will
        //     // also be disposed.
        //     using (IServiceScope scope = _provider.CreateScope()) {
        //         IServiceProvider services = scope.ServiceProvider;
        //         IRepository<Order>? orderRepository = services.GetService<IRepository<Order>>();
        //
        //         // Check if the transaction was successful
        //         if (message.Successful) {
        //             Order? order = await orderRepository.Get(message.OrderId);
        //             if (order != null) {
        //                 order.Status = OrderStatus.Cancelled;
        //                 Console.WriteLine("Writeline message debug " + order.Status + order.Id);
        //
        //                 await orderRepository.Edit(order);
        //             }
        //             // Throw exception if not found
        //             else {
        //                 throw new TaskCanceledException("Attempted to edit a nonexistent order");
        //             }
        //         }
        //         else {
        //             throw new TaskCanceledException("Failed to update products");
        //         }
        //     }
        // }
    }