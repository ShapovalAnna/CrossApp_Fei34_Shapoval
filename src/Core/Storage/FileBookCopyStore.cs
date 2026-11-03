using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
//файлове сховище 
namespace Core.Storage;

public sealed class FileBookCopyStore(string path) : IBookCopyStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly Dictionary<string, BookCopy> _cache = new(StringComparer.OrdinalIgnoreCase); //кеш - робочий словник у пам'яті
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded() //Якщо файл є, читає JSON і перетворює кожен BookCopyDto на BookCopy через FromDto
    {
        if (_loaded) return;
        if (File.Exists(_path))
        {
            var dtos = JsonSerializer.Deserialize<List<BookCopyDto>>(File.ReadAllText(_path)) ?? [];
            foreach (var dto in dtos) { var c = BookCopy.FromDto(dto); _cache[c.Id] = c; }
        }
        _loaded = true;
    }

    private void Flush() //створює папку data, якщо її немає, і повністю перезаписує файл (WriteAllText) JSON-масивом BookCopyDto з відступами.
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path,
            JsonSerializer.Serialize(_cache.Values.Select(c => c.ToDto()).ToList(), Options));
    }

//List і GetById читають лише кеш, файл вони не відкривають
    public IReadOnlyList<BookCopy> List() { EnsureLoaded(); return _cache.Values.ToList(); }
    public BookCopy? GetById(string id) { EnsureLoaded(); return _cache.GetValueOrDefault(id); }
    
//Add, Update, Remove змінюють кеш і одразу викликають Flush(), тому дані потрапляють на диск після кожної зміни.
    public void Add(BookCopy item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();
        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Примірник з id={item.Id} уже існує.");
        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(BookCopy item) { EnsureLoaded(); _cache[item.Id] = item; Flush(); }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;
        Flush();
        return true;
    }
}