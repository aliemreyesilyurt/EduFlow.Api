namespace EduFlow.Application.Features.AuthFeature.ConfirmEmail;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;

public sealed record ConfirmEmailRequest(Guid UserId, string Token);

public sealed class ConfirmEmailHandler(IIdentityService identityService) : IHandler<ConfirmEmailRequest, Result>
{
    public Task<Result> HandleAsync(ConfirmEmailRequest command, CancellationToken cancellationToken) =>
        identityService.ConfirmEmailAsync(command.UserId, command.Token, cancellationToken);
}
