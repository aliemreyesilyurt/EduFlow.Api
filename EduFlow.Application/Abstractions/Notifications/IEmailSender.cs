namespace EduFlow.Application.Abstractions.Notifications;

public interface IEmailSender
{
    Task SendAsync(EmailRequest request, CancellationToken cancellationToken);
}

public enum EmailTemplate
{
    EmailVerification,
    PasswordReset,
    InstructorInvitation
}

/// <summary>
/// Describes an email to send: the Application layer decides what to send (template + tokens),
/// the Infrastructure layer decides how (rendering + transport).
/// </summary>
public sealed record EmailRequest(
    string To,
    string Subject,
    EmailTemplate Template,
    IReadOnlyDictionary<string, string> Tokens,
    Guid? TenantId);
