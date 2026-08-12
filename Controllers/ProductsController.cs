using ABCRetailApp.Models;
using ABCRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ITableStorageService<Product> _productTable;
        private readonly IBlobStorageService _blobStorage;
        private readonly IQueueStorageService _queueStorage;
        private readonly IFileShareStorageService _fileShare;

        public ProductsController(
            ITableStorageService<Product> productTable,
            IBlobStorageService blobStorage,
            IQueueStorageService queueStorage,
            IFileShareStorageService fileShare)
        {
            _productTable = productTable;
            _blobStorage = blobStorage;
            _queueStorage = queueStorage;
            _fileShare = fileShare;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productTable.GetAllAsync();
            return View(products.OrderByDescending(p => p.Timestamp).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid || model.ImageFile == null)
            {
                ModelState.AddModelError(string.Empty, "A product image is required.");
                return View(model);
            }

            var (imageUrl, imageFileName) = await _blobStorage.UploadAsync(model.ImageFile);

            var product = new Product
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                ImageUrl = imageUrl,
                ImageFileName = imageFileName
            };
            await _productTable.AddEntityAsync(product);

            await _queueStorage.SendMessageAsync(new OrderQueueMessage
            {
                MessageType = QueueMessageType.Inventory,
                ItemName = imageFileName,
                Action = "Uploading image",
                Quantity = product.StockQuantity
            });

            try
            {
                var logName = $"ProductCreated_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.txt";
                var logContent = $"Product created: {product.Name} (image: {imageFileName}) at {DateTimeOffset.UtcNow:O}";
                await _fileShare.UploadTextAsync(logName, logContent);
            }
            catch (Exception)
            {
                // Azure File Storage may be unavailable in local dev (Azurite does not emulate it);
                // the product record itself is already saved, so this is non-fatal.
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var product = await _productTable.GetEntityAsync(partitionKey, rowKey);
            if (product == null)
            {
                return NotFound();
            }

            return View(new ProductEditViewModel
            {
                PartitionKey = product.PartitionKey,
                RowKey = product.RowKey,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CurrentImageUrl = product.ImageUrl,
                CurrentImageFileName = product.ImageFileName
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var imageUrl = model.CurrentImageUrl;
            var imageFileName = model.CurrentImageFileName;

            if (model.ImageFile != null)
            {
                (imageUrl, imageFileName) = await _blobStorage.UploadAsync(model.ImageFile);
                if (!string.IsNullOrEmpty(model.CurrentImageFileName))
                {
                    await _blobStorage.DeleteAsync(model.CurrentImageFileName);
                }
            }

            var product = new Product
            {
                PartitionKey = model.PartitionKey,
                RowKey = model.RowKey,
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                StockQuantity = model.StockQuantity,
                ImageUrl = imageUrl,
                ImageFileName = imageFileName
            };
            await _productTable.UpdateEntityAsync(product);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey, string imageFileName)
        {
            await _productTable.DeleteEntityAsync(partitionKey, rowKey);
            if (!string.IsNullOrEmpty(imageFileName))
            {
                await _blobStorage.DeleteAsync(imageFileName);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
