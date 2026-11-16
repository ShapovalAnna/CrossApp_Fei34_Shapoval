using Core.Domain;
using Core.Dto;
//початкові дані
namespace Core;

public static class SampleData
{
    // ЛАБ 7: 18 примірників 11 видань (в "Кобзаря" і "Чорного ворона" по 3, в інших 1-2)
    public static IEnumerable<BookCopy> BookCopies() =>
    [
        BookCopy.Create("C-001", "978-966-03-5548-1"),
        BookCopy.Create("C-002", "978-617-7764-11-3"),
        BookCopy.Create("C-003", "978-966-97925-0-6"),
        BookCopy.Create("C-004", "978-966-03-5548-1"),
        BookCopy.Create("C-005", "978-966-03-5548-1"),
        BookCopy.Create("C-006", "978-617-7764-11-3"),
        BookCopy.Create("C-007", "978-966-97925-0-6"),
        BookCopy.Create("C-008", "978-966-97925-0-6"),
        BookCopy.Create("C-009", "978-5-699-30718-8"),
        BookCopy.Create("C-010", "978-5-699-30718-8"),
        BookCopy.Create("C-011", "978-966-378-549-3"),
        BookCopy.Create("C-012", "978-617-7279-12-4"),
        BookCopy.Create("C-013", "978-617-7279-12-4"),
        BookCopy.Create("C-014", "978-966-03-7266-2"),
        BookCopy.Create("C-015", "978-966-14-9221-3"),
        BookCopy.Create("C-016", "978-617-7544-29-9"),
        BookCopy.Create("C-017", "978-966-448-034-7"),
        BookCopy.Create("C-018", "978-966-97059-0-0")
    ];

    // ЛАБ 7: 13 видань різних років (2 останні без жодного примірника, для демонстрації GroupJoin)
    public static IEnumerable<BookDto> Books() =>
    [
        new BookDto("B-001", "978-966-03-5548-1", "Кобзар", 1840),
        new BookDto("B-002", "978-617-7764-11-3", "Тигролови", 1944),
        new BookDto("B-003", "978-966-97925-0-6", "Чорний ворон", 2009),
        new BookDto("B-004", "978-5-699-30718-8", "Майстер і Маргарита", 1967),
        new BookDto("B-005", "978-966-378-549-3", "Захар Беркут", 1883),
        new BookDto("B-006", "978-617-7279-12-4", "Інтернат", 2017),
        new BookDto("B-007", "978-966-03-7266-2", "Лісова пісня", 1911),
        new BookDto("B-008", "978-966-14-9221-3", "Доба великого джихаду", 2004),
        new BookDto("B-009", "978-617-7544-29-9", "Фелікс Австрія", 2014),
        new BookDto("B-010", "978-966-448-034-7", "Хіба ревуть воли, як ясла повні?", 1875),
        new BookDto("B-011", "978-966-97059-0-0", "Місто", 1928),
        new BookDto("B-012", "978-966-03-0101-1", "Кайдашева сім'я", 1879),
        new BookDto("B-013", "978-966-03-0202-2", "Тіні забутих предків", 1911)
    ];

    // ЛАБ 7: 19 видач. Дати задано "днів тому", щоб прострочені були завжди, коли б ти не запускала.
    public static IReadOnlyList<Loan> Loans(DateTime today)
    {
        var copies = BookCopies().ToDictionary(c => c.Id);
        var loans = new List<Loan>();

        // (id видачі, id примірника, читач, скільки днів тому видано, скільки днів тому повернено або null)
        (string Id, string Copy, string Reader, int IssuedAgo, int? ReturnedAgo)[] rows =
        [
            ("L-001", "C-001", "R-001", 40, 30),
            ("L-002", "C-002", "R-001", 35, 20),
            ("L-003", "C-003", "R-002", 30, 25),
            ("L-004", "C-004", "R-003", 28, 10),
            ("L-005", "C-005", "R-001", 25, 15),
            ("L-006", "C-006", "R-002", 22, 12),
            ("L-007", "C-007", "R-004", 21, null),
            ("L-008", "C-009", "R-001", 18, null),
            ("L-009", "C-010", "R-003", 16, null),
            ("L-010", "C-011", "R-005", 12, null),
            ("L-011", "C-012", "R-002", 10, null),
            ("L-012", "C-013", "R-006", 8, 3),
            ("L-013", "C-014", "R-004", 6, null),
            ("L-014", "C-015", "R-003", 5, null),
            ("L-015", "C-016", "R-002", 3, null),
            ("L-016", "C-017", "R-001", 2, null),
            ("L-017", "C-018", "R-005", 30, null),
            ("L-018", "C-008", "R-004", 45, 40),
            ("L-019", "C-002", "R-006", 5, null)
        ];

        foreach (var row in rows)
        {
            var copy = copies[row.Copy];
            var loan = Loan.Open(row.Id, copy, row.Reader, today.AddDays(-row.IssuedAgo));
            if (row.ReturnedAgo is int back)
                loan.Close(today.AddDays(-back), copy);
            loans.Add(loan);
        }
        return loans;
    }

    // ЛАБ 7, додаткове 4: детермінований генератор великої кількості видач (seed = 42)
    public static IReadOnlyList<Loan> GenerateLoans(int count, DateTime today, int seed = 42)
    {
        var rnd = new Random(seed);
        var result = new List<Loan>(count);
        for (int i = 1; i <= count; i++)
        {
            var copy = BookCopy.Create($"G-{i:D6}", "978-000-00-0000-0");
            var loan = Loan.Open($"GL-{i:D6}", copy, $"R-{rnd.Next(1, 1001):D4}", today.AddDays(-rnd.Next(0, 60)));
            if (rnd.Next(0, 3) > 0)
                loan.Close(loan.IssuedOn.AddDays(rnd.Next(0, 10)), copy);
            result.Add(loan);
        }
        return result;
    }
}