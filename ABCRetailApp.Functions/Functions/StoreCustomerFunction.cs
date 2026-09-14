using System.Text.Json;
using ABCRetailApp.Shared.Models;
using ABCRetailApp.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetailApp.Functions.Functions;

/// <summary>
/// Project 2, Part A: "Create a function that stores information in Azure tables."
/// Accepts a customer profile as JSON and writes it to the CustomerProfiles table,
/// reusing the same ITableStorageService the web app's CustomersController uses.
/// </summary>
public class StoreCustomerFunction
{
    private readonly ITableStorageService<CustomerProfile> _customerTable;
    private readonly ILogger<StoreCustomerFunction> _logger;

    public StoreCustomerFunction(ITableStorageService<CustomerProfile> customerTable, ILogger<StoreCustomerFunction> logger)
    {
        _customerTable = customerTable;
        _logger = logger;
    }

    [Function("StoreCustomerFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "customers")] HttpRequest req)
    {
        CustomerProfile? customer;
        try
        {
            customer = await JsonSerializer.DeserializeAsync<CustomerProfile>(req.Body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return new BadRequestObjectResult("Request body must be valid JSON matching CustomerProfile.");
        }

        if (customer == null || string.IsNullOrWhiteSpace(customer.FullName))
        {
            return new BadRequestObjectResult("FullName is required.");
        }

        await _customerTable.AddEntityAsync(customer);
        _logger.LogInformation("Stored customer {FullName} in Azure Table Storage ({PartitionKey}/{RowKey}).",
            customer.FullName, customer.PartitionKey, customer.RowKey);

        return new OkObjectResult(customer);
    }
}
