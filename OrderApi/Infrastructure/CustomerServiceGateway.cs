using RestSharp;
using SharedModels;
namespace OrderApi.Infrastructure {
    public class CustomerServiceGateway : IServiceGateway<CustomerDto> {
        private readonly string _customerServiceBaseUrl;

        public CustomerServiceGateway(string baseUrl) {
            _customerServiceBaseUrl = baseUrl;
        }

        public CustomerDto? Get(int id) {
            RestClient c = new(_customerServiceBaseUrl);

            RestRequest request = new(id.ToString());
            Task<CustomerDto?> response = c.GetAsync<CustomerDto>(request);
            response.Wait();
            return response.Result;
        }
        public List<CustomerDto>? GetAll() {
            RestClient c = new(_customerServiceBaseUrl);

            RestRequest request = new();
            Task<IEnumerable<CustomerDto>?> response = c.GetAsync<IEnumerable<CustomerDto>>(request);
            response.Wait();
            return response.Result?.ToList();
        }
        public bool UpdateMany(List<CustomerDto> updatedModels) {
            Console.WriteLine("Fuck you");
            throw new NotImplementedException();
        }
    }
}
