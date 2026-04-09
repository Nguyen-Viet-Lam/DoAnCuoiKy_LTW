namespace DoAnLTW.Web.Services.Email;

/// <summary>Background service đọc email từ queue và gửi lần lượt bằng service gửi email đã đăng ký.</summary>
public class QueuedEmailBackgroundService : BackgroundService
{
    private readonly EmailQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QueuedEmailBackgroundService> _logger;

    /// <summary>
    /// Khởi tạo lớp QueuedEmailBackgroundService và nhận các dependency cần cho quá trình xử lý.
    /// </summary>
    public QueuedEmailBackgroundService(
        EmailQueue queue,
        IServiceProvider serviceProvider,
        ILogger<QueuedEmailBackgroundService> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _queue.DequeueAsync(stoppingToken);
                using var scope = _serviceProvider.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await sender.SendAsync(message, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Không gửi được email trong hàng đợi.");
            }
        }
    }
}
