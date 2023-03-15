using EasyNetQ;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
namespace ProductApi.Infrastructure {
    public class MessageListener {
        private readonly string _connectionString;
        private readonly IServiceProvider _provider;
        private IBus _bus;

        // The service provider is passed as a parameter, because the class needs
        // access to the product repository. With the service provider, we can create
        // a service scope that can provide an instance of the product repository.
        public MessageListener(IServiceProvider appServices, string cloudAmqpConnectionString) {
            this._provider = appServices;
            this._connectionString = cloudAmqpConnectionString;
        }
        

        public void Start() {
            using (_bus = RabbitHutch.CreateBus(_connectionString)) {
                _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiCompleted",
                    HandleOrderCompleted, x => x.WithTopic("completed"));
                _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiShipped",
                    HandleOrderShipped, x => x.WithTopic("shipped"));
                _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiCancelled",
                    HandleOrderCancelled, x => x.WithTopic("cancelled"));
                _bus.PubSub.Subscribe<OrderStatusChangedMessage>("productApiPaid",
                    HandleOrderPaid, x => x.WithTopic("paid"));
                _bus.PubSub.Subscribe<CreditStandingChangedMessage>("productApiPaid",
                    HandleOrderPaidTwo, x => x.WithTopic("paid"));
                // Block the thread so that it will not exit and stop subscribing.
                lock (this) {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderCompleted(OrderStatusChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserve items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                if (ProductAvailable(message.OrderLines, productRepos))
                {
                    try
                    {
                        foreach (OrderLine orderLine in message.OrderLines)
                        {
                            Task<Product?> product = productRepos.Get(orderLine.ProductId);
                            if (product.Result == null) continue;
                            product.Result.ItemsReserved += orderLine.Quantity;
                            productRepos.Edit(product.Result);
                        }

                        var replyMessage = new OrderAcceptMessage
                        {
                            OrderId = message.OrderId
                        };
                        _bus.PubSub.Publish(replyMessage);
                    }
                    catch
                    {
                        var replyMessage = new OrderRejectMessage
                        {
                            OrderId = message.OrderId
                        };
                        _bus.PubSub.Publish(replyMessage);

                        throw new ArgumentException();
                    }
                    
                } else
                {
                    var replyMessage = new OrderAcceptMessage
                    {
                        OrderId = message.OrderId
                    };
                    _bus.PubSub.Publish(replyMessage);
                }
                
            }
        }
        private void HandleOrderShipped(OrderStatusChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Ships(deletes from inventory) items of ordered product (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (OrderLine orderLine in message.OrderLines) {
                    Task<Product?> product = productRepos!.Get(orderLine.ProductId);
                    if (product.Result == null) continue;
                    product.Result.ItemsReserved -= orderLine.Quantity;
                    product.Result.ItemsInStock -= orderLine.Quantity;
                    productRepos.Edit(product.Result);
                }
            }
        }
        private void HandleOrderCancelled(OrderStatusChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Reserved items of ordered product is removed, as order is cancelled (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (OrderLine orderLine in message.OrderLines) {
                    Task<Product?> product = productRepos.Get(orderLine.ProductId);
                    if (product.Result == null) continue;
                    product.Result.ItemsReserved -= orderLine.Quantity;
                    productRepos.Edit(product.Result);
                }
            }
        }
        private void HandleOrderPaid(OrderStatusChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                // Paid for ordered product, removes items from stock (should be a single transaction).
                // Beware that this operation is not idempotent.
                foreach (OrderLine orderLine in message.OrderLines) {
                    Task<Product?> product = productRepos.Get(orderLine.ProductId);
                    if (product.Result == null) continue;
                    product.Result.ItemsReserved -= orderLine.Quantity;
                    product.Result.ItemsInStock -= orderLine.Quantity;
                    productRepos.Edit(product.Result);

                }
            }
        }
        
        private void HandleOrderPaidTwo(CreditStandingChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            using (IServiceScope scope = _provider.CreateScope()) {
                IServiceProvider services = scope.ServiceProvider;
                var customerRepos = services.GetService<Data.IRepository<CustomerDto>>();
                // Paid for ordered product, increases customer rating (should be a single transaction).
                // Beware that this operation is not idempotent.
                Task<CustomerDto> customer = customerRepos.Get(message.CustomerId);
                    customer.Result.creditStanding += message.CreditStanding;
                    customerRepos.Edit(customer.Result); 
                    
            }
        }

        private static bool ProductAvailable(IList<OrderLine> orderLines, IRepository<Product> productRepo)
        {
            foreach (OrderLine orderLine in orderLines)
            {
                Task<Product?> product = productRepo.Get(orderLine.ProductId);
                if (product.Result == null) continue;
                if (orderLine.Quantity > product.Result.ItemsInStock - product.Result.ItemsReserved);
                return false;
            }
            return true;
        }
    }
}
