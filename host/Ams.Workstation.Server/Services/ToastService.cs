using System;

namespace Ams.Workstation.Server.Services;

public enum ToastLevel { Info, Success, Warning, Error }

public record ToastMessage(string Text, ToastLevel Level, Guid Id)
{
    public ToastMessage(string text, ToastLevel level) : this(text, level, Guid.NewGuid()) { }
}

public class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Show(string message, ToastLevel level = ToastLevel.Info)
        => OnShow?.Invoke(new ToastMessage(message, level));

    public void Success(string message) => Show(message, ToastLevel.Success);
    public void Error(string message) => Show(message, ToastLevel.Error);
    public void Info(string message) => Show(message, ToastLevel.Info);
    public void Warning(string message) => Show(message, ToastLevel.Warning);
}
