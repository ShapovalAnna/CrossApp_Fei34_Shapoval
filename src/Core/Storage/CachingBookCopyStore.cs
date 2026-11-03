
//Лаб 5 перше додаткове (Декоратор)
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;
//конструктор який  всередині тримає інше сховище (_inner)
public sealed class CachingBookCopyStore(IBookCopyStore inner) : IBookCopyStore
{
    private readonly IBookCopyStore _inner = inner ?? throw new ArgumentNullException(nameof(inner)); //не дозволяє створити декоратор без справжнього сховища.
    private readonly Dictionary<string, BookCopy?> _byId = new(StringComparer.OrdinalIgnoreCase); 
    private IReadOnlyList<BookCopy>? _list;

    public int Hits { get; private set; }
    public int Misses { get; private set; }

    public IReadOnlyList<BookCopy> List() //дивиться у власний кеш
    { //Знайшли, то це влучання (Hits++), і внутрішнє сховище не чіпається. Не знайшли, то промах (Misses++): питається _inner і запам'ятовується відповідь.
        if (_list is not null) { Hits++; return _list; }
        Misses++;
        return _list = _inner.List();
    }

    public BookCopy? GetById(string id)
    {
        if (_byId.TryGetValue(id, out var cached)) { Hits++; return cached; }
        Misses++;
        var found = _inner.GetById(id);
        _byId[id] = found;
        return found;
    }

    public void Add(BookCopy item) { _inner.Add(item); Invalidate(); }
    public void Update(BookCopy item) { _inner.Update(item); Invalidate(); }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed) Invalidate();
        return removed;
    }

    private void Invalidate() { _byId.Clear(); _list = null; }
}