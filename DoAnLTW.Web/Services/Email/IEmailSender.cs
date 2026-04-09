namespace DoAnLTW.Web.Services.Email;

/// <summary>Hợp đồng trừu tượng cho mọi service có khả năng gửi email từ hệ thống.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
