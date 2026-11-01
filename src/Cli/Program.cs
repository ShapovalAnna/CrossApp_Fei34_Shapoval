using Core.Import;
using Core;
using Core.Dto;
using Core.Domain;
using System.Text.Encodings.Web;
using System.Text.Json;
using Core.Abstractions;
using Core.Storage;
using Core.Services;
using Cli;

// ============================================================
// Аргументи командного рядка
// ============================================================
IBookCopyStore inner = StoreFactory.Create(args);  // лаб. 5: має йти ДО очищення args
args = args.Where(a => a != "--file").ToArray();   // решта коду не бачить --file

// ============================================================
// Лабораторна 3: імпорт файлів (без змін від попередньої версії)
// ============================================================
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

// --- Додаткове завдання: мішаний імпорт запускається окремою командою ---
// dotnet run --project src/Cli -- data\sample-mixed.csv
bool isMixedFile = Path.GetFileNameWithoutExtension(path)
    .Contains("mixed", StringComparison.OrdinalIgnoreCase);

if (isMixedFile)
{
    MixedImportResult mixed = MixedCsvImporter.Load(path);

    Console.WriteLine("Додаткове завдання: мішаний імпорт книг і читачів за префіксом");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"Книг: {mixed.Books.Count}");
    foreach (BookDto b in mixed.Books)
        Console.WriteLine($"  B {b.Id,-6} {b.Isbn,-18} {b.Title,-30} {b.Year,5}");

    Console.WriteLine($"Читачів: {mixed.Readers.Count}");
    foreach (ReaderDto r in mixed.Readers)
        Console.WriteLine($"  R {r.Id,-6} {r.FullName}");

    if (mixed.Errors.Count > 0)
    {
        Console.WriteLine($"Помилок: {mixed.Errors.Count}");
        foreach (string e in mixed.Errors)
            Console.WriteLine($"  ! {e}");
    }

    return 0;
}

ImportResult<BookDto> result = Path.GetExtension(path).ToLowerInvariant() switch
{
    ".csv" => BookCsvImporter.Load(path),
    ".json" => BookJsonImporter.Load(path),
    var ext => throw new NotSupportedException($"Непідтримуване розширення файлу: {ext}")
};

Console.WriteLine("=== Лабораторна 3: середовище та імпорт файлів ===");
Console.WriteLine();

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

// ============================================================
// Лабораторна 4: доменна модель (BookCopy + Loan)
// ============================================================
Console.WriteLine();
Console.WriteLine("=== Лабораторна 4: доменна модель (BookCopy + Loan) ===");
Console.WriteLine();
Console.WriteLine("--- Сценарій 1: успіх ---");
//успішний шлях
BookCopy copy = BookCopy.Create("C-001", "978-966-03-5548-1");
Console.WriteLine(copy);
//Відкривається видача
Loan loan = Loan.Open("L-001", copy, "R-001", new DateTime(2026, 10, 1));
Console.WriteLine(copy);
Console.WriteLine(loan);
//Закривається видача (повернення книги)
loan.Close(new DateTime(2026, 10, 15), copy);
Console.WriteLine(copy);
Console.WriteLine(loan);

Console.WriteLine();
Console.WriteLine("--- Сценарій 2: порушення інваріантів ---");
//навмисні порушення правил
BookCopy copy2 = BookCopy.Create("C-002", "978-617-7764-11-3");
Loan loan2 = Loan.Open("L-002", copy2, "R-002", new DateTime(2026, 10, 5));

Console.WriteLine($"До спроб порушень: {copy2}");

TryDo("повторна видача вже виданого примірника", () => copy2.Issue());
TryDo("порожній ISBN", () => BookCopy.Create("C-003", "   "));
TryDo("дата повернення раніше дати видачі", () => loan2.Close(new DateTime(2026, 9, 1), copy2));

Console.WriteLine($"Після трьох спроб порушення: {copy2}");
Console.WriteLine("Спостережений факт: жодна з трьох спроб вище не змінила стан об'єкта.");

TryDo("повторне закриття вже закритої видачі", () =>
{
    loan2.Close(new DateTime(2026, 10, 20), copy2);   // легітимне закриття
    loan2.Close(new DateTime(2026, 10, 21), copy2);   // відхилено
});

Console.WriteLine($"Після легітимного закриття: {copy2}");

//Допоміжна функція
static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}

// --- Лабораторна 4 — додаткове завдання 1: ImportResult -> доменні сутності ---
Console.WriteLine();
Console.WriteLine("--- Додаткове завдання 1: ImportResult -> доменні сутності ---");
DomainImportResult domainResult = BookCopyImportAdapter.ToDomain(result);
Console.WriteLine($"Створено примірників: {domainResult.Copies.Count}, відхилено: {domainResult.Errors.Count}");
foreach (string e in domainResult.Errors)
    Console.WriteLine($"  ! {e}");

// --- Лабораторна 4 — додаткове завдання 2: інваріант на ДВІ сутності ---
Console.WriteLine();
Console.WriteLine("--- Додаткове завдання 2: ліміт 5 відкритих видач на читача ---");

var allLoans = new List<Loan>();
string[] copyIds = ["C-200", "C-201", "C-202", "C-203", "C-204", "C-205"];
foreach (string cid in copyIds)
{
    BookCopy c = BookCopy.Create(cid, "000-0");
    if (allLoans.Count < 5)
        allLoans.Add(LoanLimitPolicy.OpenWithLimit($"L-{cid}", c, "R-999", DateTime.Today, allLoans));
    else
        TryDo("6-та відкрита видача тому самому читачу (ліміт 5)", () =>
            LoanLimitPolicy.OpenWithLimit($"L-{cid}", c, "R-999", DateTime.Today, allLoans));
}

// --- Лабораторна 4 — додаткове завдання 3: явний стан LoanStatus ---
Console.WriteLine();
Console.WriteLine("--- Додаткове завдання 3: явний стан видачі (LoanStatus) ---");
Console.WriteLine($"Статус {loan.Id}: {loan.Status}");
Console.WriteLine($"Статус {loan2.Id}: {loan2.Status}");

// ============================================================
// Лабораторна 5: сервісний шар, два сховища, ручний DI
// ============================================================
Console.WriteLine();
Console.WriteLine("=== Лабораторна 5: сервісний шар, два сховища, ручний DI ===");

// Composition root: сховище створила фабрика (Cli/StoreFactory.cs),
// тут його обгортає декоратор-кеш, а сервіс бачить лише інтерфейс
var cache = new CachingBookCopyStore(inner);
IBookCopyStore store = cache;

var service = new LendingService(store);
Console.WriteLine($"Сховище: {cache.GetType().Name} → {inner.GetType().Name}");

Console.WriteLine();
Console.WriteLine("--- Сценарій 1: додати і видати примірник ---");
var created = service.AddBook("978-966-03-5548-1");
service.IssueCopy(created.Id);
Console.WriteLine($"Додано та видано: {service.Find(created.Id)}");

Console.WriteLine();
Console.WriteLine($"--- Сценарій 2: усі примірники у сховищі ({service.All().Count}) ---");
foreach (var item in service.All())
    Console.WriteLine($"  {item}");

Console.WriteLine();
Console.WriteLine("--- Сценарій 3: знайти за id і повернути ---");
Console.WriteLine($"Знайдено за id:   {service.Find(created.Id)}");
service.ReturnCopy(created.Id);
Console.WriteLine($"Після повернення: {service.Find(created.Id)}");

Console.WriteLine();
Console.WriteLine("--- Сценарій 4: відмови ---");
try { service.IssueCopy("NOPE"); }
catch (InvalidOperationException ex) { Console.WriteLine($"  Немає запису: {ex.Message}"); }

try { store.Add(created); }
catch (InvalidOperationException ex) { Console.WriteLine($"  Дубль id: {ex.Message}"); }

Console.WriteLine();
Console.WriteLine("--- Сценарій 5: пошук за умовою (Func) ---");
var issuedNow = service.Search(x => x.IsIssued);
Console.WriteLine($"Видані зараз: {issuedNow.Count}");
foreach (var item in issuedNow)
    Console.WriteLine($"  {item}");

string sampleIsbn = "978-966-03-5548-1";
Console.WriteLine($"З ISBN {sampleIsbn}: {service.Search(x => x.Isbn == sampleIsbn).Count}");

Console.WriteLine();
Console.WriteLine($"--- Кеш: влучань {cache.Hits}, промахів {cache.Misses} ---");

return 0;