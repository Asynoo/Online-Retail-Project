using System.Text.Json;
using ReviewApi.Infrastructure;
using SharedModels;

public class CustomerServiceGateway : IServiceGateway<CustomerDto>
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public CustomerServiceGateway(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }

    public async Task<CustomerDto> GetAsync(int id)
    {
        var response = await _httpClient.GetAsync($"{_baseUrl}{id}");
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var customerDto = JsonSerializer.Deserialize<CustomerDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return customerDto;
    }
}