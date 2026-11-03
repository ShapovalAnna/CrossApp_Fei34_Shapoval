using Core.Abstractions;
using Core.Domain;
//бізнес-операції
namespace Core.Services;
//конструктор
public sealed class LendingService(IBookCopyStore store) //Dependency Inversion (DIP) (LendingService і обидва сховища залежать від IBookCopyStore)
{
    //забороняє передавати налл
    private readonly IBookCopyStore _store = store ?? throw new ArgumentNullException(nameof(store));
//AddBook(isbn). Генерує короткий id. створює примірник через BookCopy.Create (там перевіряється ISBN) і додає в сховище.
    public BookCopy AddBook(string isbn)
    {
        var copy = BookCopy.Create(Guid.NewGuid().ToString("N")[..8], isbn);
        _store.Add(copy);
        return copy;
    }
//IssueCopy(id) і ReturnCopy(id). Знаходять примірник (?? throw, якщо його немає),
// викликають метод сутності (Issue() чи Return(), правила перевіряє сама сутність)
// зберігають через Update.
    public void IssueCopy(string id)
    {
        var copy = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        copy.Issue();
        _store.Update(copy);
    }

    public void ReturnCopy(string id)
    {
        var copy = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        copy.Return();
        _store.Update(copy);
    }
//All() і Find(id). передають виклик сховищу.
    public IReadOnlyList<BookCopy> All() => _store.List();
    public BookCopy? Find(string id) => _store.GetById(id);

    //лаб5 друге додаткове Пошук з параметром
    //Бере List() і відфільтровує за умовою, яку передано лямбдою.
        public IReadOnlyList<BookCopy> Search(Func<BookCopy, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }
}