using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetailApp.Shared.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public BlobStorageService(string connectionString, string containerName)
        {
            _containerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task InitializeAsync()
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        public async Task<(string Url, string FileName)> UploadAsync(Stream content, string fileName, string contentType)
        {
            var blobName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });
            return (blobClient.Uri.ToString(), blobName);
        }

        public async Task<List<string>> ListBlobNamesAsync()
        {
            var names = new List<string>();
            await foreach (var blobItem in _containerClient.GetBlobsAsync())
            {
                names.Add(blobItem.Name);
            }
            return names;
        }

        public async Task DeleteAsync(string fileName)
        {
            await _containerClient.DeleteBlobIfExistsAsync(fileName);
        }
    }
}
