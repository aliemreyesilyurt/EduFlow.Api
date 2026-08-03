namespace EduFlow.Application.Features.CourseFeature.GetAllCourses;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetAllCoursesRequest;

public sealed record CourseSummary(
    Guid Id,
    string Title,
    string? Description,
    Guid InstructorId,
    CourseStatus Status,
    DateTime? PublishedOn,
    double? AverageRating,
    int RatingCount,
    int CommentCount);

public sealed record GetAllCoursesResponse(IReadOnlyList<CourseSummary> Courses);

public sealed class GetAllCoursesHandler(
    IRepository<Course> courseRepository,
    IRepository<Rating> ratingRepository,
    IRepository<Comment> commentRepository,
    ITenantContext tenantContext) : IHandler<GetAllCoursesRequest, Result<GetAllCoursesResponse>>
{
    public async Task<Result<GetAllCoursesResponse>> HandleAsync(GetAllCoursesRequest command, CancellationToken cancellationToken)
    {
        var courses = await courseRepository.GetAllAsync(cancellationToken);

        var ratingsByCourse = (await ratingRepository.GetAllAsync(cancellationToken))
            .GroupBy(r => r.CourseId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var commentCountsByCourse = (await commentRepository.GetAllAsync(cancellationToken))
            .Where(c => !c.IsHidden)
            .GroupBy(c => c.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

        var visible = courses
            .Where(c => CourseAccess.CanView(c, tenantContext))
            .OrderByDescending(c => c.CreatedOn)
            .Select(c =>
            {
                var ratings = ratingsByCourse.GetValueOrDefault(c.Id);
                var averageRating = ratings is null or []
                    ? (double?)null
                    : Math.Round(ratings.Average(r => r.Value), 2);

                return new CourseSummary(
                    c.Id, c.Title, c.Description, c.InstructorId, c.Status, c.PublishedOn,
                    averageRating, ratings?.Count ?? 0, commentCountsByCourse.GetValueOrDefault(c.Id));
            })
            .ToList();

        return Result.Success(new GetAllCoursesResponse(visible));
    }
}
