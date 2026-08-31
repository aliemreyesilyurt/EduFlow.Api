using EduFlow.Application.Abstractions.Notifications;

namespace EduFlow.Application.Features.AuthFeature;

internal static class AuthEmails
{
    public static EmailRequest EmailVerification(
        string clientBaseUrl, string to, string firstName, Guid userId, string encodedToken, Guid? tenantId)
    {
        var link = $"{clientBaseUrl}/confirm-email?userId={userId}&token={encodedToken}";

        return new EmailRequest(
            to,
            "Confirm your EduFlow email address",
            EmailTemplate.EmailVerification,
            new Dictionary<string, string> { ["FirstName"] = firstName, ["ActionUrl"] = link },
            tenantId);
    }

    public static EmailRequest PasswordReset(
        string clientBaseUrl, string to, string firstName, Guid userId, string encodedToken, Guid? tenantId)
    {
        var link = $"{clientBaseUrl}/reset-password?userId={userId}&token={encodedToken}";

        return new EmailRequest(
            to,
            "Reset your EduFlow password",
            EmailTemplate.PasswordReset,
            new Dictionary<string, string> { ["FirstName"] = firstName, ["ActionUrl"] = link },
            tenantId);
    }

    public static EmailRequest InstructorInvitation(
        string clientBaseUrl, string to, string firstName, Guid userId, string encodedToken, Guid? tenantId)
    {
        var link = $"{clientBaseUrl}/accept-invitation?userId={userId}&token={encodedToken}";

        return new EmailRequest(
            to,
            "You've been invited to EduFlow as an instructor",
            EmailTemplate.InstructorInvitation,
            new Dictionary<string, string> { ["FirstName"] = firstName, ["ActionUrl"] = link },
            tenantId);
    }

    public static EmailRequest StudentInvitation(
        string clientBaseUrl, string to, string firstName, Guid userId, string encodedToken, Guid? tenantId)
    {
        var link = $"{clientBaseUrl}/accept-invitation?userId={userId}&token={encodedToken}";

        return new EmailRequest(
            to,
            "You've been invited to EduFlow as a student",
            EmailTemplate.StudentInvitation,
            new Dictionary<string, string> { ["FirstName"] = firstName, ["ActionUrl"] = link },
            tenantId);
    }
}
