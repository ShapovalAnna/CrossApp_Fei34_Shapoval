//лаб5 додаткове3  Фабрика StoreFactory.Create(args)
using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;

public static class StoreFactory
{
    public static IBookCopyStore Create(string[] args)
    {
        if (args.Contains("--file"))
        {
            string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");
            return new FileBookCopyStore(dataPath);
        }

        return new InMemoryBookCopyStore(SampleData.BookCopies());
    }
}