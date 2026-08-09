using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetailApp.Services
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

        public async Task<(string Url, string FileName)> UploadAsync(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var blobClient = _containerClient.GetBlobClient(fileName);
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            return (blobClient.Uri.ToString(), fileName);
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
