using System.Text.Json;
using ReviewApi.Infrastructure;
using SharedModels;

public class ProductServiceGateway : IServiceGateway<ProductDto>
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ProductServiceGateway(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }

    public async Task<ProductDto> GetAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}{id}");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var productDto = JsonSerializer.Deserialize<ProductDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return productDto;
    }
}