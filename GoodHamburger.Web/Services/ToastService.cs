namespace GoodHamburger.Web.Services;

public enum ToastType { Success, Error, Info, Warning }

public sealed record ToastMessage(string Id, string Message, ToastType Type, DateTimeOffset CreatedAt);

/// <summary>
/// Serviço singleton para exibir notificações toast em toda a aplicação.
/// Componentes se inscrevem em <see cref="OnChanged"/> para receber atualizações.
/// </summary>
public sealed class ToastService
{
    private readonly List<ToastMessage> _toasts = [];
    public IReadOnlyList<ToastMessage> Toasts => _toasts.AsReadOnly();

    public event Action? OnChanged;

    public void Show(string message, ToastType type = ToastType.Info, int durationMs = 4000)
    {
        var toast = new ToastMessage(Guid.NewGuid().ToString("N"), message, type, DateTimeOffset.UtcNow);
        _toasts.Add(toast);
        NotifyChanged();

        _ = Task.Delay(durationMs).ContinueWith(_ => Dismiss(toast.Id));
    }

    public void Success(string message) => Show(message, ToastType.Success);
    public void Error(string message) => Show(message, ToastType.Error, 6000);
    public void Info(string message) => Show(message, ToastType.Info);
    public void Warning(string message) => Show(message, ToastType.Warning);

    public void Dismiss(string id)
    {
        if (_toasts.RemoveAll(t => t.Id == id) > 0)
            NotifyChanged();
    }

    private void NotifyChanged() => OnChanged?.Invoke();
}
