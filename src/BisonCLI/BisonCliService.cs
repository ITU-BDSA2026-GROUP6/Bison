using SimpleDB;
using System.Net.Http.Json;

namespace Bison.CLI;


/*
    * BisonCLIService.cs
    * 
    * BisonCliService handles the business logic used by the Bison command-line interface.
    * 
    * The service communicates with the CSV database web service through HttpClient.
*/
public class BisonCliService
{
    private readonly HttpClient _client;

    public BisonCliService(HttpClient client)
    {
        _client = client;
    }

    private void DisplayStoredObservations()
    {
        try
        {
            var observations = _client.GetFromJsonAsync<List<Observation>>("/observations").Result!;
            UserInterface.DisplayObservations(observations);
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
        }
    }

    public void Observe(string message, string location)
    {
        Observation observation = new Observation(
            GetNextObservationID(),
            Environment.UserName,
            message,
            location,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        try
        {
            _client.PostAsJsonAsync("/observation", observation).Result.EnsureSuccessStatusCode();
            UserInterface.DisplaySuccess();
        }
        catch (Exception e)
        {
            UserInterface.DisplayWriteError(e);
        }

        DisplayStoredObservations();
    }

    private long GetNextObservationID()
    {
        try
        {
            var observations = _client.GetFromJsonAsync<List<Observation>>("/observations").Result!;
            //The field above does three things. Sends the HTTP GET to http://localhost:5204/observations
            //Receives the JSON the web service returns.
            //Converts that JSON into a List<Observation>
            return observations.Count + 1;
        }
        catch
        {
            return 1;
        }
    }

    public void Read()
    {
        try
        {
            var observations = _client.GetFromJsonAsync<List<Observation>>("/observations").Result!;

            UserInterface.DisplayObservations(observations);
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
        }
    }

    public bool Comment(string message, long observationId)
    {
        try
        {
            var observations =
                _client.GetFromJsonAsync<List<Observation>>("/observations").Result!;

            if (!observations.Any(observation => observation.ObsID == observationId))
            {
                Console.WriteLine(
                    $"Observation with ID {observationId} does not exist.");

                return false;
            }

            Comment comment = new(
                observationId,
                Environment.UserName,
                message,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _client
                .PostAsJsonAsync("/comment", comment)
                .Result
                .EnsureSuccessStatusCode();

            UserInterface.DisplaySuccess();
            return true;
        }
        catch (Exception exception)
        {
            UserInterface.DisplayReadError(exception);
            return false;
        }
    }

    public bool Propose(string taxonID, long obsID)
    {
        try
        {
            var observations = _client.GetFromJsonAsync<List<Observation>>("/observations").Result!;
            bool observationExists = observations.Any(o => o.ObsID == obsID);

            if (!observationExists)
            {
                System.Console.WriteLine($"Observation with ID {obsID} does not exist.");
                return false;
            }

            var taxon = Taxonomy.LoadEmbedded().GetById(taxonID);
            if (taxon == null)
            {
                System.Console.WriteLine($"Taxon with ID {taxonID} does not exist.");
                return false;
            }

            Proposal proposal = new Proposal(
                obsID,
                Environment.UserName,
                taxonID,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _client.PostAsJsonAsync("/proposal", proposal).Result.EnsureSuccessStatusCode();
            UserInterface.DisplaySuccess();
            return true;
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
            return false;
        }
    }

    public void Discussion(long obsID)
    {
        try
        {
            var observation = _client.GetFromJsonAsync<List<Observation>>("/observations").Result!
            .FirstOrDefault(o => o.ObsID == obsID);
            if (observation == null)
            {
                System.Console.WriteLine($"Observation with ID {obsID} does not exist.");
                return;
            }
            var comments = _client.GetFromJsonAsync<List<Comment>>($"/comments?id={obsID}").Result!;
            UserInterface.DisplayDiscussion(observation, comments);
        }
        catch (Exception e)
        {
            UserInterface.DisplayReadError(e);
            return;
        }
    }

}