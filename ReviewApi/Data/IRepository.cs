namespace ReviewApi.Data {
    public interface IRepository<T> {
        Task<IEnumerable<T>> GetAll(int? productId = null, int? customerId = null);
        Task<T?> Get(int id);
        Task<T?> Add(T entity);
        Task<bool> Edit(T entity);
        Task<bool> Remove(int id);
    }
}
