using ABCRetailApp.Shared.Models;

namespace ABCRetailApp.Shared.Services
{
    public interface IQueueStorageService
    {
        Task InitializeAsync();
        Task SendMessageAsync(OrderQueueMessage message);
        Task<List<OrderQueueMessage>> PeekMessagesAsync(int maxMessages = 20);
        Task<OrderQueueMessage?> ReceiveAndDeleteNextAsync();
    }
}
