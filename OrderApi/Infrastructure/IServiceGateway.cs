using SharedModels;

namespace OrderApi.Infrastructure {
    public interface IServiceGateway<T> {
        Task<T?> Get(int id);
        Task<List<T>?> GetAll();
        Task<bool> UpdateMany(List<T> updatedModels);
    }
}
