using Core.Domain;
//контракт
//Описує, що вміє будь-яке сховище примірників, але нічого не реалізує.
//Сервіс знає тільки цей інтерфейс, тому сховище можна підмінити, не чіпаючи сервіс.
namespace Core.Abstractions;

public interface IBookCopyStore
{
    IReadOnlyList<BookCopy> List();
    BookCopy? GetById(string id);
    void Add(BookCopy item);
    void Update(BookCopy item);
    bool Remove(string id);
}
