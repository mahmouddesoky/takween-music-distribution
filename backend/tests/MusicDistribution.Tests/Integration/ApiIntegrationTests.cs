using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Infrastructure.Persistence;

namespace MusicDistribution.Tests.Integration;

/// <summary>
/// End-to-end tests that boot the full API in-process via WebApplicationFactory
/// and exercise the HTTP pipeline (routing, auth, validation, business rules).
/// Uses a shared in-memory SQLite database kept alive for the whole test class
/// so migrations + seeding run exactly once and all tests see the same data.
/// </summary>
public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    // Keep a SQLite connection open so the shared in-memory DB persists across requests.
    private readonly SqliteConnection _connection;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove every registration that ties AppDbContext to the file-based SQLite provider.
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("MusicDistribution.Infrastructure.Persistence.AppDbContext") == true
                             || d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                             || d.ServiceType == typeof(DbContextOptions))
                    .ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Re-register against the shared in-memory SQLite connection.
                services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(_connection));
            });
        });
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task<string> GetTokenAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin", "admin123"));
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return body!.Token;
    }

    [Fact]
    public async Task Get_tracks_returns_seeded_data_without_auth()
    {
        var client = CreateClient();
        var tracks = await client.GetFromJsonAsync<List<TrackSummaryResponse>>("/api/tracks");

        Assert.NotNull(tracks);
        Assert.NotEmpty(tracks);
    }

    [Fact]
    public async Task Get_tracks_filters_by_status()
    {
        var client = CreateClient();
        var drafts = await client.GetFromJsonAsync<List<TrackSummaryResponse>>("/api/tracks?status=Draft");

        Assert.NotNull(drafts);
        Assert.All(drafts!, t => Assert.Equal("Draft", t.Status));
    }

    [Fact]
    public async Task Post_track_without_token_is_unauthorized()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/tracks", new
        {
            title = "Test",
            artistId = 1,
            isrc = "USRC7700001",
            releaseDate = "2025-01-01",
            genre = "Pop"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_track_with_token_creates_and_returns_detail()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetTokenAsync());

        var response = await client.PostAsJsonAsync("/api/tracks", new
        {
            title = "Integration Track",
            artistId = 1,
            isrc = "USRC7700002",
            releaseDate = "2025-08-01",
            genre = "Jazz"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TrackDetailResponse>();
        Assert.Equal("Integration Track", created!.Title);
        Assert.Equal("Draft", created.Status);
        Assert.Equal("Amr Diab", created.ArtistName);
    }

    [Fact]
    public async Task Post_track_with_duplicate_isrc_returns_conflict()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await GetTokenAsync());

        var body = new
        {
            title = "First",
            artistId = 1,
            isrc = "USRC7700003",
            releaseDate = "2025-01-01",
            genre = "Pop"
        };
        var first = await client.PostAsJsonAsync("/api/tracks", body);
        first.EnsureSuccessStatusCode();

        var duplicate = await client.PostAsJsonAsync("/api/tracks", body);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
    }

    [Fact]
    public async Task Get_track_detail_includes_distributions()
    {
        var client = CreateClient();
        // Track 3 in seed data has 3 distributions across Spotify/Apple/YouTube.
        var detail = await client.GetFromJsonAsync<TrackDetailResponse>("/api/tracks/3");

        Assert.NotNull(detail);
        Assert.Equal("Odam Merayetha", detail!.Title);
        Assert.Equal(3, detail.Distributions.Count);
    }
}
