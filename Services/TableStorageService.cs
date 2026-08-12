using Azure;
using Azure.Data.Tables;

namespace ABCRetailApp.Services
{
    public class TableStorageService<T> : ITableStorageService<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public TableStorageService(string connectionString, string tableName)
        {
            _tableClient = new TableClient(connectionString, tableName);
        }

        public async Task InitializeAsync()
        {
            await _tableClient.CreateIfNotExistsAsync();
        }

        public async Task AddEntityAsync(T entity)
        {
            await _tableClient.AddEntityAsync(entity);
        }

        public async Task UpdateEntityAsync(T entity)
        {
            await _tableClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
        }

        public async Task<List<T>> GetAllAsync()
        {
            var results = new List<T>();
            await foreach (var entity in _tableClient.QueryAsync<T>())
            {
                results.Add(entity);
            }
            return results;
        }

        public async Task<T?> GetEntityAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<T>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            await _tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }
    }
}
