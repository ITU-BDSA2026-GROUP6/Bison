using System.Security.Claims;
using Bison.Razor.Models;
using Bison.Razor.Pages.Post;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bison.Razor.Tests;

public class PostObservationUnitTests
{
    [Fact]
    public void OnPost_SavesObservationWithSelectedTaxonAndRedirectsToIt()
    {
        using var context = CreateContext();
        context.Taxons.Add(new Taxon { TaxonId = 7, VernacularName = "Heron" });
        context.SaveChanges();

        var model = CreateModel(context, "Adrian");
        model.Message = "Saw a heron";
        model.SelectedTaxonId = 7;

        var result = model.OnPost();

        var redirect = Assert.IsType<RedirectResult>(result);
        var savedObservation = Assert.Single(context.Observations);
        Assert.Equal($"/ob/{savedObservation.PostId}", redirect.Url);
        Assert.Equal("Saw a heron", savedObservation.Text);
        Assert.Equal(7, savedObservation.TaxonId);
        Assert.Equal("Adrian", context.Authors.Find(savedObservation.AuthorId)?.Name);
    }

    [Fact]
    public void OnPost_DoesNotSaveObservation_WhenSelectedTaxonDoesNotExist()
    {
        using var context = CreateContext();
        var model = CreateModel(context, "Adrian");
        model.Message = "Saw a heron";
        model.SelectedTaxonId = 999;

        var result = model.OnPost();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.Empty(context.Observations);
    }

    private static BisonDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BisonDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BisonDbContext(options);
    }

    private static ObserveModel CreateModel(BisonDbContext context, string author)
    {
        var service = new ObservationService(new DBFacade(new PostRepository(context)));
        var model = new ObserveModel(service, context);
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Name, author)],
                authenticationType: "test"))
        };
        model.PageContext = new PageContext { HttpContext = httpContext };
        return model;
    }
}
