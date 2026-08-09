namespace ABCRetailApp.Services
{
    public interface IFileShareStorageService
    {
        Task InitializeAsync();
        Task UploadTextAsync(string fileName, string content);
        Task UploadFileAsync(IFormFile file);
        Task<List<string>> ListFileNamesAsync();
        Task<(Stream Content, string ContentType)> DownloadAsync(string fileName);
    }
}
