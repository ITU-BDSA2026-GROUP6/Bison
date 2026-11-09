public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UserManager_CanCreateUser()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BisonUser>>();

        var user = new BisonUser
        {
            UserName = "eve",
            Email = "eve@example.com"
        };

        var result = await userManager.CreateAsync(user, "Password123!");
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AnynomousUser_IsRedirectedToLoginPage()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // Prevent automatic redirection to the login page
        });

        var response = await client.GetAsync("/Secret");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Identity/Account/Login", response.Headers.Location?.ToString());
    }
}