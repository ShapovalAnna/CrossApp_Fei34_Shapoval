using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class BookJsonImporter
{
    public static ImportResult<BookDto> Load(string path)
    {
        string json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        try
        {
            var items = JsonSerializer.Deserialize<List<BookDto>>(json, options) ?? [];
            return new ImportResult<BookDto>(items, Array.Empty<string>());
        }
        catch (JsonException ex)
        {
            return new ImportResult<BookDto>(Array.Empty<BookDto>(), [$"помилка розбору JSON: {ex.Message}"]);
        }
    }
}