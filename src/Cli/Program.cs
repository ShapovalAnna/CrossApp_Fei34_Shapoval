using Core;
using System.Text.Encodings.Web;
using System.Text.Json;

EnvironmentReport report = EnvironmentInfo.Collect();

bool jsonMode = args.Contains("--json");

if (jsonMode)
{
    var info = new
    {
        Student = "Шаповал Анна, група ФЕІ-34",
        OsDescription = report.OsDescription,
        Architecture = report.ProcessArchitecture,
        Runtime = report.FrameworkDescription,
        DetectedRid = report.DetectedRid,
        ReportedRid = report.ReportedRid,
        BaseDirectory = report.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)"
    };

    string json = JsonSerializer.Serialize(info, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    Console.WriteLine(json);
}
else
{
    Console.WriteLine("CrossApp – інформація про середовище");
    Console.WriteLine("Студентка: Шаповал Анна, група ФЕІ-34");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС : {report.OsDescription}");
    Console.WriteLine($"Runtime : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено): {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}");
    Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)");
}