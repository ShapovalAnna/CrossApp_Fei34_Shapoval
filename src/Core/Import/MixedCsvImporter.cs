using Core.Dto;

namespace Core.Import;

public sealed record MixedImportResult(
    IReadOnlyList<BookDto> Books,
    IReadOnlyList<ReaderDto> Readers,
    IReadOnlyList<string> Errors);

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var books = new List<BookDto>();
        var readers = new List<ReaderDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case BookRow row:
                    books.Add(row.Value);
                    break;
                case ReaderRow row:
                    readers.Add(row.Value);
                    break;
                case RowFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MixedImportResult(books, readers, errors);
    }

    private static RowOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["B", var id, var isbn, var title, var year]
                when int.TryParse(year, out int y) && y >= 1450 && y <= DateTime.Now.Year
                => new BookRow(new BookDto(id, isbn, title, y)),
            ["B", ..]
                => new RowFailed("книга: неправильна кількість полів або рік поза межами"),
            ["R", var id, var fullName] when !string.IsNullOrWhiteSpace(fullName)
                => new ReaderRow(new ReaderDto(id, fullName)),
            ["R", ..]
                => new RowFailed("читач: неправильна кількість полів або порожнє ім'я"),
            [var prefix, ..]
                => new RowFailed($"невідомий префікс рядка: '{prefix}'"),
            _ => new RowFailed("порожній рядок")
        };
    }

    private abstract record RowOutcome;
    private sealed record BookRow(BookDto Value) : RowOutcome;
    private sealed record ReaderRow(ReaderDto Value) : RowOutcome;
    private sealed record RowFailed(string Reason) : RowOutcome;
}