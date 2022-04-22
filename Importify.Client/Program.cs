using Blazored.LocalStorage;
using IgniteUI.Blazor.Controls;
using Importify.Client;
using Importify.Client.Service;
using Importify.Client.Service.Logic;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(typeof(IIgniteUIBlazor), typeof(IgniteUIBlazor));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBasicService, BasicService>();
builder.Services.AddScoped<IPlotService, PlotService>();
builder.Services.AddScoped<IExcelGenerator, ExcelGenerator>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredLocalStorage(config =>
    config.JsonSerializerOptions.WriteIndented = true);

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

await builder.Build().RunAsync();
