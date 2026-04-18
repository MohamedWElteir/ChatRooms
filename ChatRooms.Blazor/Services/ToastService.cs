namespace ChatRooms.Blazor.Services;

public enum ToastVariant { Success, Error, Info }

public sealed record Toast(Guid Id, string Title, string? Message, ToastVariant Variant);

public sealed class ToastService(ILogger<ToastService> logger)
{
    private readonly Lock _toastsLock = new();
    private readonly List<Toast> _toasts = [];

    public IReadOnlyList<Toast> Toasts
    {
        get
        {
            lock (_toastsLock)
            {
                return [.. _toasts];
            }
        }
    }

    public event Action? OnChange;

    public void Show(string title, string? message = null, ToastVariant variant = ToastVariant.Info)
    {
        var toast = new Toast(Guid.NewGuid(), title, message, variant);

        lock (_toastsLock)
        {
            _toasts.Add(toast);
        }

        OnChange?.Invoke();

        _ = Task.Run(async () =>
        {
            try
            {
                await RemoveAfterDelay(toast.Id, DefaultToastDisplayDurationMs);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while removing toast with ID {ToastId}", toast.Id);
            }
        });
    }

    public void Success(string title, string? message = null) => Show(title, message, ToastVariant.Success);
    public void Error(string title, string? message = null) => Show(title, message, ToastVariant.Error);
    public void Info(string title, string? message = null) => Show(title, message, ToastVariant.Info);

    public void Dismiss(Guid id)
    {
        lock (_toastsLock)
        {
            _toasts.RemoveAll(t => t.Id == id);
        }

        OnChange?.Invoke();
    }

    private async Task RemoveAfterDelay(Guid id, int delay)
    {
        await Task.Delay(delay);
        Dismiss(id);
    }

    private const int DefaultToastDisplayDurationMs = 4000;
}