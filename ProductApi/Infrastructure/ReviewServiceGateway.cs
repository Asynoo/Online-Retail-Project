using RestSharp;
using SharedModels;
namespace ProductApi.Infrastructure {
    public class ReviewServiceGateway : IServiceGateway<ReviewDto> {
        private readonly string _reviewServiceBaseUrl;

        public ReviewServiceGateway(string baseUrl) {
            _reviewServiceBaseUrl = baseUrl;
        }
        
        public async Task<ReviewDto?> Get(int id) {
            RestClient c = new(_reviewServiceBaseUrl);

            RestRequest request = new(id.ToString());
            ReviewDto? response = await c.GetAsync<ReviewDto>(request);
            return response;
        }
        
        public async Task<List<ReviewDto>?> GetAll() {
            RestClient c = new(_reviewServiceBaseUrl);

            RestRequest request = new();
            IEnumerable<ReviewDto>? response = await c.GetAsync<IEnumerable<ReviewDto>>(request);
            return response?.ToList();
        }
        
        public async Task<List<ReviewDto>?> GetForProduct(int productId) {
            RestClient c = new(_reviewServiceBaseUrl);

            RestRequest request = new($"/product/{productId}");
            IEnumerable<ReviewDto>? response = await c.GetAsync<IEnumerable<ReviewDto>>(request);
            return response?.ToList();
        }
    }
}
