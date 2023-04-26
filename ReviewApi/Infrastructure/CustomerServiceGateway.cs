using RestSharp;
using SharedModels;
namespace ReviewApi.Infrastructure {
    public class CustomerServiceGateway : IServiceGateway<CustomerDto> {
        private readonly string _customerServiceBaseUrl;

        public CustomerServiceGateway(string baseUrl) {
            _customerServiceBaseUrl = baseUrl;
        }

        public async Task<CustomerDto?> Get(int id) {
            RestClient c = new(_customerServiceBaseUrl);

            RestRequest request = new(id.ToString());
            CustomerDto? response = await c.GetAsync<CustomerDto>(request);
            return response;
        }
        public async Task<List<CustomerDto>?> GetAll() {
            RestClient c = new(_customerServiceBaseUrl);

            RestRequest request = new();
            IEnumerable<CustomerDto>? response = await c.GetAsync<IEnumerable<CustomerDto>>(request);
            return response?.ToList();
        }
    }
}
