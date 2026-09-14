using System.Text;
using Azure.Storage.Files.Shares;

namespace ABCRetailApp.Shared.Services
{
    public class FileShareStorageService : IFileShareStorageService
    {
        private readonly ShareClient _shareClient;
        private readonly ShareDirectoryClient _rootDirectory;

        public FileShareStorageService(string connectionString, string shareName)
        {
            var options = new ShareClientOptions();
            options.Retry.MaxRetries = 2;
            options.Retry.Delay = TimeSpan.FromMilliseconds(200);
            _shareClient = new ShareClient(connectionString, shareName, options);
            _rootDirectory = _shareClient.GetRootDirectoryClient();
        }

        public async Task InitializeAsync()
        {
            await _shareClient.CreateIfNotExistsAsync();
        }

        public async Task UploadTextAsync(string fileName, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            using var stream = new MemoryStream(bytes);
            var fileClient = _rootDirectory.GetFileClient(fileName);
            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadAsync(stream);
        }

        public async Task UploadFileAsync(Stream content, string fileName)
        {
            // ShareFileClient.CreateAsync needs the exact size upfront, so a non-seekable
            // stream (e.g. an HTTP request body) must be buffered first to know its length.
            if (!content.CanSeek)
            {
                var buffered = new MemoryStream();
                await content.CopyToAsync(buffered);
                buffered.Position = 0;
                content = buffered;
            }

            var fileClient = _rootDirectory.GetFileClient(fileName);
            await fileClient.CreateAsync(content.Length);
            await fileClient.UploadAsync(content);
        }

        public async Task<List<string>> ListFileNamesAsync()
        {
            var names = new List<string>();
            await foreach (var item in _rootDirectory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    names.Add(item.Name);
                }
            }
            return names;
        }

        public async Task<(Stream Content, string ContentType)> DownloadAsync(string fileName)
        {
            var fileClient = _rootDirectory.GetFileClient(fileName);
            var download = await fileClient.DownloadAsync();
            return (download.Value.Content, download.Value.ContentType ?? "application/octet-stream");
        }
    }
}
