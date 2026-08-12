# ABC Retail Cloud

An ASP.NET Core MVC web application built for ABC Retail, an online retailer moving its order
processing system off aging on-premises infrastructure and onto Azure. The app uses all four core
Azure Storage services — Tables, Blobs, Queues, and Files — to store customer profiles, product
catalog data and images, order/inventory transactions, and application logs.

**Live app:** http://st10501614.azurewebsites.net

## Features

| Area | Azure service | What it does |
|---|---|---|
| Customers | Azure Table Storage | Create, edit, delete, and list customer profiles |
| Products | Azure Table Storage + Azure Blob Storage | Create, edit, delete, and list products, with image upload/storage in Blob Storage |
| Orders | Azure Queue Storage | Submit and view order-processing and inventory-update messages |
| Logs | Azure File Storage | Auto-generated log files on key actions, plus manual upload/download |

## Tech stack

- ASP.NET Core MVC, .NET 10
- Azure.Data.Tables, Azure.Storage.Blobs, Azure.Storage.Queues, Azure.Storage.Files.Shares
- Bootstrap 5 (vendored locally, no external CDN dependency)
- Hosted on Azure App Service (Linux, Basic B1)

## Project structure

```
Controllers/    Customers, Products, Orders, Logs, Home
Models/         Table entities (CustomerProfile, Product), queue message, view models
Services/       One service per Azure Storage primitive (Table, Blob, Queue, File Share)
Views/          Razor views per controller
```

Each Azure Storage service is wrapped in its own service class under `Services/`, registered in
`Program.cs`, and injected into the relevant controllers. `ITableStorageService<T>` is generic so
both `CustomerProfile` and `Product` entities share the same Table Storage CRUD implementation.

## Running locally

The app targets Azure Storage via the connection string in `appsettings.json` /
`appsettings.Development.json`. For local development, run [Azurite](https://github.com/Azure/Azurite)
(the Azure Storage emulator):

```bash
docker run -d --name azurite \
  -p 10000:10000 -p 10001:10001 -p 10002:10002 -p 10003:10003 \
  mcr.microsoft.com/azure-storage/azurite \
  azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0 --skipApiVersionCheck
```

> Note: Azurite does not emulate Azure Files. The app degrades gracefully when the File Share is
> unreachable (a warning is shown on the Logs page instead of an error) — this only affects local
> development; File Storage works normally once deployed against a real Azure Storage account.

Then run the app:

```bash
dotnet run
```

## Deployment

Deployed to Azure App Service via `dotnet publish` + `az webapp deploy` (zip deploy). The real
storage account connection string is set as the `AzureStorage__ConnectionString` App Service
application setting rather than committed to source control.
