namespace Core.Reports;

// ЛАБ 7: DTO звітів. record = незмінні й зручні для порівняння в тестах (тиждень 8).
// Звіт повертає ці типи, а не доменні сутності, тому контракт звіту не залежить від моделі.

// рядок звіту "топ читачів"
public sealed record ReaderLoanLine(string ReaderId, int Loans, int OpenLoans);

// рядок звіту "прострочені видачі"
public sealed record OverdueLine(string LoanId, string CopyId, string ReaderId, DateTime DueOn, int DaysOverdue);

// підсумок групи (десятиліття): скільки видань і скільки примірників
public sealed record GroupSummary(string Key, int Count, int TotalQuantity);

// ЛАБ 7, додаткове 1: зведений рядок після Join (видача + примірник + видання)
public sealed record LoanLine(string LoanId, string Title, string ReaderId, DateTime IssuedOn, bool IsOpen);

// пара "назва видання, кількість" (GroupJoin, топ видань)
public sealed record TitleCount(string Title, int Count);