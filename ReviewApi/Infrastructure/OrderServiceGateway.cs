using ReviewApi.Infrastructure;
using SharedModels;

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
        if (response.IsSuccessStatusCode)
        {
            OrderDto order = await response.Content.ReadFromJsonAsync<OrderDto>();
            return order;
        }
        return null;
    }
}