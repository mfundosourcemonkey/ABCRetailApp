using ABCRetailApp.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailApp.Functions.Functions;

/// <summary>
/// Project 2, Part A: "Create a function that sends a file to Azure Files."
/// Accepts a binary file body (query string carries the file name) and writes it to the
/// abcretaillogs file share, reusing the same IFileShareStorageService the web app's
/// LogsController uses.
/// </summary>
public class UploadLogFileFunction
{
    private readonly IFileShareStorageService _fileShare;
    private readonly ILogger<UploadLogFileFunction> _logger;

    public UploadLogFileFunction(IFileShareStorageService fileShare, ILogger<UploadLogFileFunction> logger)
    {
        _fileShare = fileShare;
        _logger = logger;
    }

    [Function("UploadLogFileFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "logs/file")] HttpRequest req)
    {
        var fileName = req.Query["fileName"].ToString();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new BadRequestObjectResult("Provide the file name via the 'fileName' query string parameter.");
        }

        await _fileShare.UploadFileAsync(req.Body, fileName);
        _logger.LogInformation("Uploaded {FileName} to Azure Files.", fileName);

        return new OkObjectResult(new { fileName });
    }
}
