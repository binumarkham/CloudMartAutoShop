using System.Globalization;
using CloudMartAutoShop.Pwa;
using CloudMartAutoShop.Pwa.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var culture = new CultureInfo("en-CA");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(
        builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5169/")
});

builder.Services.AddScoped<ApiService>();

await builder.Build().RunAsync();
