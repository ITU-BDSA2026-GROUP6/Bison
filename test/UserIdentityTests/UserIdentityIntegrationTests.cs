using System.Net;
using System.Text.RegularExpressions;
using Bison.Razor.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

/*
 * Integration tests for the UserIdentity functionality.
 */
public class UserIdentityIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UserIdentityIntegrationTests(WebApplicationFactory<Program> factory)
    {
        Environment.SetEnvironmentVariable(
            "BISONDBPATH",
            Path.Combine(Path.GetTempPath(), $"bison-identity-{Guid.NewGuid():N}.db"));

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
        });
    }

    [Fact]
    public async Task RegisterEndpoint_CreatesUser()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var registerPage = await client.GetAsync("/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.OK, registerPage.StatusCode);

        var html = await registerPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(html);

        var form = new Dictionary<string, string>
        {
            ["Input.Email"] = "eve@example.com",
            ["Input.Password"] = "Password123!",
            ["Input.ConfirmPassword"] = "Password123!",
            ["__RequestVerificationToken"] = token
        };

        var response = await client.PostAsync(
            "/Identity/Account/Register",
            new FormUrlEncodedContent(form));

        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.OK,
            $"Unexpected status: {response.StatusCode}");

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BisonUser>>();

        var created = await userManager.FindByEmailAsync("eve@example.com");
        Assert.NotNull(created);
    }

    [Fact]
    public async Task SignInManager_ValidCredentials_Succeeds()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BisonUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<BisonUser>>();

        var user = new BisonUser
        {
            UserName = "frank",
            Email = "frank@example.com",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "Password123!");

        Assert.True(createResult.Succeeded);

        var result = await signInManager.PasswordSignInAsync(
            "frank",
            "Password123!",
            isPersistent: false,
            lockoutOnFailure: false);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task SignInManager_InvalidCredentials_Fails()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BisonUser>>();
        var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<BisonUser>>();

        var user = new BisonUser
        {
            UserName = "gina",
            Email = "gina@example.com",
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, "Password123!");
        Assert.True(createResult.Succeeded);

        var result = await signInManager.PasswordSignInAsync(
            "gina",
            "WrongPassword!",
            isPersistent: false,
            lockoutOnFailure: false);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task ProtectedRoute_WithoutAuthentication_RedirectsToLogin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Identity/Account/Manage");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.ToString());
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            @"name=""__RequestVerificationToken""\s+type=""hidden""\s+value=""([^""]+)""");

        Assert.True(match.Success, "Could not find __RequestVerificationToken in register page.");

        return match.Groups[1].Value;
    }
}