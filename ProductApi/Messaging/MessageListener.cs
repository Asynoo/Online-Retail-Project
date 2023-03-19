using EasyNetQ;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;

namespace ProductApi.Messaging {
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
                _bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>(
                    "productApiCompleted",
                    message => Task.Factory.StartNew(() => HandleOrderCompleted(message)),
                    x => x.WithTopic("completed")
                );
                _bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>("productApiShipped",
                    message => Task.Factory.StartNew(() => HandleOrderShipped(message)),
                    x => x.WithTopic("shipped")
                );
                _bus.PubSub.SubscribeAsync<OrderStatusChangedMessage>("productApiCancelled",
                    message => Task.Factory.StartNew(() => HandleOrderCancelled(message)),
                    x => x.WithTopic("cancelled")
                );
                // Block the thread so that it will not exit and stop subscribing.
                lock (this) {
                    Monitor.Wait(this);
                }
            }
        }

        private async Task HandleOrderCompleted(OrderStatusChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<Product>? productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                
                // Verify that all products are available
                if (await ProductAvailable(message.OrderLines, productRepos)) {
                    foreach (OrderLineDto orderLine in message.OrderLines) {
                        Product? product = await productRepos.Get(orderLine.ProductId);
                        if (product != null) {
                            product.ItemsReserved += orderLine.Quantity;
                            await productRepos.Edit(product);
                        }
                        // Publish reject message if product doesn't exist
                        else {
                            Console.WriteLine($"Reservation of products for order {message.OrderId} failed.");
                            await _bus.PubSub.PublishAsync(new OrderTransactionMessage { OrderId = message.OrderId, Successful = false });
                            return;
                        }
                    }
                    Console.WriteLine($"Reservation of products for order {message.OrderId} successful.");
                    // All products have been updated, return success message
                    await _bus.PubSub.PublishAsync(new OrderTransactionMessage { OrderId = message.OrderId, Successful = true });
                }
                else {
                    Console.WriteLine($"Reservation of products for order {message.OrderId} failed.");
                    await _bus.PubSub.PublishAsync(new OrderTransactionMessage { OrderId = message.OrderId, Successful = false });
                }
            }
        }

        private async Task HandleOrderShipped(OrderStatusChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<Product>? productRepos = services.GetService<IRepository<Product>>();

                // Ships(deletes from inventory) items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (OrderLineDto orderLine in message.OrderLines) {
                    Product? product = await productRepos.Get(orderLine.ProductId);
                    if (product != null) {
                        product.ItemsReserved -= orderLine.Quantity;
                        product.ItemsInStock -= orderLine.Quantity;
                        await productRepos.Edit(product);
                    }
                    // Publish reject message if product doesn't exist
                    else {
                        Console.WriteLine($"Shipping of reserved products for order {message.OrderId} failed.");
                        await _bus.PubSub.PublishAsync(new OrderTransactionMessage { OrderId = message.OrderId, Successful = false });
                        return;
                    }
                }                       
                Console.WriteLine($"Shipping of reserved products for order {message.OrderId} successful.");
                // All products have been updated, return success message
                await _bus.PubSub.PublishAsync(new OrderTransactionMessage { OrderId = message.OrderId, Successful = true });
            }
        }

        private async Task HandleOrderCancelled(OrderStatusChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<Product>? productRepos = services.GetService<IRepository<Product>>();

                // Reserved items of ordered product is removed, as order is cancelled (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (OrderLineDto orderLine in message.OrderLines) {
                    Product? product = await productRepos.Get(orderLine.ProductId);
                    if (product != null) {
                        product.ItemsReserved -= orderLine.Quantity;
                        await productRepos.Edit(product);
                    }
                    // Publish reject message if product doesn't exist
                    else {
                        Console.WriteLine($"Cancellation of reserved products for order {message.OrderId} failed.");
                        await _bus.PubSub.PublishAsync(new OrderTransactionMessage { OrderId = message.OrderId, Successful = false });
                        return;
                    }
                }
                Console.WriteLine($"Cancellation of reserved products for order {message.OrderId} successful.");
                // All products have been updated, return success message
                await _bus.PubSub.PublishAsync(new OrderTransactionMessage { OrderId = message.OrderId, Successful = true });
            }
        }

        private void HandleOrderPaidTwo(CreditStandingChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                IRepository<CustomerDto>? customerRepos = services.GetService<IRepository<CustomerDto>>();
                // Paid for ordered product, increases customer rating (should be a single transaction).
                // Beware that this operation is not idempotent.
                Task<CustomerDto> customer = customerRepos.Get(message.CustomerId);
                customer.Result.creditStanding += message.CreditStanding;
                customerRepos.Edit(customer.Result);
            }
        }

        private static async Task<bool> ProductAvailable(IList<OrderLineDto> orderLines, IRepository<Product> productRepo) {
            foreach (OrderLineDto orderLine in orderLines) {
                Product? product = await productRepo.Get(orderLine.ProductId);
                if (product != null) {
                    if (orderLine.Quantity > product.ItemsInStock - product.ItemsReserved)
                        return false;
                }
            }
            return true;
        }
    }
}