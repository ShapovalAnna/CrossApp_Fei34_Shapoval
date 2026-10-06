using Core.Import;
using Core;
using Core.Dto;
using System.Text.Encodings.Web;
using System.Text.Json;

EnvironmentReport report = EnvironmentInfo.Collect();

bool jsonMode = args.Contains("--json");

// --- Лабораторна 3: шлях до CSV береться з аргументів, --json пропускаємо ---
string path = args.FirstOrDefault(a => a != "--json")
    ?? Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<BookDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    var ext => throw new NotSupportedException($"Непідтримуване розширення файлу: {ext}")
};

if (jsonMode)
{
    var info = new EnvironmentReportDto
    {
        Student = "Шаповал Анна, група ФЕІ-34",
        OsDescription = report.OsDescription,
        Architecture = report.ProcessArchitecture,
        Runtime = report.FrameworkDescription,
        DetectedRid = report.DetectedRid,
        ReportedRid = report.ReportedRid,
        BaseDirectory = report.BaseDirectory,
        CurrentDirectory = report.CurrentDirectory,
        BuildNote = report.BuildNote,
        Domain = "Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)"
    };

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        TypeInfoResolver = AppJsonContext.Default
    };

    Console.WriteLine(JsonSerializer.Serialize(info, options));
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
    Console.WriteLine($"Поточний каталог : {report.CurrentDirectory}");
    Console.WriteLine($"Збірка : {report.BuildNote}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область: Бібліотека (Book, BookCopy, Reader, Loan)");
}

// --- Лабораторна 3: вивід результату імпорту (в обох режимах) ---
Console.WriteLine(new string('-', 52));
Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (BookDto b in result.Items.Take(5))
    Console.WriteLine($"  {b.Id,-6} {b.Isbn,-18} {b.Title,-30} {b.Year,5}");

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
        Console.WriteLine($"  ! {e}");
}

// --- Додаткове завдання: статистика імпорту одним рядком ---
int total = result.Items.Count + result.Errors.Count;
double errorRate = total == 0 ? 0 : (double)result.Errors.Count / total * 100;
Console.WriteLine($"Усього: {total}, прийнято: {result.Items.Count}, пропущено: {result.Errors.Count}, % помилок: {errorRate:F1}%");

// --- Додаткове завдання: мішаний імпорт книг і читачів за префіксом ---
MixedImportResult mixed = MixedCsvImporter.Load(Path.Combine("data", "sample-mixed.csv"));
Console.WriteLine($"Мішаний імпорт — книг: {mixed.Books.Count}, читачів: {mixed.Readers.Count}, помилок: {mixed.Errors.Count}");

return 0;