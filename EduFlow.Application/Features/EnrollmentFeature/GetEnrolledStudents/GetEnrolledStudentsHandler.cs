namespace EduFlow.Application.Features.EnrollmentFeature.GetEnrolledStudents;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetEnrolledStudentsRequest(Guid CourseId);

public sealed record EnrolledStudentSummary(
    Guid EnrollmentId,
    Guid StudentId,
    DateTime EnrolledOn,
    DateTime? CompletedOn,
    int TotalSteps,
    int CompletedSteps,
    double ProgressPercentage);

public sealed record GetEnrolledStudentsResponse(IReadOnlyList<EnrolledStudentSummary> Students);

public sealed class GetEnrolledStudentsHandler(
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Step> stepRepository,
    IRepository<StepProgress> stepProgressRepository,
    ITenantContext tenantContext) : IHandler<GetEnrolledStudentsRequest, Result<GetEnrolledStudentsResponse>>
{
    public async Task<Result<GetEnrolledStudentsResponse>> HandleAsync(GetEnrolledStudentsRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null)
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        if (!CourseAccess.CanManage(course, tenantContext))
        {
            return CourseErrors.Forbidden;
        }

        var enrollments = (await enrollmentRepository.GetAllAsync(cancellationToken))
            .Where(e => e.CourseId == command.CourseId)
            .ToList();

        var totalSteps = (await stepRepository.GetAllAsync(cancellationToken))
            .Count(s => s.CourseId == command.CourseId);

        var completedCountsByEnrollment = (await stepProgressRepository.GetAllAsync(cancellationToken))
            .GroupBy(sp => sp.EnrollmentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var summaries = enrollments
            .OrderByDescending(e => e.EnrolledOn)
            .Select(e =>
            {
                var completedSteps = completedCountsByEnrollment.GetValueOrDefault(e.Id);
                var progress = totalSteps == 0 ? 0d : Math.Round(100.0 * completedSteps / totalSteps, 2);

                return new EnrolledStudentSummary(
                    e.Id, e.StudentId, e.EnrolledOn, e.CompletedOn, totalSteps, completedSteps, progress);
            })
            .ToList();

        return Result.Success(new GetEnrolledStudentsResponse(summaries));
    }
}
