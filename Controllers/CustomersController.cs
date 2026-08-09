using ABCRetailApp.Models;
using ABCRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ITableStorageService<CustomerProfile> _customerTable;
        private readonly IFileShareStorageService _fileShare;

        public CustomersController(ITableStorageService<CustomerProfile> customerTable, IFileShareStorageService fileShare)
        {
            _customerTable = customerTable;
            _fileShare = fileShare;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _customerTable.GetAllAsync();
            return View(customers.OrderByDescending(c => c.Timestamp).ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            await _customerTable.AddEntityAsync(customer);

            try
            {
                var logName = $"CustomerCreated_{DateTimeOffset.UtcNow:yyyyMMdd_HHmmss}.txt";
                var logContent = $"Customer created: {customer.FullName} ({customer.Email}) at {DateTimeOffset.UtcNow:O}";
                await _fileShare.UploadTextAsync(logName, logContent);
            }
            catch (Exception)
            {
                // Azure File Storage may be unavailable in local dev (Azurite does not emulate it);
                // the customer record itself is already saved, so this is non-fatal.
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _customerTable.DeleteEntityAsync(partitionKey, rowKey);
            return RedirectToAction(nameof(Index));
        }
    }
}
