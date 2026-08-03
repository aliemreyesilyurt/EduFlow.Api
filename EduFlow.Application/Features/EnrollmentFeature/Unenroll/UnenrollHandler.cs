namespace EduFlow.Application.Features.EnrollmentFeature.Unenroll;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UnenrollRequest(Guid CourseId);

public sealed class UnenrollHandler(
    IRepository<Enrollment> enrollmentRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UnenrollRequest, Result>
{
    public async Task<Result> HandleAsync(UnenrollRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var enrollment = await enrollmentRepository.FindAsync(
            e => e.CourseId == command.CourseId && e.StudentId == studentId, cancellationToken);

        if (enrollment is null)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        await enrollmentRepository.DeleteAsync(enrollment, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
