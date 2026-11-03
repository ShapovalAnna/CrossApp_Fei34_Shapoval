using Core.Abstractions;
using Core.Domain;
//після завершення програми все зникає 
namespace Core.Storage;

public sealed class InMemoryBookCopyStore(IEnumerable<BookCopy>? seed = null) : IBookCopyStore
{
    //конструктор
    private readonly Dictionary<string, BookCopy> _items =
        (seed ?? []).ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
//методи
    public IReadOnlyList<BookCopy> List() => _items.Values.ToList(); //щоб не можна було ззовні змінити словник
    public BookCopy? GetById(string id) => _items.GetValueOrDefault(id); //повертає налл якщо ід нема
    public void Update(BookCopy item) => _items[item.Id] = item;//перезаписує запис за ід
    public bool Remove(string id) => _items.Remove(id); //повертає тру чи фолз

    public void Add(BookCopy item) //перевіряє га налл та  дублювання ід 
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Примірник з id={item.Id} уже існує.");
        _items.Add(item.Id, item);
    }
}