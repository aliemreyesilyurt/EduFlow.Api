using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.EnrollmentFeature;

public static class EnrollmentErrors
{
    public static readonly Error AlreadyEnrolled =
        Error.Conflict("Enrollments.AlreadyEnrolled", "You are already enrolled in this course");

    public static readonly Error CourseNotPublished =
        Error.Conflict("Enrollments.CourseNotPublished", "You can only enroll in a published course");

    public static readonly Error NotEnrolled =
        Error.Forbidden("Enrollments.NotEnrolled", "You are not enrolled in this course");
}
