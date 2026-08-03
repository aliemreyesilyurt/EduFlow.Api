namespace EduFlow.Application.Features.AuthFeature.AcceptInvitation;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record AcceptInvitationRequest(Guid UserId, string Token, string Password);

public sealed record AcceptInvitationResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresOn);

public sealed class AcceptInvitationHandler(IIdentityService identityService)
    : IHandler<AcceptInvitationRequest, Result<AcceptInvitationResponse>>
{
    public async Task<Result<AcceptInvitationResponse>> HandleAsync(AcceptInvitationRequest command, CancellationToken cancellationToken)
    {
        var result = await identityService.AcceptInvitationAsync(command.UserId, command.Token, command.Password, cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<AcceptInvitationResponse>(result.Error);
        }

        var tokens = result.Value;

        return Result.Success(new AcceptInvitationResponse(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresOn));
    }
}
