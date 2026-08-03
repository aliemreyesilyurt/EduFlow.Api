namespace EduFlow.Application.Constants;

public static class SystemSettingKeys
{
    public const string SmtpHost = "Smtp.Host";
    public const string SmtpPort = "Smtp.Port";
    public const string SmtpUseSsl = "Smtp.UseSsl";
    public const string SmtpUsername = "Smtp.Username";
    public const string SmtpPassword = "Smtp.Password";
    public const string SmtpFromAddress = "Smtp.FromAddress";
    public const string SmtpFromName = "Smtp.FromName";

    /// <summary>Keys whose values must be encrypted at rest and masked on read.</summary>
    public static readonly HashSet<string> Secrets = [SmtpPassword];
}
