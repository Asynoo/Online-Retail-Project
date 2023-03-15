﻿using System.Net;
using RestSharp;
using SharedModels;
namespace OrderApi.Infrastructure {
    public class ProductServiceGateway : IServiceGateway<ProductDto> {
        private readonly string _productServiceBaseUrl;

        public ProductServiceGateway(string baseUrl) {
            _productServiceBaseUrl = baseUrl;
        }

        public async Task<ProductDto?> Get(int id) {
            RestClient c = new(_productServiceBaseUrl);
        
            RestRequest request = new(id.ToString());
            ProductDto? response = await c.GetAsync<ProductDto>(request);
            return response;
        }

        public async Task<List<ProductDto>?> GetAll() {
            RestClient c = new(_productServiceBaseUrl);

            RestRequest request = new();
            IEnumerable<ProductDto>? response = await c.GetAsync<IEnumerable<ProductDto>>(request);
            return response?.ToList();
        }

        public async Task<bool> UpdateMany(List<ProductDto> updatedModels) {
            RestClient c = new(_productServiceBaseUrl);

            RestRequest request = new();
            request.AddJsonBody(updatedModels);
            RestResponse response = await c.PutAsync(request);
            return response.StatusCode is HttpStatusCode.OK;
        }
    }
}
