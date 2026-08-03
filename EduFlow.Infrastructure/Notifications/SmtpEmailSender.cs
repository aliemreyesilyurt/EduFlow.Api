using EduFlow.Application.Abstractions.Notifications;
using EduFlow.Application.Abstractions.Settings;
using EduFlow.Application.Constants;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EduFlow.Infrastructure.Notifications;

/// <summary>
/// Renders the requested template and sends it via SMTP using settings resolved from
/// <see cref="ISystemSettingsService"/> (tenant override, falling back to the global default).
/// If no SMTP host is configured, the rendered email is logged instead of sent — this keeps
/// local development working with zero configuration.
/// </summary>
internal sealed class SmtpEmailSender(
    ISystemSettingsService systemSettingsService,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(EmailRequest request, CancellationToken cancellationToken)
    {
        var html = EmailTemplateRenderer.Render(request.Template, request.Subject, request.Tokens);

        var host = await systemSettingsService.GetValueAsync(SystemSettingKeys.SmtpHost, request.TenantId, cancellationToken);

        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogInformation(
                "SMTP not configured; logging email instead. To: {To} — {Subject}\n{HtmlBody}",
                request.To,
                request.Subject,
                html);
            return;
        }

        var port = int.TryParse(
            await systemSettingsService.GetValueAsync(SystemSettingKeys.SmtpPort, request.TenantId, cancellationToken),
            out var parsedPort)
            ? parsedPort
            : 587;

        var useSsl = bool.TryParse(
            await systemSettingsService.GetValueAsync(SystemSettingKeys.SmtpUseSsl, request.TenantId, cancellationToken),
            out var parsedUseSsl) && parsedUseSsl;

        var username = await systemSettingsService.GetValueAsync(SystemSettingKeys.SmtpUsername, request.TenantId, cancellationToken);
        var password = await systemSettingsService.GetValueAsync(SystemSettingKeys.SmtpPassword, request.TenantId, cancellationToken);
        var fromAddress = await systemSettingsService.GetValueAsync(SystemSettingKeys.SmtpFromAddress, request.TenantId, cancellationToken);
        var fromName = await systemSettingsService.GetValueAsync(SystemSettingKeys.SmtpFromName, request.TenantId, cancellationToken);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName ?? "EduFlow", fromAddress ?? username ?? "no-reply@eduflow.local"));
        message.To.Add(MailboxAddress.Parse(request.To));
        message.Subject = request.Subject;
        message.Body = new BodyBuilder { HtmlBody = html }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable, cancellationToken);

        if (!string.IsNullOrEmpty(username))
        {
            await client.AuthenticateAsync(username, password ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
