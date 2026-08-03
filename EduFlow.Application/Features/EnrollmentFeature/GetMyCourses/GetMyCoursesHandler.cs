namespace EduFlow.Application.Features.EnrollmentFeature.GetMyCourses;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetMyCoursesRequest;

public sealed record MyCourseSummary(
    Guid EnrollmentId,
    Guid CourseId,
    string CourseTitle,
    CourseStatus CourseStatus,
    DateTime EnrolledOn,
    DateTime? CompletedOn,
    int TotalSteps,
    int CompletedSteps,
    double ProgressPercentage);

public sealed record GetMyCoursesResponse(IReadOnlyList<MyCourseSummary> Courses);

public sealed class GetMyCoursesHandler(
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<Step> stepRepository,
    IRepository<StepProgress> stepProgressRepository,
    ITenantContext tenantContext) : IHandler<GetMyCoursesRequest, Result<GetMyCoursesResponse>>
{
    public async Task<Result<GetMyCoursesResponse>> HandleAsync(GetMyCoursesRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return Result.Success(new GetMyCoursesResponse([]));
        }

        var enrollments = (await enrollmentRepository.GetAllAsync(cancellationToken))
            .Where(e => e.StudentId == studentId)
            .ToList();

        var courses = (await courseRepository.GetAllAsync(cancellationToken))
            .ToDictionary(c => c.Id);

        var stepCountsByCourse = (await stepRepository.GetAllAsync(cancellationToken))
            .GroupBy(s => s.CourseId)
            .ToDictionary(g => g.Key, g => g.Count());

        var completedCountsByEnrollment = (await stepProgressRepository.GetAllAsync(cancellationToken))
            .GroupBy(sp => sp.EnrollmentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var summaries = enrollments
            .Where(e => courses.ContainsKey(e.CourseId))
            .OrderByDescending(e => e.EnrolledOn)
            .Select(e =>
            {
                var course = courses[e.CourseId];
                var totalSteps = stepCountsByCourse.GetValueOrDefault(e.CourseId);
                var completedSteps = completedCountsByEnrollment.GetValueOrDefault(e.Id);
                var progress = totalSteps == 0 ? 0d : Math.Round(100.0 * completedSteps / totalSteps, 2);

                return new MyCourseSummary(
                    e.Id, e.CourseId, course.Title, course.Status, e.EnrolledOn, e.CompletedOn,
                    totalSteps, completedSteps, progress);
            })
            .ToList();

        return Result.Success(new GetMyCoursesResponse(summaries));
    }
}
