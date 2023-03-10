namespace OrderApi.Infrastructure;

public interface IServiceGateway<T>
{
    T? Get(int id);
    List<T>? GetAll();
    bool UpdateMany(List<T> updatedModels);
}