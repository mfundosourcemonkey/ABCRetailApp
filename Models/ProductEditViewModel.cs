using System.ComponentModel.DataAnnotations;

namespace ABCRetailApp.Models
{
    public class ProductEditViewModel
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        public string CurrentImageUrl { get; set; } = string.Empty;
        public string CurrentImageFileName { get; set; } = string.Empty;

        [Display(Name = "Replace Image (optional)")]
        public IFormFile? ImageFile { get; set; }
    }
}
