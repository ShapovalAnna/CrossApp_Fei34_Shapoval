using System.Diagnostics;
using Core;
using Core.Domain;
using Core.Reports;

namespace Cli;

// ЛАБ 7: підкоманди звітів. Тут лише виклики ReportService і вивід (жодного Where/GroupBy).
public static class ReportCommands
{
    public static int Run(string command, IReadOnlyList<BookCopy> copies)
    {
        var today = DateTime.Today;
        var books = SampleData.Books();
        var loans = SampleData.Loans(today);

        switch (command)
        {
            case "report-top":
                PrintLines("Топ-5 читачів за кількістю видач:", ReportService.TopReaders(loans, 5),
                    r => $"{r.ReaderId,-8} видач: {r.Loans,2}   відкритих: {r.OpenLoans,2}");
                break;

            case "report-overdue":
                PrintLines($"Прострочені видачі (термін {ReportService.LoanPeriodDays} днів) станом на {today:dd.MM.yyyy}:",
                    ReportService.Overdue(loans, today),
                    o => $"{o.LoanId,-7} {o.CopyId,-7} {o.ReaderId,-7} до {o.DueOn:dd.MM.yyyy}  прострочено {o.DaysOverdue,2} дн.");
                break;

            case "report-groups":
                PrintLines("Видання за десятиліттями:", ReportService.ByDecade(books, copies), FormatGroup);
                break;

            case "report-sum":
                var totals = ReportService.Totals(copies);
                Console.WriteLine($"  видань {totals.Books}, примірників {totals.Copies}, середнє {totals.Average:F2} на видання, макс {totals.Max}");
                break;

            case "report-defer":
                var d = ReportService.DeferredDemo(copies);
                Console.WriteLine("Відкладене виконання (запит: примірники на полиці):");
                Console.WriteLine($"  до змін джерела:             {d.Before}");
                Console.WriteLine($"  після додавання примірника:  {d.After}");
                Console.WriteLine($"  знімок (ToList) після Clear: {d.Snapshot}");
                Console.WriteLine($"  сам запит після Clear:       {d.QueryAfterClear}");
                break;

            // ----- додаткове 1: Join і GroupJoin -----
            case "report-join":
                PrintLines("Видачі з назвами видань (Join):", ReportService.LoanLines(loans, copies, books),
                    l => $"{l.LoanId,-7} {l.Title,-34} {l.ReaderId,-7} {l.IssuedOn:dd.MM.yyyy}  {(l.IsOpen ? "відкрита" : "закрита")}");
                break;

            case "report-copies":
                PrintLines("Примірників на видання (GroupJoin, видання з 0 теж є):", ReportService.CopiesPerBook(books, copies), FormatTitle);
                break;

            // ----- додаткове 2: TopBy у двох звітах -----
            case "report-topbooks":
                PrintLines("Топ-3 видань за кількістю примірників (TopBy):", ReportService.TopBooksByCopies(books, copies, 3), FormatTitle);
                PrintLines("Топ-3 видань за кількістю видач (TopBy):", ReportService.TopBooksByLoans(loans, copies, books, 3), FormatTitle);
                break;

            // ----- додаткове 3: синтаксис запитів -----
            case "report-query":
                var methodStyle = ReportService.ByDecade(books, copies);
                var queryStyle = ReportService.ByDecadeQuery(books, copies);
                PrintLines("Видання за десятиліттями (синтаксис запитів from ... group ... into):", queryStyle, FormatGroup);
                Console.WriteLine($"Збігається з методним синтаксисом: {methodStyle.SequenceEqual(queryStyle)}");
                break;

            // ----- додаткове 4: Stopwatch -----
            case "report-bench":
                Bench(today);
                break;

            default:
                Console.WriteLine("report-top | report-overdue | report-groups | report-sum | report-defer |");
                Console.WriteLine("report-join | report-copies | report-topbooks | report-query | report-bench");
                return 1;
        }
        return 0;
    }

    private static string FormatGroup(GroupSummary g) =>
        $"{g.Key,-10} видань: {g.Count,2}  примірників: {g.TotalQuantity,3}";

    private static string FormatTitle(TitleCount t) =>
        $"{t.Title,-34} {t.Count,2}";

    // Один формат друку для всіх звітів; Func<T, string> каже, як виглядає один рядок
    private static void PrintLines<T>(string title, IReadOnlyList<T> lines, Func<T, string> format)
    {
        Console.WriteLine(title);
        if (lines.Count == 0)
        {
            Console.WriteLine("  (порожньо)");
            return;
        }
        foreach (var line in lines)
            Console.WriteLine("  " + format(line));
    }

    // ЛАБ 7, додаткове 4: час звіту на 100 000 згенерованих видач
    private static void Bench(DateTime today)
    {
        const int N = 100_000;
        var loans = SampleData.GenerateLoans(N, today);
        Console.WriteLine($"Згенеровано видач: {N}");

        ReportService.TopReaders(loans, 5);        // прогрів JIT, щоб перший запуск не спотворив час
        ReportService.TopReadersEager(loans, 5);

        Measure("без проміжних ToList  ", () => ReportService.TopReaders(loans, 5));
        Measure("ToList у кожному кроці", () => ReportService.TopReadersEager(loans, 5));
    }

    private static void Measure(string name, Action run)
    {
        const int Runs = 20;
        long bytesBefore = GC.GetAllocatedBytesForCurrentThread();
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < Runs; i++)
            run();
        sw.Stop();
        long bytes = GC.GetAllocatedBytesForCurrentThread() - bytesBefore;
        Console.WriteLine($"  {name}: {sw.Elapsed.TotalMilliseconds / Runs,8:F2} мс/запуск, {bytes / Runs / 1024,8:N0} КБ/запуск");
    }
}
