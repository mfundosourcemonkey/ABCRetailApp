using ABCRetailApp.Models;
using ABCRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailApp.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IQueueStorageService _queueStorage;
        private readonly IFileShareStorageService _fileShare;

        public OrdersController(IQueueStorageService queueStorage, IFileShareStorageService fileShare)
        {
            _queueStorage = queueStorage;
            _fileShare = fileShare;
        }

        public async Task<IActionResult> Index()
        {
            var messages = await _queueStorage.PeekMessagesAsync();
            return View(messages);
        }

        public IActionResult Create()
        {
            return View(new OrderQueueMessage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderQueueMessage message)
        {
            if (!ModelState.IsValid)
            {
                return View(message);
            }

            message.Action = message.MessageType == QueueMessageType.Order ? "Processing order" : "Updating inventory";
            message.CreatedAt = DateTimeOffset.UtcNow;
            await _queueStorage.SendMessageAsync(message);

            try
            {
                var logName = $"{message.MessageType}Queued_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.txt";
                var logContent = $"{message.Action}: {message.ItemName} x{message.Quantity} at {message.CreatedAt:O}";
                await _fileShare.UploadTextAsync(logName, logContent);
            }
            catch (Exception)
            {
                // Azure File Storage may be unavailable in local dev (Azurite does not emulate it);
                // the queue message itself is already sent, so this is non-fatal.
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessNext()
        {
            await _queueStorage.ReceiveAndDeleteNextAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
