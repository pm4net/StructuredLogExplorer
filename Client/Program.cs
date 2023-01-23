global using Microsoft.AspNetCore.Mvc.RazorPages;
global using System.Linq;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using StructuredLogExplorer;

var builder = WebApplication.CreateBuilder(args);

JsonConvert.DefaultSettings = () => new JsonSerializerSettings{ContractResolver = new CamelCasePropertyNamesContractResolver()};

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient().AddOptions();

// Add custom services
builder.Services.AddSingleton<IProjectService>(x => {
    var userDir = builder.Configuration["DataDirectory"];
    var defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "pm4net");
    return new ProjectService(!string.IsNullOrWhiteSpace(userDir) ? userDir : defaultDir);
});

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
app.MapControllers();

app.MapFallback(context =>
{
    if (!context.Request.Path.StartsWithSegments(Urls.ApiSegment))
    {
        context.Response.Redirect(Urls.NotFoundUrl);
    }
    return Task.CompletedTask;
});

app.Run();
