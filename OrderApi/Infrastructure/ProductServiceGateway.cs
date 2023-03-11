using System.Net;
using RestSharp;
using SharedModels;
namespace OrderApi.Infrastructure {
    public class ProductServiceGateway : IServiceGateway<ProductDto> {
        private readonly string _productServiceBaseUrl;

        public ProductServiceGateway(string baseUrl) {
            _productServiceBaseUrl = baseUrl;
        }


        
        public ProductDto? Get(int id) {
            RestClient c = new(_productServiceBaseUrl);
        
            RestRequest request = new(id.ToString());
            Task<ProductDto?> response = c.GetAsync<ProductDto>(request);
            response.Wait();
            return response.Result;
        }

        public List<ProductDto>? GetAll() {
            RestClient c = new(_productServiceBaseUrl);

            RestRequest request = new();
            Task<IEnumerable<ProductDto>?> response = c.GetAsync<IEnumerable<ProductDto>>(request);
            response.Wait();
            return response.Result?.ToList();
        }

        public bool UpdateMany(List<ProductDto> updatedModels) {
            RestClient c = new(_productServiceBaseUrl);

            RestRequest request = new();
            request.AddJsonBody(updatedModels);
            Task<RestResponse> response = c.PutAsync(request);
            response.Wait();
            return response.Result.StatusCode is HttpStatusCode.OK;
        }
    }
}
