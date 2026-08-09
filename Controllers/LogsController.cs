using ABCRetailApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetailApp.Controllers
{
    public class LogsController : Controller
    {
        private readonly IFileShareStorageService _fileShare;

        public LogsController(IFileShareStorageService fileShare)
        {
            _fileShare = fileShare;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var files = await _fileShare.ListFileNamesAsync();
                return View(files.OrderByDescending(f => f).ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Azure File Storage is unreachable right now: " + ex.Message;
                return View(new List<string>());
            }
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please choose a file to upload.");
                return View();
            }

            try
            {
                await _fileShare.UploadFileAsync(file);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Azure File Storage is unreachable right now: " + ex.Message);
                return View();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Download(string fileName)
        {
            var (content, contentType) = await _fileShare.DownloadAsync(fileName);
            return File(content, contentType, fileName);
        }
    }
}
