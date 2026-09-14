namespace ABCRetailApp.Shared.Services
{
    public interface IBlobStorageService
    {
        Task InitializeAsync();
        Task<(string Url, string FileName)> UploadAsync(Stream content, string fileName, string contentType);
        Task<List<string>> ListBlobNamesAsync();
        Task DeleteAsync(string fileName);
    }
}
