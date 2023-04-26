using SharedModels;
namespace ReviewApi.Infrastructure;

public interface IServiceGateway<T>
{
    Task<T?> GetAsync(int id);
}