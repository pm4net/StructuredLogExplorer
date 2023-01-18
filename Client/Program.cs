global using Microsoft.AspNetCore.Mvc.RazorPages;
global using System.Linq;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Globalization;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Localization;
using StructuredLogExplorer;

JsonConvert.DefaultSettings = () => new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

var builder = WebApplication.CreateBuilder(args);

// Configure web host
builder.WebHost.UseElectron(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient().AddOptions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(Urls.ErrorUrl);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapFallback(context =>
{
    if (!context.Request.Path.StartsWithSegments(Urls.ApiSegment))
    {
        context.Response.Redirect(Urls.NotFoundUrl);
    }
    return Task.CompletedTask;
});

if (HybridSupport.IsElectronActive) {
    CreateElectronWindow();
}

app.Run();

static async void CreateElectronWindow()
{
    BrowserWindow window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        AutoHideMenuBar = true
    });
    
    window.OnClosed += () => Electron.App.Quit();
}