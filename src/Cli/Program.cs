using System.Runtime.InteropServices;
using System.Text.Json;

bool jsonMode = args.Contains("--json");

if (jsonMode)
{
    var info = new
    {
        Student = "Шаповал Анна, група ФЕІ-34",
        OsDescription = RuntimeInformation.OSDescription,
        OsEnvironment = Environment.OSVersion.ToString(),
        Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
        DotNetVersion = Environment.Version.ToString(),
        Runtime = RuntimeInformation.FrameworkDescription,
        BaseDirectory = AppContext.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)"
    };

    string json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
    Console.WriteLine(json);
}
else
{
    Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
    Console.WriteLine("Студентка: Шаповал Анна, група ФЕІ-34");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription) : {RuntimeInformation.OSDescription}");
    Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}");
    Console.WriteLine($"Архітектура процесу : {RuntimeInformation.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR) : {Environment.Version}");
    Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку : {AppContext.BaseDirectory}");
    Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)");
}