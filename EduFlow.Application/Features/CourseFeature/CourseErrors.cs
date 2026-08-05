using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.CourseFeature;

public static class CourseErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Courses.NotFound", $"The course with Id '{id}' was not found");

    public static readonly Error Forbidden =
        Error.Forbidden("Courses.Forbidden", "You do not have permission to manage this course");

    public static readonly Error AlreadyPublished =
        Error.Conflict("Courses.AlreadyPublished", "The course is already published");

    public static readonly Error AlreadyArchived =
        Error.Conflict("Courses.AlreadyArchived", "The course is already archived");
}
