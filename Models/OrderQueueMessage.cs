using System.ComponentModel.DataAnnotations;

namespace ABCRetailApp.Models
{
    public enum QueueMessageType
    {
        Order,
        Inventory
    }

    public class OrderQueueMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Display(Name = "Message Type")]
        public QueueMessageType MessageType { get; set; }

        [Required]
        [Display(Name = "Item / Product Name")]
        public string ItemName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
