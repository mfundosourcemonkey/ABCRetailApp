namespace ABCRetailApp.Shared.Services
{
    public interface IFileShareStorageService
    {
        Task InitializeAsync();
        Task UploadTextAsync(string fileName, string content);
        Task UploadFileAsync(Stream content, string fileName);
        Task<List<string>> ListFileNamesAsync();
        Task<(Stream Content, string ContentType)> DownloadAsync(string fileName);
    }
}
