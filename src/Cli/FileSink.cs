using Core.Abstractions;

namespace Cli;

// ЛАБ 6, додаткове 1: сповіщення у файл
public sealed class FileSink(string path) : INotificationSink
{
    private readonly string _path = Path.GetFullPath(path);

    public void Notify(string message)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.AppendAllText(_path, $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}");
    }
}