//лаб5 додаткове3  Фабрика StoreFactory.Create(args)
//вибір сховища
//Файл лежить у Cli, щоб Core не знав конкретних сховищ.
using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;
//Composition root
public static class StoreFactory
{
    public static IBookCopyStore Create(string[] args)
    {
        string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
    return args.Contains("--file")
        ? new FileBookCopyStore(dataPath)
        : new InMemoryBookCopyStore(SampleData.BookCopies());
    }
}