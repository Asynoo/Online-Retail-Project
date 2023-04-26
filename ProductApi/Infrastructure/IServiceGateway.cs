namespace ProductApi.Infrastructure {
    public interface IServiceGateway<T> {
        Task<T?> Get(int id);
        Task<List<T>?> GetAll();
        Task<List<T>?> GetForProduct(int productId);
    }
}
