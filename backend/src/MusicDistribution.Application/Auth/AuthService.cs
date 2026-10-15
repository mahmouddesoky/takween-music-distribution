using FluentValidation;
using MusicDistribution.Application.Abstractions.Authentication;
using MusicDistribution.Application.Common.Exceptions;
using MusicDistribution.Application.Common.Validation;

namespace MusicDistribution.Application.Auth;

/// <summary>
/// Login use case: validates input, checks credentials through <see cref="IIdentityService"/>
/// and issues a JWT through <see cref="IJwtTokenGenerator"/>. How users are stored and how
/// tokens are signed are infrastructure details.
/// </summary>
public class AuthService(
    IIdentityService identityService,
    IJwtTokenGenerator tokenGenerator,
    IValidator<LoginRequest> validator) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await ValidationHelper.ValidateAndThrowAsync(validator, request, cancellationToken);

        var user = await identityService.AuthenticateAsync(request.Username, request.Password, cancellationToken)
            ?? throw new UnauthorizedException("Invalid username or password.");

        var token = tokenGenerator.Generate(user);
        return new LoginResponse(token.Token, token.ExpiresAt);
    }
}
