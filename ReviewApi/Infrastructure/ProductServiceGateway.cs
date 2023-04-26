using System.Net;
using System.Text.Json;
using SharedModels;

namespace ReviewApi.Infrastructure; 

public class ProductServiceGateway : IServiceGateway<ProductDto>
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ProductServiceGateway(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }

    public async Task<ProductDto?> GetAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}{id}");
        if (response.StatusCode != HttpStatusCode.OK) {
            return null;
        }
        string responseContent = await response.Content.ReadAsStringAsync();
        ProductDto? productDto = JsonSerializer.Deserialize<ProductDto>(responseContent, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });
        return productDto;
    }
}