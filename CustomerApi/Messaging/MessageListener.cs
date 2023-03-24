using CustomerApi.Data;
using CustomerApi.Models;
using EasyNetQ;
using SharedModels;

namespace CustomerApi.Messaging {
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
                _bus.PubSub.SubscribeAsync<CreditStandingChangedMessage>(
                    "productApiPaid",
                    message => Task.Factory.StartNew(
                        () => HandleOrderPaid(message)),
                    x => x.WithTopic("paid")
                );
                // Block the thread so that it will not exit and stop subscribing.
                lock (this) {
                    Monitor.Wait(this);
                }
            }
        }

        private async Task HandleOrderPaid(CreditStandingChangedMessage message) {
            // A service scope is created to get an instance of the product repository.
            // When the service scope is disposed, the product repository instance will
            // also be disposed.
            Console.WriteLine("youve reached this far");
            try {
                using (IServiceScope scope = _provider.CreateScope())
                {
                    IServiceProvider services = scope.ServiceProvider;
                    IRepository<Customer>? customerRepository = services.GetService<IRepository<Customer>>();
                    // Paid for ordered product, increases customer rating (should be a single transaction).
                    // Beware that this operation is not idempotent.

                    Customer? customer = await customerRepository.Get(message.CustomerId);
                    if (customer is null) {
                        Console.WriteLine($"Update of credit standing for customer {message.CustomerId} failed.");
                        return;
                    }
                    customer.creditStanding += message.CreditStanding;
                    customerRepository.Edit(customer);
                    Console.WriteLine($"Update of credit standing for customer {message.CustomerId} successful.");
                    //TODO do we want to send a message back perhaps?
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Update of credit standing for customer {message.CustomerId} failed.");
            }
        }
    }
}