using Azure.Data.Tables;

namespace ABCRetailApp.Services
{
    public interface ITableStorageService<T> where T : class, ITableEntity, new()
    {
        Task InitializeAsync();
        Task AddEntityAsync(T entity);
        Task<List<T>> GetAllAsync();
        Task<T?> GetEntityAsync(string partitionKey, string rowKey);
        Task DeleteEntityAsync(string partitionKey, string rowKey);
    }
}
