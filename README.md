# ABC Retail Cloud

An ASP.NET Core MVC web application built for ABC Retail, an online retailer moving its order
processing system off aging on-premises infrastructure and onto Azure. The app uses all four core
Azure Storage services — Tables, Blobs, Queues, and Files — to store customer profiles, product
catalog data and images, order/inventory transactions, and application logs. Project 2 extends it
with a set of Azure Functions that call into the same storage services, plus a discussion of
Azure Event Hubs and Event Bus for customer experience (see the submission document).

**Live web app:** http://st10501614.azurewebsites.net
**Live Function App:** https://st10501614-functions.azurewebsites.net

## Solution structure

This is a multi-project solution (`ABCRetailAzure.slnx`):

| Project | What it is |
|---|---|
| `ABCRetailApp` | The Project 1 MVC web app (Customers, Products, Orders, Logs) |
| `ABCRetailApp.Shared` | Class library with the Models and Azure Storage service classes, shared by both the web app and the Functions project |
| `ABCRetailApp.Functions` | Project 2's Azure Functions (isolated worker, .NET 10) |

## Features (web app)

| Area | Azure service | What it does |
|---|---|---|
| Customers | Azure Table Storage | Create, edit, delete, and list customer profiles |
| Products | Azure Table Storage + Azure Blob Storage | Create, edit, delete, and list products, with image upload/storage in Blob Storage |
| Orders | Azure Queue Storage | Submit and view order-processing and inventory-update messages |
| Logs | Azure File Storage | Auto-generated log files on key actions, plus manual upload/download |

## Functions (Project 2, `ABCRetailApp.Functions`)

| Function | Trigger | What it does |
|---|---|---|
| `StoreCustomerFunction` | HTTP POST `/api/customers` | Writes a customer profile to Azure Table Storage |
| `UploadProductImageFunction` | HTTP POST `/api/products/image?fileName=...` | Writes a binary file to Azure Blob Storage |
| `OrderQueueWriteFunction` | HTTP POST `/api/orders/queue` | Writes an order/inventory message to Azure Queue Storage |
| `OrderQueueTriggerFunction` | Queue trigger on `order-processing` | Fires automatically on new queue messages — the "reads from the queue" half of the pair above |
| `UploadLogFileFunction` | HTTP POST `/api/logs/file?fileName=...` | Writes a binary file to Azure Files |

All five reuse the exact same `ABCRetailApp.Shared` service classes the web app uses — no storage
logic is duplicated between the two apps.

## Tech stack

- ASP.NET Core MVC, .NET 10
- Azure Functions v4, isolated worker, .NET 10
- Azure.Data.Tables, Azure.Storage.Blobs, Azure.Storage.Queues, Azure.Storage.Files.Shares
- Bootstrap 5 (vendored locally, no external CDN dependency)
- Web app hosted on Azure App Service (Linux, Basic B1); Functions hosted on the same App Service Plan

## Running locally

Both apps target Azure Storage via connection strings in `appsettings.Development.json` (web app)
and `local.settings.json` (Functions, not committed to source control). For local development, run
[Azurite](https://github.com/Azure/Azurite) (the Azure Storage emulator):

```bash
docker run -d --name azurite \
  -p 10000:10000 -p 10001:10001 -p 10002:10002 -p 10003:10003 \
  mcr.microsoft.com/azure-storage/azurite \
  azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 --skipApiVersionCheck
```

> Note: Azurite does not emulate Azure Files. Both apps degrade gracefully when the File Share is
> unreachable — this only affects local development; File Storage works normally once deployed
> against a real Azure Storage account.

Then run the web app:

```bash
dotnet run --project ABCRetailApp
```

Or the Functions project (requires [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)):

```bash
cd ABCRetailApp.Functions
func start
```

## Deployment

- **Web app**: `dotnet publish` + `az webapp deploy` (zip deploy)
- **Functions**: `func azure functionapp publish st10501614-functions --dotnet-isolated`

Real storage connection strings are set as Application Settings on each Azure resource
(`AzureStorage__ConnectionString` etc.) rather than committed to source control.
