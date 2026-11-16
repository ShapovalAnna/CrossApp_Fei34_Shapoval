namespace Core.Reports;

// ЛАБ 7, додаткове 2: власний метод-розширення "топ-N за числовим ключем".
// Другий ключ thenBy обов'язковий, тому порядок однаковий за кожного запуску.
public static class EnumerableExtensions
{
    public static IEnumerable<T> TopBy<T>(this IEnumerable<T> source, Func<T, int> key, int n, Func<T, string> thenBy) =>
        source.OrderByDescending(key)
              .ThenBy(thenBy, StringComparer.Ordinal)
              .Take(n);
}