using System.Threading.Channels;

namespace DoAnLTW.Web.Services.Email;

public class EmailQueue
{
    private readonly Channel<EmailMessage> _channel = Channel.CreateUnbounded<EmailMessage>();

    public ValueTask QueueAsync(EmailMessage message)
    {
        return _channel.Writer.WriteAsync(message);
    }

    public ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
