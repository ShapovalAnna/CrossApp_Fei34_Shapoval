namespace Core.Dto;

public record LoanDto(
    string Id,
    string BookCopyId,
    string ReaderId,
    DateTime IssuedOn,
    DateTime? ReturnedOn);