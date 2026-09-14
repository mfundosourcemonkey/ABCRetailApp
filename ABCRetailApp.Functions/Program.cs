using ABCRetailApp.Shared.Models;
using ABCRetailApp.Shared.Services;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var azureStorageOptions = builder.Configuration.GetSection("AzureStorage").Get<AzureStorageOptions>()
    ?? throw new InvalidOperationException("Missing 'AzureStorage' configuration section.");
builder.Services.AddSingleton(azureStorageOptions);

builder.Services.AddSingleton<ITableStorageService<CustomerProfile>>(_ =>
    new TableStorageService<CustomerProfile>(azureStorageOptions.ConnectionString, azureStorageOptions.TableName));
builder.Services.AddSingleton<IBlobStorageService>(_ =>
    new BlobStorageService(azureStorageOptions.ConnectionString, azureStorageOptions.BlobContainerName));
builder.Services.AddSingleton<IQueueStorageService>(_ =>
    new QueueStorageService(azureStorageOptions.ConnectionString, azureStorageOptions.QueueName));
builder.Services.AddSingleton<IFileShareStorageService>(_ =>
    new FileShareStorageService(azureStorageOptions.ConnectionString, azureStorageOptions.FileShareName));

builder.Build().Run();
