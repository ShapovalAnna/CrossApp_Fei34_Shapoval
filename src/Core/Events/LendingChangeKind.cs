namespace Core.Events;

// ЛАБ 6: вид зміни стану. Це «що сталося» у домені Бібліотека.
public enum LendingChangeKind
{
    BookAdded,     // у сховище додано новий примірник
    CopyIssued,    // примірник видано читачу
    CopyReturned   // примірник повернено
}