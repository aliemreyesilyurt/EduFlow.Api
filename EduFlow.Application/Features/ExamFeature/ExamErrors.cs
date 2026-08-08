using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.ExamFeature;

public static class ExamErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Exams.NotFound", $"The exam with Id '{id}' was not found");

    public static readonly Error Forbidden =
        Error.Forbidden("Exams.Forbidden", "You do not have permission to manage this course's exam");

    public static readonly Error AlreadyExists =
        Error.Conflict("Exams.AlreadyExists", "This course already has an exam");

    public static readonly Error AlreadyPublished =
        Error.Conflict("Exams.AlreadyPublished", "The exam is already published");

    public static readonly Error NotPublished =
        Error.Conflict("Exams.NotPublished", "The exam is not published yet");

    public static readonly Error NoQuestions =
        Error.Validation("Exams.NoQuestions", "The exam must have at least one question before it can be published");

    public static Error QuestionNotFound(Guid id) =>
        Error.NotFound("Exams.QuestionNotFound", $"The question with Id '{id}' was not found");

    public static readonly Error ReorderMismatch =
        Error.Validation("Exams.ReorderMismatch", "The provided question ids must exactly match the exam's current questions");

    public static readonly Error CourseNotCompleted =
        Error.Forbidden("Exams.CourseNotCompleted", "You must complete all steps of this course before taking the exam");

    public static readonly Error AttemptLimitReached =
        Error.Forbidden("Exams.AttemptLimitReached", "You have used all of your allowed attempts for this exam");

    public static Error AttemptNotFound(Guid id) =>
        Error.NotFound("Exams.AttemptNotFound", $"The exam attempt with Id '{id}' was not found");

    public static readonly Error AttemptAlreadySubmitted =
        Error.Conflict("Exams.AttemptAlreadySubmitted", "This exam attempt has already been submitted");
}
