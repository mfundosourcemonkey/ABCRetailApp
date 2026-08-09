namespace ABCRetailApp.Services
{
    public interface IBlobStorageService
    {
        Task InitializeAsync();
        Task<(string Url, string FileName)> UploadAsync(IFormFile file);
        Task<List<string>> ListBlobNamesAsync();
        Task DeleteAsync(string fileName);
    }
}
