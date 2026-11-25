using Bison.Domain.Models;
using Bison.Razor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

/*
 * Unit tests for the UserIdentity functionality.
 */
public class UserIdentityUnitTests
{
    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<BisonDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentityCore<Author>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<BisonDbContext>()
            .AddDefaultTokenProviders();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task CreateAsync_AllowsValidUser()
    {
        using var provider = BuildServices();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();

        var user = new Author
        {
            UserName = "alice",
            Email = "alice@example.com"
        };

        var result = await userManager.CreateAsync(user, "Password123!");

        Assert.True(result.Succeeded);
        Assert.NotNull(await userManager.FindByEmailAsync("alice@example.com"));
    }

    [Fact]
    public async Task CreateAsync_RejectsWeakPassword()
    {
        using var provider = BuildServices();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();

        var user = new Author
        {
            UserName = "bob",
            Email = "bob@example.com"
        };

        var result = await userManager.CreateAsync(user, "123");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "PasswordTooShort");
    }

    [Fact]
    public async Task CheckPasswordAsync_ReturnsTrueForCorrectPassword()
    {
        using var provider = BuildServices();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();

        var user = new Author
        {
            UserName = "carol",
            Email = "carol@example.com"
        };

        var createResult = await userManager.CreateAsync(user, "Password123!");

        Assert.True(createResult.Succeeded);

        var isValid = await userManager.CheckPasswordAsync(user, "Password123!");
        var isWrong = await userManager.CheckPasswordAsync(user, "WrongPassword!");

        Assert.True(isValid);
        Assert.False(isWrong);
    }

    [Fact]
    public async Task CreateAsync_RejectsDuplicateEmail()
    {
        using var provider = BuildServices();
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();

        var first = new Author
        {
            UserName = "dan",
            Email = "dan@example.com"
        };

        var second = new Author
        {
            UserName = "daniel",
            Email = "dan@example.com"
        };

        var firstResult = await userManager.CreateAsync(first, "Password123!");
        var secondResult = await userManager.CreateAsync(second, "Password123!");

        Assert.True(firstResult.Succeeded);
        Assert.False(secondResult.Succeeded);
        Assert.Contains(secondResult.Errors, e => e.Code == "DuplicateEmail");
    }
}