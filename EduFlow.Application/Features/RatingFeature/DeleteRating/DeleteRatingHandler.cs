namespace EduFlow.Application.Features.RatingFeature.DeleteRating;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record DeleteRatingRequest(Guid CourseId);

public sealed class DeleteRatingHandler(
    IRepository<Rating> ratingRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<DeleteRatingRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteRatingRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return RatingErrors.NotFound;
        }

        var rating = await ratingRepository.FindAsync(
            r => r.CourseId == command.CourseId && r.StudentId == studentId, cancellationToken);

        if (rating is null)
        {
            return RatingErrors.NotFound;
        }

        await ratingRepository.DeleteAsync(rating, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
