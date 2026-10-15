using MusicDistribution.Api.ExceptionHandling;
using MusicDistribution.Api.Extensions;
using MusicDistribution.Application;
using MusicDistribution.Infrastructure;
using MusicDistribution.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication();

builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Local/demo convenience: apply pending migrations and seed sample data on startup.
// In Production, migrations are applied by the deployment pipeline (dotnet ef database update / migration bundle).
if (!app.Environment.IsProduction())
{
    await app.Services.InitialiseDatabaseAsync();
}

app.UseExceptionHandler();
// Empty 401/403/404 responses also get a ProblemDetails body.
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Exposed as public so Microsoft.AspNetCore.Mvc.Testing's WebApplicationFactory<Program>
// can reference the entry point from the test assembly.
public partial class Program { }
