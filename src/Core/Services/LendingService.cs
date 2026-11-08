using Core.Abstractions;
using Core.Domain;
using Core.Events; // ЛАБ 6: типи події (LendingChangeKind, LendingChangedEventArgs)
//бізнес-операції
namespace Core.Services;
//конструктор
public sealed class LendingService(IBookCopyStore store) //Dependency Inversion (DIP) (LendingService і обидва сховища залежать від IBookCopyStore)
{
    //забороняє передавати налл
    private readonly IBookCopyStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // ЛАБ 6: подія. Сервіс лише повідомляє «щось сталося», а що з цим робити, вирішують підписники (Cli, пізніше Api і Blazor).
    // Слово event: ззовні можна тільки += і -=, викликати подію може лише цей клас.
    public event EventHandler<LendingChangedEventArgs>? Changed;

    // ЛАБ 6: єдине місце, де піднімається подія.
    // ?. означає: якщо підписників немає (Changed == null), нічого не робимо.
    private void OnChanged(string copyId, LendingChangeKind kind,
                           string? readerId = null, DateOnly? dueDate = null) =>
        Changed?.Invoke(this, new LendingChangedEventArgs(copyId, kind, readerId, dueDate));

//AddBook(isbn). Генерує короткий id. створює примірник через BookCopy.Create (там перевіряється ISBN) і додає в сховище.
    public BookCopy AddBook(string isbn)
    {
        var copy = BookCopy.Create(Guid.NewGuid().ToString("N")[..8], isbn);
        _store.Add(copy);
        OnChanged(copy.Id, LendingChangeKind.BookAdded); // ЛАБ 6: після успішного Add, не до
        return copy;
    }
//IssueCopy(id) і ReturnCopy(id). Знаходять примірник (?? throw, якщо його немає),
// викликають метод сутності (Issue() чи Return(), правила перевіряє сама сутність)
// зберігають через Update.
    // ЛАБ 6: readerId і dueDate необов'язкові, щоб старі виклики з лаби 5 працювали без змін
    public void IssueCopy(string id, string? readerId = null, DateOnly? dueDate = null)
    {
        var copy = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        copy.Issue();       // якщо інваріант кине виняток, до події не дійде
        _store.Update(copy);
        OnChanged(copy.Id, LendingChangeKind.CopyIssued, readerId, dueDate); // ЛАБ 6: після успішної зміни
    }

    public void ReturnCopy(string id)
    {
        var copy = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        copy.Return();
        _store.Update(copy);
        OnChanged(copy.Id, LendingChangeKind.CopyReturned); // ЛАБ 6: після успішної зміни
    }
//All() і Find(id). передають виклик сховищу.
    public IReadOnlyList<BookCopy> All() => _store.List();
    public BookCopy? Find(string id) => _store.GetById(id);

    //лаб5 друге додаткове Пошук з параметром
    //Бере List() і відфільтровує за умовою, яку передано лямбдою.
    // ЛАБ 6 (крок 7): це і є «Func замість події», бо тут потрібно повернути значення
    public IReadOnlyList<BookCopy> Search(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }
}