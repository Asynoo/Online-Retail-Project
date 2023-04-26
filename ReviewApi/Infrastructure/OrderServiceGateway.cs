using System.Net;
using System.Text.Json;
using SharedModels;

namespace ReviewApi.Infrastructure; 

public class OrderServiceGateway : IServiceGateway<OrderDto>
{
    private readonly HttpClient _httpClient;

    public OrderServiceGateway(string baseUrl)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<OrderDto?> GetAsync(int id)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"orders/{id}");
        if (response.StatusCode != HttpStatusCode.OK) {
            return null;
        }
        string responseContent = await response.Content.ReadAsStringAsync();
        OrderDto? order = JsonSerializer.Deserialize<OrderDto>(responseContent, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        });
        return order;
    }
}