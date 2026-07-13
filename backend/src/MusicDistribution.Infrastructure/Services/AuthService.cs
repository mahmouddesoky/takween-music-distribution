using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MusicDistribution.Application.Common;
using MusicDistribution.Application.DTOs;
using MusicDistribution.Application.Exceptions;
using MusicDistribution.Application.Interfaces;

namespace MusicDistribution.Infrastructure.Services;

public class AuthService(
    IConfiguration configuration,
    IValidator<LoginRequest> validator) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(validator, request, cancellationToken);

        var configuredUsername = configuration["Auth:Username"];
        var configuredPassword = configuration["Auth:Password"];
        var configuredRole = configuration["Auth:Role"] ?? "Admin";

        if (configuredUsername is null || configuredPassword is null)
        {
            throw new InvalidOperationException("Auth credentials are not configured.");
        }

        // Constant-time comparison to avoid timing-based username/password oracle attacks.
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(request.Username),
                Encoding.UTF8.GetBytes(configuredUsername))
            || !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(request.Password),
                Encoding.UTF8.GetBytes(configuredPassword)))
        {
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        var jwtSection = configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, configuredRole)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
