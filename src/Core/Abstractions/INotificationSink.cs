namespace Core.Abstractions;

// ЛАБ 6, додаткове 1: контракт «каналу сповіщення».
// Core знає лише цей інтерфейс, а куди піде текст (консоль, файл), вирішує реалізація в Cli.
public interface INotificationSink
{
    void Notify(string message);
}