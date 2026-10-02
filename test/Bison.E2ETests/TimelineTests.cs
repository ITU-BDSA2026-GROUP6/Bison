using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace Bison.E2ETests;

public class TimelineTests : PageTest, IClassFixture<AppFixture>
{
    private readonly string BaseUrl;

    public TimelineTests(AppFixture app)
    {
        BaseUrl = app.BaseUrl;
    }

    [Fact]
    public async Task Root_RedirectsToPublicTimeline()
    {
        await Page.GotoAsync(BaseUrl);

        await Expect(Page).ToHaveURLAsync(new Regex("/obs$"));
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task PublicTimeline_ShowsFullPageOfObservations()
    {
        await Page.GotoAsync($"{BaseUrl}/obs");

        await Expect(Page.Locator("#messagelist > li")).ToHaveCountAsync(32);
    }

    [Fact]
    public async Task PublicTimeline_NextAndPreviousNavigateBetweenPages()
    {
        await Page.GotoAsync($"{BaseUrl}/obs");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Next" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"page=2$"));

        await Page.GetByRole(AriaRole.Link, new() { Name = "Previous" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(new Regex(@"page=1$"));
    }

    [Fact]
    public async Task ClickingAuthor_ShowsOnlyThatAuthorsObservations()
    {
        await Page.GotoAsync($"{BaseUrl}/obs");
        var authorLink = Page.Locator("#messagelist li strong a").First;
        var author = await authorLink.InnerTextAsync();

        await authorLink.ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = $"{author}'s Observations" })).ToBeVisibleAsync();
        var authors = await Page.Locator("#messagelist li strong a").AllInnerTextsAsync();
        Assert.All(authors, a => Assert.Equal(author, a));
    }
}