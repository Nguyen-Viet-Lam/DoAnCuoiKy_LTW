using System.Threading.Channels;

namespace DoAnLTW.Web.Services.Email;

/// <summary>Hàng đợi email trong bộ nhớ để tách việc tạo email khỏi việc gửi email nền.</summary>
public class EmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>();

    /// <summary>
    /// Đưa một email mới vào hàng đợi nền để gửi sau.
    /// </summary>
    public ValueTask QueueAsync(EmailMessage message)
    {
        return _channel.Writer.WriteAsync(message);
    }

    /// <summary>
    /// Lấy email kế tiếp ra khỏi hàng đợi để background service xử lý.
    /// </summary>
    public ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
