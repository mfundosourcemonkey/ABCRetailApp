using System.ComponentModel.DataAnnotations;

namespace ABCRetailApp.Models
{
    public class ProductCreateViewModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public double Price { get; set; }

        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Required]
        [Display(Name = "Product Image")]
        public IFormFile? ImageFile { get; set; }
    }
}
