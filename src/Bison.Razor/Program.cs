using System;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
var dbPath = Environment.GetEnvironmentVariable("BISONDBPATH")
             ?? Path.Combine(Path.GetTempPath(), "bison.db");
builder.Services.AddDbContext<BisonDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<DBFacade>(); // Scoped, since DbContext is scoped
builder.Services.AddScoped<IObservationService, ObservationService>();


var app = builder.Build();

// Create/update the database from the migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BisonDbContext>();
    context.Database.Migrate();
    DbInitializer.SeedDatabase(context);
}

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

public partial class Program { }