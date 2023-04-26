using System.Net;
using System.Text.Json;
using SharedModels;

namespace ReviewApi.Infrastructure; 

public class CustomerServiceGateway : IServiceGateway<CustomerDto>
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public CustomerServiceGateway(string baseUrl)
    {
        _httpClient = new HttpClient();
        _baseUrl = baseUrl;
    }

    public async Task<CustomerDto?> GetAsync(int id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}{id}");
        if (response.StatusCode != HttpStatusCode.OK) {
            return null;
        }
        string responseContent = await response.Content.ReadAsStringAsync();
        CustomerDto? customerDto = JsonSerializer.Deserialize<CustomerDto>(responseContent, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });
        return customerDto;
    }
}