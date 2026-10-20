namespace Core.Domain;

// Інваріант, що охоплює ДВІ сутності одразу: Loan + "усі видачі читача".
// Свідомо НЕ в Loan/BookCopy — жодна сутність не має доступу до списку всіх видач,
// лише сервіс/сховище (тиждень 5). Тут — тимчасова заглушка через переданий список.
public static class LoanLimitPolicy
{
    private const int MaxOpenLoansPerReader = 5;

    public static Loan OpenWithLimit(
        string id, BookCopy copy, string readerId, DateTime issuedOn,
        IReadOnlyCollection<Loan> existingLoans)
    {
        int openCount = existingLoans.Count(l => l.ReaderId == readerId && l.Status == LoanStatus.Open);

        if (openCount >= MaxOpenLoansPerReader)
            throw new InvalidOperationException(
                $"Читач {readerId} вже має {openCount} відкритих видач (ліміт {MaxOpenLoansPerReader}) — видача примірника {copy.Id} неможлива");

        return Loan.Open(id, copy, readerId, issuedOn);
    }
}