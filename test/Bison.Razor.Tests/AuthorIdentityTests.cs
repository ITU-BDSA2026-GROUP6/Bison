using Bison.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

public class AuthorIdentityTests
{
    [Fact]
    public async Task Rename_UpdatesAllPostDisplays_WithoutCreatingAnotherAuthor()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BisonDbContext>(o => o.UseSqlite(connection));
        services.AddIdentityCore<Author>().AddEntityFrameworkStores<BisonDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BisonDbContext>();
        context.Database.EnsureCreated();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();
        var author = new Author { UserName = "Original", Email = "author@example.com" };
        Assert.True((await manager.CreateAsync(author, "Password123!")).Succeeded);
        context.Taxons.Add(new Taxon { TaxonId = 7, VernacularName = "Heron" });
        context.SaveChanges();
        var repository = new PostRepository(context);
        repository.AddObservation(author.Id, "Observation", 7);
        var observation = context.Observations.Single();
        repository.AddComment(author.Id, observation.PostId, "Comment");
        repository.AddProposal(author.Id, observation.PostId, 7, "Proposal");

        Assert.True((await manager.SetUserNameAsync(author, "Renamed")).Succeeded);
        context.ChangeTracker.Clear();
        var service = new ObservationService(repository);
        Assert.Equal("Renamed", service.GetObservationById(observation.PostId)!.Author);
        Assert.Equal("Renamed", Assert.Single(service.GetCommentsForObservation(observation.PostId)).Author);
        Assert.Equal("Renamed", Assert.Single(service.GetProposalsForObservation(observation.PostId)).Author);
        Assert.Single(context.Authors);
        Assert.Equal(author.Id, context.Authors.Single().Id);
        Assert.True(await manager.CheckPasswordAsync(context.Authors.Single(), "Password123!"));
        Assert.Empty(repository.GetObservationsByAuthor("Original", 10, 1));
        Assert.Single(repository.GetObservationsByAuthor("Renamed", 10, 1));

        var duplicate = await manager.CreateAsync(new Author { UserName = "renamed" });
        Assert.False(duplicate.Succeeded);
        Assert.Contains(duplicate.Errors, e => e.Code == "DuplicateUserName");
    }

    [Fact]
    public async Task Migration_PreservesAccountsAndPosts_WithLinkedAndUnlinkedAuthors()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<BisonDbContext>().UseSqlite(connection).Options;
        using var context = new BisonDbContext(options);
        var migrator = context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261118143131_LinkAuthorsToIdentity");
        await context.Database.ExecuteSqlRawAsync("""
            INSERT INTO AspNetUsers
                (Id, UserName, NormalizedUserName, Email, NormalizedEmail, PasswordHash,
                 SecurityStamp, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled,
                 LockoutEnabled, AccessFailedCount)
            VALUES ('account-1', 'CurrentName', 'CURRENTNAME', 'account@example.com',
                    'ACCOUNT@EXAMPLE.COM', 'existing-hash', 'existing-stamp', 1, 0, 0, 1, 0);
            INSERT INTO Authors (AuthorId, Name, Email, BisonUserId) VALUES
                (1, 'OldName', 'account@example.com', 'account-1'),
                (2, 'LegacyName', 'legacy@example.com', NULL),
                (3, 'CurrentName', 'account@example.com', NULL);
            INSERT INTO Posts (PostId, AuthorId, Text, TimeStamp) VALUES
                (1, 1, 'Observation', '2026-11-25'),
                (2, 2, 'Comment', '2026-11-25'),
                (3, 3, 'Proposal', '2026-11-25');
            INSERT INTO Taxons (TaxonId, dwc_TaxonID, VernacularName) VALUES (1, 'test', 'Heron');
            INSERT INTO Observations (PostId, TaxonId) VALUES (1, 1);
            INSERT INTO Comments (PostId, ObservationId) VALUES (2, 1);
            INSERT INTO Proposals (PostId, ObservationId, TaxonId) VALUES (3, 1, 1);
            INSERT INTO AspNetUserLogins (LoginProvider, ProviderKey, UserId)
                VALUES ('GitHub', 'external-id', 'account-1');
            """);

        await migrator.MigrateAsync();

        var account = await context.Users.SingleAsync(a => a.Id == "account-1");
        Assert.Equal("CurrentName", account.UserName);
        Assert.Equal("existing-hash", account.PasswordHash);
        Assert.Equal("existing-stamp", account.SecurityStamp);
        Assert.Equal("account-1", context.Observations.Single().AuthorId);
        Assert.Equal("legacy-author-2", context.Comments.Single().AuthorId);
        Assert.Equal("legacy-author-3", context.Proposals.Single().AuthorId);
        Assert.Equal("account-1", context.UserLogins.Single().UserId);
        Assert.Equal(3, context.Users.Count());
        var legacy = context.Users.Single(a => a.Id == "legacy-author-2");
        Assert.Equal("LegacyName", legacy.UserName);
        Assert.Null(legacy.PasswordHash);
        Assert.False(legacy.EmailConfirmed);
        Assert.Null(legacy.NormalizedEmail);
        Assert.StartsWith("legacy-", context.Users.Single(a => a.Id == "legacy-author-3").UserName);
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_key_check";
        using var reader = command.ExecuteReader();
        Assert.False(reader.Read());
    }
}
