using ABCRetailApp.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailApp.Functions.Functions;

/// <summary>
/// Project 2, Part A: "Create a function that writes to Blob Storage."
/// Accepts a binary file body (query string carries the file name / content type) and
/// uploads it to the product-media container, reusing the same IBlobStorageService the
/// web app's ProductsController uses.
/// </summary>
public class UploadProductImageFunction
{
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<UploadProductImageFunction> _logger;

    public UploadProductImageFunction(IBlobStorageService blobStorage, ILogger<UploadProductImageFunction> logger)
    {
        _blobStorage = blobStorage;
        _logger = logger;
    }

    [Function("UploadProductImageFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "products/image")] HttpRequest req)
    {
        var fileName = req.Query["fileName"].ToString();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return new BadRequestObjectResult("Provide the image file name via the 'fileName' query string parameter.");
        }

        var contentType = string.IsNullOrWhiteSpace(req.ContentType) ? "application/octet-stream" : req.ContentType;

        var (url, blobName) = await _blobStorage.UploadAsync(req.Body, fileName, contentType);
        _logger.LogInformation("Uploaded {BlobName} to Blob Storage.", blobName);

        return new OkObjectResult(new { url, blobName });
    }
}
