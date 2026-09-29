using System;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
var dbPath = Environment.GetEnvironmentVariable("BISONDBPATH")
             ?? Path.Combine(Path.GetTempPath(), "bison.db");
builder.Services.AddSingleton(new DBFacade(dbPath));
builder.Services.AddScoped<IObservationService, ObservationService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.MapGet("/", () => Results.Redirect("/obs")); //Redirects to the observations instead of getting 404. 
app.Run();