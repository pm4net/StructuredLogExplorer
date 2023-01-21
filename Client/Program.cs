global using Microsoft.AspNetCore.Mvc.RazorPages;
global using System.Linq;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using StructuredLogExplorer;

var builder = WebApplication.CreateBuilder(args);

JsonConvert.DefaultSettings = () => new JsonSerializerSettings{ContractResolver = new CamelCasePropertyNamesContractResolver()};

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

app.Run();
