namespace Core.Domain;

public sealed record DomainImportResult(
    IReadOnlyList<BookCopy> Copies,
    IReadOnlyList<string> Errors);