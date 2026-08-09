namespace ABCRetailApp.Models
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string ProductTableName { get; set; } = string.Empty;
        public string BlobContainerName { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public string FileShareName { get; set; } = string.Empty;
    }
}
