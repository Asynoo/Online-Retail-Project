namespace ReviewApi.Infrastructure {
    public interface IServiceGateway<T> {
        Task<T?> Get(int id);
        Task<List<T>?> GetAll();
    }
}
