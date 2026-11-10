using Core.Abstractions;

namespace Cli;

// ЛАБ 6, додаткове 1: сповіщення в консоль
public sealed class ConsoleSink : INotificationSink
{
    public void Notify(string message) => Console.WriteLine($"  [консоль] {message}");
}