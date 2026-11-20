using System.Security.Claims;
using Bison.Razor.Models;
using Bison.Razor.Pages.Post;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Tests;

public class ObserveUnitTests
{
    [Fact]
    public void OnPost_RedirectsAnonymousUserToLogin()
    {
        var model = CreateModel(null);

        var result = model.OnPost();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Identity/Account/Login", redirect.PageName);
    }

    private static ObserveModel CreateModel(string? author)
    {
        var model = new ObserveModel(null!);
        var identity = author is null
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Name, author)], "test");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        model.PageContext = new PageContext { HttpContext = httpContext };
        return model;
    }
}
