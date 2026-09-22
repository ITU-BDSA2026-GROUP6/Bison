namespace Bison.EndToEnd.Tests;

using System.Diagnostics;
using System.Net.Http.Json;
using SimpleDB;

public class FuzzEndToEndTest
{
    private static readonly string PathToWebServiceCsproj = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../../src/CSVDatabase.WebService/CSVDatabase.WebService.csproj"));

    private const int NumberOfPosts = 100;
    private const double ValidIdRate = 0.9; // most generated ObsIDs/TaxonIDs should be valid


    //fuzzer
    [Fact]
    public async Task fuzzed_observations_comments_and_proposals_are_returned_correct()
    {
        var random = new Random(1111); 
        var taxonIds = LoadAllTaxonIds();

    
        string csvDirectory = Path.Combine(
            Path.GetDirectoryName(PathToWebServiceCsproj)!, "bin/Debug/net8.0/CSVFiles");
        if (Directory.Exists(csvDirectory))
        {
            Directory.Delete(csvDirectory, recursive: true);
        }

        //start web service
        using var server = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{PathToWebServiceCsproj}\" --urls http://localhost:5205",
            UseShellExecute = false,
        })!;

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri("http://localhost:5205") };
            await WaitForServer(client);

            //Test Oracle
            var expectedObservations = new List<Observation>();
            var expectedComments = new List<Comment>();
            var expectedProposals = new List<Proposal>();
            var knownObsIds = new List<long>();
            long nextObsId = 1;

            for (int i = 0; i < NumberOfPosts; i++)
            {
                //check if we should post an observation, comment, or proposal
                switch (random.Next(3))
                {
                    case 0:
                        {
                            var observation = new Observation(
                                nextObsId,
                                RandomWord(random, "author"),
                                RandomWord(random, "text"),
                                RandomWord(random, "location"),
                                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                            var response = await client.PostAsJsonAsync("/observation", observation);
                            response.EnsureSuccessStatusCode();

                            expectedObservations.Add(observation);
                            knownObsIds.Add(nextObsId);
                            nextObsId++;
                            break;
                        }
                    case 1:
                        {
                            var comment = new Comment(
                                PickObsId(random, knownObsIds),
                                RandomWord(random, "author"),
                                RandomWord(random, "text"),
                                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                            var response = await client.PostAsJsonAsync("/comment", comment);
                            response.EnsureSuccessStatusCode();

                            expectedComments.Add(comment);
                            break;
                        }
                    case 2:
                        {
                            var proposal = new Proposal(
                                PickObsId(random, knownObsIds),
                                RandomWord(random, "author"),
                                PickTaxonId(random, taxonIds),
                                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                            var response = await client.PostAsJsonAsync("/proposal", proposal);
                            response.EnsureSuccessStatusCode();

                            expectedProposals.Add(proposal);
                            break;
                        }
                }
            }

            // observations must contain exactly what we posted.
            var actualObservations = await client.GetFromJsonAsync<List<Observation>>("/observations")
                ?? new List<Observation>();

            AssertSameRecords(expectedObservations, actualObservations, o => (o.ObsID, o.Author, o.Text));

            foreach (long obsId in knownObsIds.Distinct())
            {
                // assert that result is as expected for comments and proposals for this observation
                var expectedForObs = expectedComments.Where(c => c.ObsID == obsId).ToList();
                var actualForObs = await client.GetFromJsonAsync<List<Comment>>($"/comments?id={obsId}")
                    ?? new List<Comment>();
                AssertSameRecords(expectedForObs, actualForObs, c => (c.ObsID, c.Author, c.Text));

                var expectedProposalsForObs = expectedProposals.Where(p => p.ObsID == obsId).ToList();
                var actualProposalsForObs = await client.GetFromJsonAsync<List<Proposal>>($"/proposals?id={obsId}")
                    ?? new List<Proposal>();
                AssertSameRecords(expectedProposalsForObs, actualProposalsForObs, p => (p.ObsID, p.Author, p.TaxonID));
            }
        }
        finally
        {
            // Stop webservice
            server.Kill(entireProcessTree: true);
        }
    }

    private static long PickObsId(Random random, List<long> knownObsIds)
    {
        if (knownObsIds.Count > 0 && random.NextDouble() < ValidIdRate)
        {
            return knownObsIds[random.Next(knownObsIds.Count)];
        }

        return random.NextInt64(100_000, 1_000_000); //  maybe invalid ObsID
    }

    private static string PickTaxonId(Random random, IReadOnlyList<string> taxonIds)
    {
        if (taxonIds.Count > 0 && random.NextDouble() < ValidIdRate)
        {
            return taxonIds[random.Next(taxonIds.Count)];
        }

        return $"invalid-{random.Next(100_000)}";
    }

    private static string RandomWord(Random random, string prefix) =>
        $"{prefix}-{random.Next(100_000)}";

    private static IReadOnlyList<string> LoadAllTaxonIds()
    {

        using var stream = typeof(Taxonomy).Assembly
            .GetManifestResourceStream("SimpleDB.Taxonomy.csv")!;
        using StreamReader reader = new(stream);
        using CsvHelper.CsvReader csv = new(reader, System.Globalization.CultureInfo.InvariantCulture);
        return csv.GetRecords<Taxon>().Select(t => t.TaxonID).ToList();
    }

    private static void AssertSameRecords<T, TKey>(
        List<T> expected, List<T> actual, Func<T, TKey> key)
    {
        var expectedKeys = expected.Select(key).OrderBy(k => k).ToList();
        var actualKeys = actual.Select(key).OrderBy(k => k).ToList();
        Assert.Equal(expectedKeys, actualKeys);
    }

    private static async Task WaitForServer(HttpClient client)
    {
        for (int i = 0; i < 60; i++)
        {
            try
            {
                var response = await client.GetAsync("/observations");
                if (response.IsSuccessStatusCode) return;
            }
            catch (HttpRequestException)
            {
            }
            await Task.Delay(500);
        }
        throw new TimeoutException("Web service did not start");
    }
}
