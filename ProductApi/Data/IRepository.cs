﻿using ProductApi.Models;

namespace ProductApi.Data {
    public interface IRepository<T> {
        Task<IEnumerable<T>> GetAll();
        Task<T?> Get(int id);
        Task<T> Add(T entity);
        Task<bool> Edit(T entity);
        Task<bool> Edit(List<T> entityList);
        Task<bool> Remove(int id);
    }
}
