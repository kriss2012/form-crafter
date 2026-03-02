using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FormMaker.Client;
using FormMaker.Client.Services;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Default HttpClient for the Blazor app itself
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Load configuration
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var configFileName = builder.HostEnvironment.IsDevelopment()
    ? "appsettings.Development.json"
    : "appsettings.json";

try
{
    var config = await http.GetFromJsonAsync<Dictionary<string, string>>(configFileName);
    var apiBaseUrl = config?["ApiBaseUrl"] ?? "http://localhost:7071/api/";

    // Named HttpClient for API calls
    builder.Services.AddHttpClient("FormMakerAPI", client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
}
catch
{
    // Fallback to localhost if config can't be loaded
    builder.Services.AddHttpClient("FormMakerAPI", client =>
    {
        client.BaseAddress = new Uri("http://localhost:7071/api/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });
}

// Add MudBlazor services with custom configuration
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.MaxDisplayedSnackbars = 3;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 3000;
});

// Add application services
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<HistoryService>();
builder.Services.AddScoped<AccessibilityService>();
builder.Services.AddScoped<AccessibilityAuditService>();
builder.Services.AddScoped<PdfExportService>();

// Add API service with the named HttpClient
builder.Services.AddScoped<ApiService>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("FormMakerAPI");
    var localStorage = sp.GetRequiredService<LocalStorageService>();
    return new ApiService(httpClient, localStorage);
});

// Add authentication services
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

await builder.Build().RunAsync();
