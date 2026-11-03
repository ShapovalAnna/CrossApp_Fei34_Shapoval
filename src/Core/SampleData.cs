using Core.Domain;
//початкові дані
namespace Core;

public static class SampleData
{
    public static IEnumerable<BookCopy> BookCopies() =>
    [
        BookCopy.Create("C-001", "978-966-03-5548-1"),
        BookCopy.Create("C-002", "978-617-7764-11-3"),
        BookCopy.Create("C-003", "978-966-97925-0-6")
    ];
}