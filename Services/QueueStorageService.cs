using System.Text.Json;
using ABCRetailApp.Models;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace ABCRetailApp.Services
{
    public class QueueStorageService : IQueueStorageService
    {
        private readonly QueueClient _queueClient;

        public QueueStorageService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
        }

        public async Task InitializeAsync()
        {
            await _queueClient.CreateIfNotExistsAsync();
        }

        public async Task SendMessageAsync(OrderQueueMessage message)
        {
            var json = JsonSerializer.Serialize(message);
            await _queueClient.SendMessageAsync(json);
        }

        public async Task<List<OrderQueueMessage>> PeekMessagesAsync(int maxMessages = 20)
        {
            var results = new List<OrderQueueMessage>();
            PeekedMessage[] peeked = await _queueClient.PeekMessagesAsync(maxMessages);
            foreach (var msg in peeked)
            {
                var deserialized = JsonSerializer.Deserialize<OrderQueueMessage>(msg.MessageText);
                if (deserialized != null)
                {
                    results.Add(deserialized);
                }
            }
            return results;
        }

        public async Task<OrderQueueMessage?> ReceiveAndDeleteNextAsync()
        {
            var response = await _queueClient.ReceiveMessageAsync();
            var msg = response.Value;
            if (msg == null)
            {
                return null;
            }

            var deserialized = JsonSerializer.Deserialize<OrderQueueMessage>(msg.MessageText);
            await _queueClient.DeleteMessageAsync(msg.MessageId, msg.PopReceipt);
            return deserialized;
        }
    }
}
