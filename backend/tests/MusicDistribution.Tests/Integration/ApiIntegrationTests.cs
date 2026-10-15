using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MusicDistribution.Application.Abstractions.Authentication;
using MusicDistribution.Application.Auth;
using MusicDistribution.Application.Dsps;
using MusicDistribution.Application.Tracks;
using MusicDistribution.Infrastructure.Authentication;
using MusicDistribution.Infrastructure.Persistence;

namespace MusicDistribution.Tests.Integration;

/// <summary>
/// End-to-end tests that boot the full API in-process via WebApplicationFactory
/// and exercise the HTTP pipeline (routing, auth, validation, business rules, error contract).
/// Each test gets its own in-memory SQLite database, migrated and seeded on startup.
/// </summary>
public sealed class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private const string TestSigningKey = "IntegrationTestSigningKey-0123456789abcdef";

    private readonly WebApplicationFactory<Program> _factory;
    // Keep a SQLite connection open so the in-memory DB lives for the whole test.
    private readonly SqliteConnection _connection;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            // Secrets are not in the base appsettings.json; tests supply their own.
            builder.UseSetting("Jwt:Key", TestSigningKey);
            builder.UseSetting("Auth:Username", "admin");
            builder.UseSetting("Auth:Password", "admin123");
            builder.ConfigureServices(services =>
            {
                // Remove every registration that ties AppDbContext to the file-based SQLite database.
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("MusicDistribution.Infrastructure.Persistence.AppDbContext") == true
                             || d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                             || d.ServiceType == typeof(DbContextOptions))
                    .ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(_connection));
            });
        });
    }

    public void Dispose() => _connection.Dispose();

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task<HttpClient> CreateAdminClientAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "admin123"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    [Fact]
    public async Task Get_tracks_returns_seeded_data_without_auth()
    {
        var tracks = await CreateClient().GetFromJsonAsync<List<TrackSummaryResponse>>("/api/tracks");

        Assert.NotNull(tracks);
        Assert.Equal(10, tracks!.Count);
        Assert.True(tracks.Select(t => t.Genre).Distinct().Count() >= 3);
        Assert.Equal(3, tracks.Select(t => t.Status).Distinct().Count());
    }

    [Fact]
    public async Task Get_tracks_filters_by_status()
    {
        var drafts = await CreateClient().GetFromJsonAsync<List<TrackSummaryResponse>>("/api/tracks?status=Draft");

        Assert.NotNull(drafts);
        Assert.NotEmpty(drafts!);
        Assert.All(drafts, t => Assert.Equal("Draft", t.Status));
    }

    [Fact]
    public async Task Get_tracks_combines_artist_and_genre_filters()
    {
        var tracks = await CreateClient().GetFromJsonAsync<List<TrackSummaryResponse>>("/api/tracks?artistId=2&genre=tarab");

        Assert.NotNull(tracks);
        Assert.Equal(2, tracks!.Count);
        Assert.All(tracks, t => Assert.Equal("Mohamed Mounir", t.ArtistName));
    }

    [Fact]
    public async Task Get_tracks_with_unknown_status_returns_validation_problem()
    {
        var response = await CreateClient().GetAsync("/api/tracks?status=Published");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.True(problem.GetProperty("errors").TryGetProperty("Status", out _));
    }

    [Fact]
    public async Task Get_dsps_returns_the_three_seeded_providers()
    {
        var dsps = await CreateClient().GetFromJsonAsync<List<DspResponse>>("/api/dsps");

        Assert.Equal(["Apple Music", "Spotify", "YouTube"], dsps!.Select(d => d.Name));
    }

    [Fact]
    public async Task Get_track_detail_includes_distributions()
    {
        // Track 3 in seed data has 3 distributions across Spotify/Apple/YouTube.
        var detail = await CreateClient().GetFromJsonAsync<TrackDetailResponse>("/api/tracks/3");

        Assert.NotNull(detail);
        Assert.Equal("Odam Merayetha", detail!.Title);
        Assert.Equal(3, detail.Distributions.Count);
    }

    [Fact]
    public async Task Get_missing_track_returns_not_found_problem()
    {
        var response = await CreateClient().GetAsync("/api/tracks/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_unauthorized()
    {
        var response = await CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "wrong"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await ReadProblemAsync(response);
    }

    [Fact]
    public async Task Post_track_without_token_is_unauthorized()
    {
        var response = await CreateClient().PostAsJsonAsync("/api/tracks", new
        {
            title = "Test",
            artistId = 1,
            isrc = "USRC77000001",
            releaseDate = "2025-01-01",
            genre = "Pop"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        await ReadProblemAsync(response);
    }

    [Fact]
    public async Task Post_track_with_non_admin_token_is_forbidden()
    {
        // A validly signed token whose role is not Admin.
        var generator = new JwtTokenGenerator(
            Options.Create(new JwtSettings { Key = TestSigningKey, Issuer = "MusicDistribution", Audience = "MusicDistribution" }),
            TimeProvider.System);
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", generator.Generate(new AuthenticatedUser("viewer", "User")).Token);

        var response = await client.PostAsJsonAsync("/api/tracks", new
        {
            title = "Test",
            artistId = 1,
            isrc = "USRC77000009",
            releaseDate = "2025-01-01",
            genre = "Pop"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        await ReadProblemAsync(response);
    }

    [Fact]
    public async Task Post_track_with_token_creates_and_returns_detail()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/tracks", new
        {
            title = "Integration Track",
            artistId = 1,
            isrc = "USRC77000002",
            releaseDate = "2025-08-01",
            genre = "Jazz"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<TrackDetailResponse>();
        Assert.Equal("Integration Track", created!.Title);
        Assert.Equal("Draft", created.Status);
        Assert.Equal("Amr Diab", created.ArtistName);
    }

    [Fact]
    public async Task Post_track_with_invalid_body_returns_field_errors()
    {
        var client = await CreateAdminClientAsync();

        var response = await client.PostAsJsonAsync("/api/tracks", new
        {
            title = "",
            artistId = 1,
            isrc = "BAD",
            releaseDate = "2025-08-01",
            genre = "Jazz"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var errors = (await ReadProblemAsync(response)).GetProperty("errors");
        Assert.True(errors.TryGetProperty("Title", out _));
        Assert.True(errors.TryGetProperty("Isrc", out _));
    }

    [Fact]
    public async Task Post_track_with_duplicate_isrc_returns_conflict()
    {
        var client = await CreateAdminClientAsync();
        var body = new
        {
            title = "First",
            artistId = 1,
            isrc = "USRC77000003",
            releaseDate = "2025-01-01",
            genre = "Pop"
        };

        (await client.PostAsJsonAsync("/api/tracks", body)).EnsureSuccessStatusCode();
        var duplicate = await client.PostAsJsonAsync("/api/tracks", body);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        await ReadProblemAsync(duplicate);
    }

    [Fact]
    public async Task Distribute_submits_draft_track_and_rejects_repeat()
    {
        var client = await CreateAdminClientAsync();

        // Track 1 is a seeded Draft with no distributions.
        var response = await client.PostAsJsonAsync("/api/tracks/1/distribute", new { dspIds = new[] { 1, 3 } });
        response.EnsureSuccessStatusCode();
        var detail = await response.Content.ReadFromJsonAsync<TrackDetailResponse>();
        Assert.Equal("Submitted", detail!.Status);
        Assert.Equal(2, detail.Distributions.Count);

        var repeat = await client.PostAsJsonAsync("/api/tracks/1/distribute", new { dspIds = new[] { 1 } });
        Assert.Equal(HttpStatusCode.Conflict, repeat.StatusCode);
    }

    [Fact]
    public async Task Patch_status_applies_domain_rules()
    {
        var client = await CreateAdminClientAsync();

        // Track 1 has never been submitted, so it cannot be Distributed yet.
        var rejected = await client.PatchAsJsonAsync("/api/tracks/1/status", new { status = "Distributed" });
        Assert.Equal(HttpStatusCode.Conflict, rejected.StatusCode);

        // Track 2 is Submitted with pending DSP submissions.
        var accepted = await client.PatchAsJsonAsync("/api/tracks/2/status", new { status = "distributed" });
        accepted.EnsureSuccessStatusCode();
        var detail = await accepted.Content.ReadFromJsonAsync<TrackDetailResponse>();
        Assert.Equal("Distributed", detail!.Status);

        var backwards = await client.PatchAsJsonAsync("/api/tracks/2/status", new { status = "Submitted" });
        Assert.Equal(HttpStatusCode.Conflict, backwards.StatusCode);
    }
}
