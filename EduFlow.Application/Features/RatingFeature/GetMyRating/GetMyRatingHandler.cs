namespace EduFlow.Application.Features.RatingFeature.GetMyRating;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetMyRatingRequest(Guid CourseId);

public sealed record GetMyRatingResponse(Guid? RatingId, int? Value);

public sealed class GetMyRatingHandler(
    IRepository<Rating> ratingRepository,
    ITenantContext tenantContext) : IHandler<GetMyRatingRequest, Result<GetMyRatingResponse>>
{
    public async Task<Result<GetMyRatingResponse>> HandleAsync(GetMyRatingRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return Result.Success(new GetMyRatingResponse(null, null));
        }

        var rating = await ratingRepository.FindAsync(
            r => r.CourseId == command.CourseId && r.StudentId == studentId, cancellationToken);

        return Result.Success(new GetMyRatingResponse(rating?.Id, rating?.Value));
    }
}
