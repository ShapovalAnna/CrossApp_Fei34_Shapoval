using Core.Dto;

namespace Core.Domain;

public enum LoanStatus
{
    Open,
    Closed
}

public sealed class Loan
{
    public string Id { get; }
    public string BookCopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }

    public LoanStatus Status => ReturnedOn is null ? LoanStatus.Open : LoanStatus.Closed;

    private Loan(string id, string bookCopyId, string readerId, DateTime issuedOn, DateTime? returnedOn)
    {
        Id = id;
        BookCopyId = bookCopyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
    }

    public static Loan Open(string id, BookCopy copy, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));
        if (copy is null)
            throw new ArgumentNullException(nameof(copy));
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        copy.Issue();

        return new Loan(id.Trim(), copy.Id, readerId.Trim(), issuedOn, returnedOn: null);
    }

    public void Close(DateTime returnedOn, BookCopy copy)
    {
        EnsureTransition(LoanStatus.Closed);

        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn,
                "Дата повернення не може бути раніше дати видачі");

        copy.Return();
        ReturnedOn = returnedOn;
    }

    // Допустимі переходи стану перевіряються одним switch-виразом із кортежним патерном.
    private void EnsureTransition(LoanStatus target)
    {
        bool allowed = (Status, target) switch
        {
            (LoanStatus.Open, LoanStatus.Closed) => true,
            (LoanStatus.Closed, LoanStatus.Closed) => false,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException(
                $"Видача {Id}: недопустимий перехід стану {Status} -> {target}");
    }

    public LoanDto ToDto() => new(Id, BookCopyId, ReaderId, IssuedOn, ReturnedOn);

    public static Loan FromDto(LoanDto dto) =>
        new(dto.Id, dto.BookCopyId, dto.ReaderId, dto.IssuedOn, dto.ReturnedOn);

    public override string ToString() =>
        $"{Id}: примірник {BookCopyId} → читач {ReaderId}, видано {IssuedOn:d}" +
        (ReturnedOn.HasValue ? $", повернено {ReturnedOn:d}" : ", не повернено");
}