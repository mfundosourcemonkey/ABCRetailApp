using System.Text.Json;
using ABCRetailApp.Shared.Models;
using ABCRetailApp.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailApp.Functions.Functions;

/// <summary>
/// Project 2, Part A: "Create a function that reads from/writes to the Azure queue" (write half).
/// Accepts an order/inventory message as JSON and writes it to the order-processing queue,
/// reusing the same IQueueStorageService the web app's OrdersController uses.
/// The read half is <see cref="OrderQueueTriggerFunction"/>, which fires automatically when a
/// message like this one lands on the queue.
/// </summary>
public class OrderQueueWriteFunction
{
    private readonly IQueueStorageService _queueStorage;
    private readonly ILogger<OrderQueueWriteFunction> _logger;

    public OrderQueueWriteFunction(IQueueStorageService queueStorage, ILogger<OrderQueueWriteFunction> logger)
    {
        _queueStorage = queueStorage;
        _logger = logger;
    }

    [Function("OrderQueueWriteFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders/queue")] HttpRequest req)
    {
        OrderQueueMessage? message;
        try
        {
            message = await JsonSerializer.DeserializeAsync<OrderQueueMessage>(req.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Request body must be valid JSON matching OrderQueueMessage.");
        }

        if (message == null || string.IsNullOrWhiteSpace(message.ItemName))
        {
            return new BadRequestObjectResult("ItemName is required.");
        }

        message.Action = message.MessageType == QueueMessageType.Order ? "Processing order" : "Updating inventory";
        message.CreatedAt = DateTimeOffset.UtcNow;

        await _queueStorage.SendMessageAsync(message);
        _logger.LogInformation("Wrote {MessageType} message for {ItemName} to the order-processing queue.",
            message.MessageType, message.ItemName);

        return new OkObjectResult(message);
    }
}
