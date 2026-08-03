using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Encodings.Web;
using EduFlow.Application.Abstractions.Notifications;

namespace EduFlow.Infrastructure.Notifications;

internal static class EmailTemplateRenderer
{
    private const string ResourcePrefix = "EduFlow.Infrastructure.Notifications.Templates.";

    private static readonly Assembly Assembly = typeof(EmailTemplateRenderer).Assembly;
    private static readonly ConcurrentDictionary<string, string> TemplateCache = new();

    private static readonly IReadOnlyDictionary<EmailTemplate, string> TemplateFiles = new Dictionary<EmailTemplate, string>
    {
        [EmailTemplate.EmailVerification] = "email-verification.html",
        [EmailTemplate.PasswordReset] = "password-reset.html",
        [EmailTemplate.InstructorInvitation] = "instructor-invitation.html"
    };

    public static string Render(EmailTemplate template, string subject, IReadOnlyDictionary<string, string> tokens)
    {
        var body = ApplyTokens(LoadTemplate(TemplateFiles[template]), tokens);

        var layoutTokens = new Dictionary<string, string>
        {
            ["Title"] = subject,
            ["AppName"] = "EduFlow",
            ["Year"] = DateTime.UtcNow.Year.ToString()
        };

        var layout = ApplyTokens(LoadTemplate("_layout.html"), layoutTokens);

        // Content is already-rendered HTML (with its own tokens already escaped above), so it is
        // inserted verbatim here rather than through ApplyTokens, which would re-escape the markup.
        return layout.Replace("{{Content}}", body);
    }

    private static string ApplyTokens(string template, IReadOnlyDictionary<string, string> tokens)
    {
        foreach (var (key, value) in tokens)
        {
            template = template.Replace($"{{{{{key}}}}}", HtmlEncoder.Default.Encode(value));
        }

        return template;
    }

    private static string LoadTemplate(string fileName) =>
        TemplateCache.GetOrAdd(fileName, name =>
        {
            using var stream = Assembly.GetManifestResourceStream(ResourcePrefix + name)
                ?? throw new InvalidOperationException($"Embedded email template '{name}' was not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        });
}
