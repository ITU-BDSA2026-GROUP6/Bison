using Bison.Razor.Pages.Post;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Tests;

public class ObserveUnitTests
{
    [Fact]
    public void OnPost_ChallengesAnonymousUser()
    {
        var model = new ObserveModel(null!)
        {
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };

        Assert.IsType<ChallengeResult>(model.OnPost());
    }
}
