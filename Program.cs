using System.Globalization;
using ABCRetailApp.Models;
using ABCRetailApp.Services;
using Microsoft.AspNetCore.Localization;

// Pin form parsing (e.g. Price="99.99") to a fixed culture, independent of the host OS locale.
var appCulture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = appCulture;
CultureInfo.DefaultThreadCurrentUICulture = appCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var azureStorageOptions = builder.Configuration.GetSection("AzureStorage").Get<AzureStorageOptions>()
    ?? throw new InvalidOperationException("Missing 'AzureStorage' configuration section.");
builder.Services.AddSingleton(azureStorageOptions);

builder.Services.AddSingleton<ITableStorageService<CustomerProfile>>(_ =>
    new TableStorageService<CustomerProfile>(azureStorageOptions.ConnectionString, azureStorageOptions.TableName));
builder.Services.AddSingleton<ITableStorageService<Product>>(_ =>
    new TableStorageService<Product>(azureStorageOptions.ConnectionString, azureStorageOptions.ProductTableName));
builder.Services.AddSingleton<IBlobStorageService>(_ =>
    new BlobStorageService(azureStorageOptions.ConnectionString, azureStorageOptions.BlobContainerName));
builder.Services.AddSingleton<IQueueStorageService>(_ =>
    new QueueStorageService(azureStorageOptions.ConnectionString, azureStorageOptions.QueueName));
builder.Services.AddSingleton<IFileShareStorageService>(_ =>
    new FileShareStorageService(azureStorageOptions.ConnectionString, azureStorageOptions.FileShareName));

var app = builder.Build();

// Ensure the underlying Azure Storage table/container/queue/share exist before handling requests.
// Each is initialized independently: Azurite (used for local dev) does not emulate Azure Files,
// so that one initialization can fail locally without preventing the rest of the app from starting.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var startupLogger = services.GetRequiredService<ILogger<Program>>();

    async Task TryInitializeAsync(string name, Func<Task> initialize)
    {
        try
        {
            await initialize();
        }
        catch (Exception ex)
        {
            startupLogger.LogWarning(ex, "Failed to initialize {StorageService}. It may be unavailable in this environment.", name);
        }
    }

    await TryInitializeAsync("Customer table", () => services.GetRequiredService<ITableStorageService<CustomerProfile>>().InitializeAsync());
    await TryInitializeAsync("Product table", () => services.GetRequiredService<ITableStorageService<Product>>().InitializeAsync());
    await TryInitializeAsync("Blob container", () => services.GetRequiredService<IBlobStorageService>().InitializeAsync());
    await TryInitializeAsync("Queue", () => services.GetRequiredService<IQueueStorageService>().InitializeAsync());
    await TryInitializeAsync("File share", () => services.GetRequiredService<IFileShareStorageService>().InitializeAsync());
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(appCulture),
    SupportedCultures = new[] { appCulture },
    SupportedUICultures = new[] { appCulture }
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
