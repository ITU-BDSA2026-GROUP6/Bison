namespace Bison.Razor.Tests;

public class ObservationServiceTests
{
    private readonly ObservationService _service;

    public ObservationServiceTests()
    {
        var db = new DBFacade(TestHelpers.GetSeededDbPath());
        _service = new ObservationService(db);
    }

    [Fact]
    public void GetObservationById_ReturnsNull_WhenObservationDoesNotExist()
    {
        var result = _service.GetObservationById(-1);

        Assert.Null(result);
    }

    [Fact]
    public void GetObservations_ReturnsNonEmptyList_ForFirstPage()
    {
        var result = _service.GetObservations(page: 1);

        Assert.NotEmpty(result);
    }

    [Fact]
    public void GetObservationsFromAuthor_ReturnsOnlyThatAuthorsObservations()
    {
        var result = _service.GetObservationsFromAuthor("Adrian", page: 1);

        Assert.NotEmpty(result);
        Assert.All(result, obs => Assert.Equal("Adrian", obs.Author));
    }

    [Fact]
    public void GetCommentsForObservation_ReturnsEmptyList_WhenObservationDoesNotExist()
    {
        var result = _service.GetCommentsForObservation(-1);

        Assert.Empty(result);
    }

    [Fact]
        public void GetProposalsForObservation_ReturnsEmptyList_WhenObservationDoesNotExist()
        {
        var result = _service.GetProposalsForObservation(-1);

        Assert.Empty(result);
        }
}