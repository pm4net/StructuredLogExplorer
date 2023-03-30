global using Microsoft.AspNetCore.Mvc.RazorPages;
global using System.Linq;
using System.Net;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Globalization;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Localization;
using StructuredLogExplorer;

var builder = WebApplication.CreateBuilder(args);

JsonConvert.DefaultSettings = () => new JsonSerializerSettings{ContractResolver = new CamelCasePropertyNamesContractResolver()};

// Configure web host
builder.WebHost.UseElectron(args);

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient().AddOptions();
builder.Services.AddMvcCore().AddApiExplorer();
builder.Services.AddSwaggerDocument();
builder.Services.AddOutputCache();

// Add custom services (scoped instead of singletons to avoid mutex issues when using shared LiteDb connections (https://github.com/mbdavid/LiteDB/issues/1546#issuecomment-1321174469))
builder.Services.AddScoped<IProjectService>(_ => {
    var userDir = builder.Configuration["DataDirectory"];
    var defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "pm4net");
    return new ProjectService(!string.IsNullOrWhiteSpace(userDir) ? userDir : defaultDir);
});

builder.Services.AddScoped<ILogFileService>(sp =>
{
    var projectService = sp.GetService<IProjectService>();
    return new LogFileService(projectService!);
});

builder.Services.AddScoped<ITextSizeService>(_ => new TextSizeService());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseExceptionHandler(options =>
{
    // https://stackoverflow.com/a/47142207/2102106
    options.Run(async context =>
    {
        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
        context.Response.ContentType = "plain/text";
        var ex = context.Features.Get<IExceptionHandlerFeature>();
        if (ex != null)
        {
            var err = $"{ex.Error.Message}";
            await context.Response.WriteAsync(err).ConfigureAwait(false);
        }
    });
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseOutputCache();
app.UseOpenApi();
app.UseSwaggerUi3(settings =>
{
    settings.Path = "/swagger";
    settings.DocumentPath = "/api/spec.json";
});

app.MapRazorPages();
app.MapControllers();

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
        AutoHideMenuBar = true,
        Width = 1200,
        Height = 800
    });
    
    window.OnClosed += () => Electron.App.Quit();
}