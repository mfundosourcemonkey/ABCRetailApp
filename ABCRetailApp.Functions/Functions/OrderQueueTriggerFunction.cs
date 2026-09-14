using System.Text;
using System.Text.Json;
using ABCRetailApp.Shared.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailApp.Functions.Functions;

/// <summary>
/// Project 2, Part A: "Create a function that reads from/writes to the Azure queue" (read half).
/// Fires automatically whenever a new message lands on the order-processing queue — whether it
/// was written by the web app's Orders page or by <see cref="OrderQueueWriteFunction"/> — and
/// logs it. This is the idiomatic way Azure Functions "reads from" a queue: an event-driven
/// trigger rather than a manual poll.
/// </summary>
public class OrderQueueTriggerFunction
{
    private readonly ILogger<OrderQueueTriggerFunction> _logger;

    public OrderQueueTriggerFunction(ILogger<OrderQueueTriggerFunction> logger)
    {
        _logger = logger;
    }

    [Function("OrderQueueTriggerFunction")]
    public void Run([QueueTrigger("order-processing", Connection = "AzureWebJobsStorage")] string rawMessage)
    {
        var json = DecodeIfBase64(rawMessage);

        try
        {
            var message = JsonSerializer.Deserialize<OrderQueueMessage>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation(
                "Read queue message: {MessageType} / {ItemName} x{Quantity} — {Action} (queued at {CreatedAt}).",
                message?.MessageType, message?.ItemName, message?.Quantity, message?.Action, message?.CreatedAt);
        }
        catch (JsonException)
        {
            _logger.LogInformation("Read queue message (not JSON): {RawMessage}", json);
        }
    }

    // Our QueueStorageService (and Storage Explorer/the Azure portal by default) base64-encode
    // queue message bodies. Decode defensively so the log/screenshot shows readable JSON either way.
    private static string DecodeIfBase64(string value)
    {
        try
        {
            var bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException)
        {
            return value;
        }
    }
}
