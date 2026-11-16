using Core.Domain;
using Core.Dto;

namespace Core.Reports;

// ЛАБ 7: чисті функції звітів. Приймають IEnumerable<T>, повертають IReadOnlyList<DTO>.
// Ні Console, ні сховища тут немає, лише LINQ to Objects.
public static class ReportService
{
    public const int LoanPeriodDays = 14;   // термін видачі (у Loan немає DueDate)

    private static DateTime DueOn(Loan loan) => loan.IssuedOn.AddDays(LoanPeriodDays);

    private static Dictionary<string, int> CountCopiesByIsbn(IEnumerable<BookCopy> copies) =>
        copies.GroupBy(c => c.Isbn).ToDictionary(g => g.Key, g => g.Count());

    // 1) report-top: групування + агрегат + топ-N зі стабільним порядком (другий ключ ThenBy)
    public static IReadOnlyList<ReaderLoanLine> TopReaders(IEnumerable<Loan> loans, int n) =>
        loans.GroupBy(l => l.ReaderId, StringComparer.Ordinal)
             .Select(g => new ReaderLoanLine(g.Key, g.Count(), g.Count(l => l.Status == LoanStatus.Open)))
             .OrderByDescending(r => r.Loans)
             .ThenBy(r => r.ReaderId, StringComparer.Ordinal)
             .Take(n)
             .ToList();

    // 2) report-overdue: фільтр СПОЧАТКУ, потім сортування і проєкція
    public static IReadOnlyList<OverdueLine> Overdue(IEnumerable<Loan> loans, DateTime today) =>
        loans.Where(l => l.Status == LoanStatus.Open && DueOn(l) < today)
             .OrderBy(l => l.IssuedOn)
             .ThenBy(l => l.Id, StringComparer.Ordinal)
             .Select(l => new OverdueLine(l.Id, l.BookCopyId, l.ReaderId, DueOn(l), (today.Date - DueOn(l).Date).Days))
             .ToList();

    // 3) report-groups: GroupBy за десятиліттям, всередині групи Count і Sum
    public static IReadOnlyList<GroupSummary> ByDecade(IEnumerable<BookDto> books, IEnumerable<BookCopy> copies)
    {
        var copiesByIsbn = CountCopiesByIsbn(copies);
        return books.GroupBy(b => b.Year / 10 * 10)
                    .Select(g => new GroupSummary($"{g.Key}-{g.Key + 9}", g.Count(), g.Sum(b => copiesByIsbn.GetValueOrDefault(b.Isbn))))
                    .OrderByDescending(s => s.TotalQuantity)
                    .ThenBy(s => s.Key, StringComparer.Ordinal)
                    .ToList();
    }

    // 4) report-sum: агрегати з обробкою порожньої послідовності (іменований кортеж)
    public static (int Books, int Copies, double Average, int Max) Totals(IEnumerable<BookCopy> copies)
    {
        var perBook = copies.GroupBy(c => c.Isbn).Select(g => g.Count()).ToList();
        return perBook.Count == 0
            ? (0, 0, 0d, 0)   // порожнє джерело: Average і Max не викликаємо, інакше InvalidOperationException
            : (perBook.Count, perBook.Sum(), perBook.Average(), perBook.Max());
    }

    // 5) report-defer (крок 3 методички): відкладене виконання. Локальний список, бо All() щоразу дає новий знімок.
    public static (int Before, int After, int Snapshot, int QueryAfterClear) DeferredDemo(IEnumerable<BookCopy> copies)
    {
        var source = copies.ToList();
        var query = source.Where(c => !c.IsIssued);   // запит побудовано, не виконано
        int before = query.Count();                   // перше виконання
        source.Add(BookCopy.Create("tmp00001", "978-000-00-0000-0"));
        int after = query.Count();                    // друге виконання: число інше
        var snapshot = query.ToList();                // результат зафіксовано
        source.Clear();
        return (before, after, snapshot.Count, query.Count());   // знімок не змінився, а сам запит тепер порожній
    }

    // ===== ДОДАТКОВЕ 1: Join і GroupJoin =====

    // Join: видача + примірник + видання -> зведений DTO (два внутрішні з'єднання)
    public static IReadOnlyList<LoanLine> LoanLines(IEnumerable<Loan> loans, IEnumerable<BookCopy> copies, IEnumerable<BookDto> books) =>
        loans.Join(copies, l => l.BookCopyId, c => c.Id, (l, c) => new { Loan = l, c.Isbn })
             .Join(books, x => x.Isbn, b => b.Isbn,
                   (x, b) => new LoanLine(x.Loan.Id, b.Title, x.Loan.ReaderId, x.Loan.IssuedOn, x.Loan.Status == LoanStatus.Open))
             .OrderBy(x => x.IssuedOn)
             .ThenBy(x => x.LoanId, StringComparer.Ordinal)
             .ToList();

    // GroupJoin: видання лишається, навіть якщо в нього 0 примірників
    public static IReadOnlyList<TitleCount> CopiesPerBook(IEnumerable<BookDto> books, IEnumerable<BookCopy> copies) =>
        books.GroupJoin(copies, b => b.Isbn, c => c.Isbn, (b, cs) => new TitleCount(b.Title, cs.Count()))
             .OrderByDescending(t => t.Count)
             .ThenBy(t => t.Title, StringComparer.Ordinal)
             .ToList();

    // ===== ДОДАТКОВЕ 2: TopBy використано у двох звітах =====

    public static IReadOnlyList<TitleCount> TopBooksByCopies(IEnumerable<BookDto> books, IEnumerable<BookCopy> copies, int n) =>
        CopiesPerBook(books, copies).TopBy(t => t.Count, n, t => t.Title).ToList();

    public static IReadOnlyList<TitleCount> TopBooksByLoans(IEnumerable<Loan> loans, IEnumerable<BookCopy> copies, IEnumerable<BookDto> books, int n) =>
        LoanLines(loans, copies, books)
            .GroupBy(l => l.Title)
            .Select(g => new TitleCount(g.Key, g.Count()))
            .TopBy(t => t.Count, n, t => t.Title)
            .ToList();

    // ===== ДОДАТКОВЕ 3: той самий звіт ByDecade у синтаксисі запитів =====

    public static IReadOnlyList<GroupSummary> ByDecadeQuery(IEnumerable<BookDto> books, IEnumerable<BookCopy> copies)
    {
        var copiesByIsbn = CountCopiesByIsbn(copies);
        var query =
            from book in books
            group book by book.Year / 10 * 10 into g
            let total = g.Sum(x => copiesByIsbn.GetValueOrDefault(x.Isbn))
            orderby total descending, g.Key
            select new GroupSummary($"{g.Key}-{g.Key + 9}", g.Count(), total);
        return query.ToList();
    }

    // ===== ДОДАТКОВЕ 4: варіант TopReaders з ToList у кожному кроці (для порівняння швидкості) =====

    public static IReadOnlyList<ReaderLoanLine> TopReadersEager(IEnumerable<Loan> loans, int n)
    {
        var all = loans.ToList();
        var groups = all.GroupBy(l => l.ReaderId, StringComparer.Ordinal).ToList();
        var lines = groups.Select(g => new ReaderLoanLine(g.Key, g.Count(), g.Count(l => l.Status == LoanStatus.Open))).ToList();
        var sorted = lines.OrderByDescending(r => r.Loans).ThenBy(r => r.ReaderId, StringComparer.Ordinal).ToList();
        return sorted.Take(n).ToList();
    }
}
