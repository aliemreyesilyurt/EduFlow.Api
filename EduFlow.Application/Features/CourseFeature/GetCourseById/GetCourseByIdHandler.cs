namespace EduFlow.Application.Features.CourseFeature.GetCourseById;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetCourseByIdRequest(Guid Id);

public sealed record GetCourseByIdResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid InstructorId,
    string InstructorName,
    CourseStatus Status,
    DateTime? PublishedOn,
    double? AverageRating,
    int RatingCount,
    int CommentCount,
    Guid? ExamId);

public sealed class GetCourseByIdHandler(
    IRepository<Course> courseRepository,
    IRepository<Rating> ratingRepository,
    IRepository<Comment> commentRepository,
    IRepository<Exam> examRepository,
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<GetCourseByIdRequest, Result<GetCourseByIdResponse>>
{
    public async Task<Result<GetCourseByIdResponse>> HandleAsync(GetCourseByIdRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.Id, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.Id);
        }

        var ratings = (await ratingRepository.GetAllAsync(cancellationToken))
            .Where(r => r.CourseId == course.Id)
            .ToList();

        var commentCount = (await commentRepository.GetAllAsync(cancellationToken))
            .Count(c => c.CourseId == course.Id && !c.IsHidden);

        var averageRating = ratings.Count == 0 ? null : (double?)Math.Round(ratings.Average(r => r.Value), 2);

        var instructorNames = await identityService.GetDisplayNamesAsync([course.InstructorId], cancellationToken);

        var exam = await examRepository.FindAsync(e => e.CourseId == course.Id && e.IsPublished, cancellationToken);

        return Result.Success(new GetCourseByIdResponse(
            course.Id, course.Title, course.Description, course.InstructorId,
            instructorNames.GetValueOrDefault(course.InstructorId, "Unknown"),
            course.Status, course.PublishedOn,
            averageRating, ratings.Count, commentCount, exam?.Id));
    }
}
