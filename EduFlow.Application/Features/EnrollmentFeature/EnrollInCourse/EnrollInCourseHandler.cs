namespace EduFlow.Application.Features.EnrollmentFeature.EnrollInCourse;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record EnrollInCourseRequest(Guid CourseId);

public sealed record EnrollInCourseResponse(Guid Id, Guid CourseId, Guid StudentId, DateTime EnrolledOn);

public sealed class EnrollInCourseHandler(
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<EnrollInCourseRequest, Result<EnrollInCourseResponse>>
{
    public async Task<Result<EnrollInCourseResponse>> HandleAsync(EnrollInCourseRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return CourseErrors.Forbidden;
        }

        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        if (course.Status != CourseStatus.Published)
        {
            return EnrollmentErrors.CourseNotPublished;
        }

        var existing = await enrollmentRepository.FindAsync(
            e => e.CourseId == command.CourseId && e.StudentId == studentId, cancellationToken);

        if (existing is not null)
        {
            return EnrollmentErrors.AlreadyEnrolled;
        }

        var enrollment = new Enrollment
        {
            Id = Guid.CreateVersion7(),
            CourseId = command.CourseId,
            StudentId = studentId,
            EnrolledOn = DateTime.UtcNow
        };

        await enrollmentRepository.AddAsync(enrollment, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new EnrollInCourseResponse(
            enrollment.Id, enrollment.CourseId, enrollment.StudentId, enrollment.EnrolledOn));
    }
}
