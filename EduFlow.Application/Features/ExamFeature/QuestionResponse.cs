namespace EduFlow.Application.Features.ExamFeature;

public sealed record QuestionOptionResponse(Guid Id, string Text, int Order, bool? IsCorrect);

public sealed record QuestionResponse(
    Guid Id,
    Guid ExamId,
    string Text,
    int Order,
    int Points,
    IReadOnlyList<QuestionOptionResponse> Options);
