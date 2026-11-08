namespace Core.Events;

// ЛАБ 6: дані події. Їх сервіс передає всім підписникам разом із повідомленням.
// Клас лише зберігає дані, нічого не друкує і не пише у файл.
public sealed class LendingChangedEventArgs(
    string copyId, LendingChangeKind kind, string? readerId, DateOnly? dueDate) : EventArgs
{
    public string CopyId { get; } = copyId;                 // який примірник змінився
    public LendingChangeKind Kind { get; } = kind;          // що саме сталося
    public string? ReaderId { get; } = readerId;            // кому видано (лише для CopyIssued)
    public DateOnly? DueDate { get; } = dueDate;            // до якої дати (лише для CopyIssued)
    public DateTimeOffset At { get; } = DateTimeOffset.Now; // коли сталося
}