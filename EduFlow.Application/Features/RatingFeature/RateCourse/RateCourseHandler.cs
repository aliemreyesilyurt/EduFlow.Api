namespace EduFlow.Application.Features.RatingFeature.RateCourse;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.EnrollmentFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record RateCourseRequest(Guid CourseId, int Value);

public sealed class RateCourseHandler(
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Rating> ratingRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<RateCourseRequest, Result<RatingResponse>>
{
    public async Task<Result<RatingResponse>> HandleAsync(RateCourseRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        var enrollment = await enrollmentRepository.FindAsync(
            e => e.CourseId == command.CourseId && e.StudentId == studentId, cancellationToken);

        if (enrollment is null)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var rating = await ratingRepository.FindAsync(
            r => r.CourseId == command.CourseId && r.StudentId == studentId, cancellationToken);

        if (rating is null)
        {
            rating = new Rating
            {
                Id = Guid.CreateVersion7(),
                CourseId = command.CourseId,
                StudentId = studentId,
                Value = command.Value
            };

            await ratingRepository.AddAsync(rating, cancellationToken);
        }
        else
        {
            rating.Value = command.Value;
            await ratingRepository.UpdateAsync(rating, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new RatingResponse(
            rating.Id, rating.CourseId, rating.StudentId, rating.Value, rating.CreatedOn, rating.UpdatedOn));
    }
}
