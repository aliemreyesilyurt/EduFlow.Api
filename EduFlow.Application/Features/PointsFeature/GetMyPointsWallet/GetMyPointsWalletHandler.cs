namespace EduFlow.Application.Features.PointsFeature.GetMyPointsWallet;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetMyPointsWalletRequest;

public sealed class GetMyPointsWalletHandler(
    IRepository<PointsWallet> walletRepository,
    ITenantContext tenantContext) : IHandler<GetMyPointsWalletRequest, Result<PointsWalletResponse>>
{
    public async Task<Result<PointsWalletResponse>> HandleAsync(GetMyPointsWalletRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return Result.Success(new PointsWalletResponse(0));
        }

        var wallet = await walletRepository.FindAsync(w => w.StudentId == studentId, cancellationToken);

        return Result.Success(new PointsWalletResponse(wallet?.Balance ?? 0));
    }
}
