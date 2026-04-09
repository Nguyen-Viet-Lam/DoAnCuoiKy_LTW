using System.Net;
using System.Net.Mail;
using System.Text;
using DoAnLTW.Web.Models.Options;
using Microsoft.Extensions.Options;

namespace DoAnLTW.Web.Services.Email;

/// <summary>Service gửi email thật qua SMTP hoặc lưu bản email HTML khi đang ở môi trường development.</summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Khởi tạo lớp SmtpEmailSender và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public SmtpEmailSender(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailSender> logger,
        IWebHostEnvironment environment)
    {
        _options = options.Value;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Gửi email thật sự ra ngoài thông qua máy chủ SMTP đã cấu hình.
    /// </summary>
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) ||
            string.IsNullOrWhiteSpace(_options.Username) ||
            string.IsNullOrWhiteSpace(_options.Password))
        {
            var fallbackPath = await SaveDevelopmentCopyAsync(message, "smtp-not-configured", cancellationToken);
            _logger.LogWarning("SMTP chua cau hinh day du. Bo qua email toi {Email}.", message.To);

            if (!string.IsNullOrWhiteSpace(fallbackPath))
            {
                _logger.LogInformation("Da luu ban email dev tai {Path}.", fallbackPath);
            }

            return;
        }

        try
        {
            using var smtp = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(
                    string.IsNullOrWhiteSpace(_options.FromEmail) ? _options.Username : _options.FromEmail,
                    string.IsNullOrWhiteSpace(_options.FromName) ? "Finance Flow" : _options.FromName),
                Subject = message.Subject,
                Body = message.HtmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(message.To);

            cancellationToken.ThrowIfCancellationRequested();
            await smtp.SendMailAsync(mail, cancellationToken);
        }
        catch (Exception ex) when (_environment.IsDevelopment())
        {
            var fallbackPath = await SaveDevelopmentCopyAsync(message, "smtp-failed", cancellationToken);
            _logger.LogWarning(ex, "SMTP gap loi trong moi truong development. Da luu email tai {Path}.", fallbackPath);
        }
    }

    private async Task<string?> SaveDevelopmentCopyAsync(
        EmailMessage message,
        string reason,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return null;
        }

        var folder = Path.Combine(_environment.ContentRootPath, "App_Data", "DevEmails");
        Directory.CreateDirectory(folder);

        var safeRecipient = SanitizeFilePart(message.To);
        var safeSubject = SanitizeFilePart(message.Subject);
        var fileName = $"{DateTime.Now:yyyyMMdd-HHmmss}-{reason}-{safeRecipient}-{safeSubject}.html";
        var filePath = Path.Combine(folder, fileName);

        var html = $$"""
                     <!DOCTYPE html>
                     <html lang="vi">
                     <head>
                         <meta charset="utf-8" />
                         <title>{{message.Subject}}</title>
                     </head>
                     <body style="font-family:Segoe UI,Arial,sans-serif;background:#f7fbfc;padding:24px;">
                         <div style="max-width:760px;margin:0 auto;background:#ffffff;border:1px solid #dbe7ec;border-radius:16px;padding:24px;">
                             <p><strong>To:</strong> {{message.To}}</p>
                             <p><strong>Subject:</strong> {{message.Subject}}</p>
                             <hr style="border:none;border-top:1px solid #e2e8f0;margin:20px 0;" />
                             {{message.HtmlBody}}
                         </div>
                     </body>
                     </html>
                     """;

        await File.WriteAllTextAsync(filePath, html, Encoding.UTF8, cancellationToken);
        return filePath;
    }

    private static string SanitizeFilePart(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            builder.Append(invalidChars.Contains(character) ? '-' : character);
        }

        return builder.ToString().Trim().Replace(' ', '-');
    }
}
