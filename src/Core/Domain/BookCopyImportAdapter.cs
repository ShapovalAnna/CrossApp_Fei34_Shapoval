using Core.Dto;

namespace Core.Domain;

// Місток між тижнем 3 (ImportResult<BookDto>) і доменом.
// Кожен успішно розібраний DTO намагається стати доменною сутністю;
// якщо порушено доменний інваріант — рядок іде в список помилок, а не валить усю операцію.
public static class BookCopyImportAdapter
{
    public static DomainImportResult ToDomain(ImportResult<BookDto> importResult)
    {
        var copies = new List<BookCopy>();
        var errors = new List<string>();

        foreach (BookDto dto in importResult.Items)
        {
            try
            {
                copies.Add(BookCopy.Create(dto.Id, dto.Isbn));
            }
            catch (ArgumentException ex)
            {
                errors.Add($"{dto.Id}: {ex.Message}");
            }
        }

        return new DomainImportResult(copies, errors);
    }
}