# CrossApp

Наскрізний проєкт з крос-платформного програмування. Студентка: Шаповал Анна, група ФЕІ-34.

## Предметна область

**Назва:** Бібліотека.
**Сутності:** `Book` (видання), `BookCopy` (примірник), `Reader` (читач), `Loan` (видача).
**Призначення:** ведення обліку видачі примірників книг читачам та контроль повернень.

## Структура solution

```
CrossApp/
├── CrossApp.sln
├── README.md
├── .gitignore
└── src/
    ├── Core/
    │   ├── Core.csproj              (TargetFrameworks: net8.0;net10.0)
    │   ├── EnvironmentInfo.cs       (EnvironmentReport + EnvironmentInfo, namespace Core)
    │   └── Dto/
    │       └── EnvironmentReportDto.cs  (DTO для JSON-виводу, namespace Core.Dto)
    └── Cli/
        ├── Cli.csproj               (ProjectReference на Core; TargetFrameworks: net8.0;net10.0)
        └── Program.cs               (лише виклик Core і форматування виводу)
```

Core — class library, не має точки входу й не запускається самостійно.
Залежність одностороння: **Cli → Core**. Core не посилається на Cli (це
спричинило б циклічну залежність), тому його без змін можна буде підключити
до майбутніх проєктів Api (тиждень 10) та Blazor-клієнта (тиждень 12).

### Домовленість про каталоги в Core на весь семестр

| Каталог | Що там буде | Коли |
|---|---|---|
| `Core/Dto/` | record-типи формату даних: BookDto, ReaderDto, LoanDto | тиждень 3 |
| `Core/Domain/` | сутності з поведінкою та інваріантами: Book, BookCopy, Reader, Loan | тиждень 4 |
| `Core/Storage/` | реалізації сховищ | тиждень 5 |

Правило: у `Program.cs` немає бізнес-логіки. Якщо рядок можна повторно
використати в Api або Blazor, він належить Core. Цього тижня Core ще не
містить доменної логіки — лише допоміжний код для отримання інформації
про середовище виконання.

## Перевірка структури (самоперевірка)

```
dotnet --list-sdks
dotnet --info
dotnet sln list
type src\Cli\Cli.csproj
dotnet build src/Core/Core.csproj
dir src\Core\bin\Debug
```

`dotnet --list-sdks` показує встановлені SDK, `dotnet --info` — RID машини.
`dotnet sln list` показує два проєкти (Cli і Core), `Cli.csproj` містить
ProjectReference на Core, Core збирається окремо, а в `bin\Debug`
є два підкаталоги — `net8.0` і `net10.0`.

## Запуск

Оскільки Cli має два TFM, для `dotnet run` потрібно вказати `-f`:

```
chcp 65001
dotnet build
dotnet run --project src/Cli -f net8.0
dotnet run --project src/Cli -f net10.0
dotnet run --project src/Cli -f net8.0 -- --json
```

## Формат вхідного файлу (data/sample.csv) lab3

Роздільник колонок — `;`; перший рядок — заголовок (`id;isbn;title;year`),
кодування — UTF-8. Биті рядки (замало колонок, порожній ISBN/назва,
некоректний рік) не перуривають імпорт — вони пропускаються й виводяться
окремим списком з номером рядка.
### Перевірка сценаріїв lab3

```powershell
# Коректні + пошкоджені рядки одночасно
dotnet run --project src/Cli -- data\sample.csv

# Неіснуючий файл
dotnet run --project src/Cli -- data\немає-такого-файлу.csv
echo $LASTEXITCODE
```
### Додаткове завдання (опційно)

- `BookJsonImporter` — другий імпортер на тих самих типах (`data/sample.json`).
  Вибір імпортера за розширенням файлу (`.csv` / `.json`) — у `Program.cs`.
- `MixedCsvImporter` розпізнає рядки за префіксом (`B;...` — книга,
  `R;...` — читач) в одному файлі `data/sample-mixed.csv`.
- Статистика імпорту (усього / прийнято / пропущено / % помилок) одним
  рядком у виводі Cli.
  
  ## Лабораторна робота 4 — доменна модель (BookCopy, Loan)

### Інваріанти

| Сутність / метод | Інваріант | Виняток при порушенні |
|---|---|---|
| `BookCopy.Create(id, isbn)` | `Id` та `Isbn` не можуть бути порожніми | `ArgumentException` |
| `BookCopy.Issue()` | не можна видати примірник, який вже видано | `InvalidOperationException` |
| `BookCopy.Return()` | не можна повернути примірник, який не видано | `InvalidOperationException` |
| `Loan.Open(...)` | примірник на момент видачі має бути вільним (делегує перевірку в `BookCopy.Issue()`) | `InvalidOperationException` |
| `Loan.Close(returnedOn, copy)` | дата повернення не може бути раніше дати видачі | `ArgumentException` |
| `Loan.Close(...)` → `EnsureTransition` | дозволений лише перехід статусу `Open → Closed`; `Closed → Closed` заборонено | `InvalidOperationException` |
| `LoanLimitPolicy.OpenWithLimit(...)` | в одного читача не може бути більше **5** активних (відкритих) видач одночасно | `InvalidOperationException` |

### Команди для запуску
```bash
# лаб. 4 (сценарій успіху + 4 порушення інваріантів)
dotnet run --project src/Cli -- data\sample.csv
# Те саме, але з JSON-файлом 
dotnet run --project src/Cli -- data\sample.json
# Змішаний файл (демонструє розпізнавання за префіксом книги/читача)
dotnet run --project src/Cli -- data\sample-mixed.csv

## Команди, якими додано Core і посилання

```bash
dotnet new classlib -n Core -o src/Core -f net8.0
dotnet sln add src/Core/Core.csproj
dotnet add src/Cli/Cli.csproj reference src/Core/Core.csproj
```

Перевірка: `src/Cli/Cli.csproj` містить
`<ProjectReference Include="..\Core\Core.csproj" />`.

## Публікація

Через multi-targeting у кожній команді publish вказано `-f net8.0`,
а `-o` задає каталог результату:

```bash
dotnet publish src/Cli -c Release -f net8.0 -r win-x64 --self-contained true  -o publish/self-contained
dotnet publish src/Cli -c Release -f net8.0 -r win-x64 --self-contained false -o publish/framework-dependent
dotnet publish src/Cli -c Release -f net8.0 -r linux-x64 --self-contained true -o publish/linux-x64
```

Додатково опубліковано:

```bash
dotnet publish src/Cli -c Release -f net8.0 -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/single-file
dotnet publish src/Cli -c Release -f net8.0 -r win-x64 --self-contained true -p:PublishTrimmed=true -o publish/trimmed
dotnet publish src/Cli -c Release -f net8.0 -r win-x64 --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true -o publish/trimmed-single-file
```

Запуск саме з каталогу publish (не через `dotnet run`):

```powershell
.\publish\self-contained\Cli.exe
.\publish\framework-dependent\Cli.exe
```

Розмір каталогу publish:

```powershell
(Get-ChildItem -Recurse <шлях publish> | Measure-Object Length -Sum).Sum/1MB
```

```bash
du -sh <шлях publish>
```
## Лабораторна 5: сервісний шар, два сховища, ручний DI

### Домен і контракт
Головна сутність — `BookCopy` (примірник книги). Контракт `IBookCopyStore` (`Core/Abstractions`) має 5 методів:
- `List()` повертає `IReadOnlyList`, щоб зовнішній код не міняв сховище в обхід `Add/Update/Remove`;
- `GetById(id)` повертає `BookCopy?`, бо запису може не бути;
- `Add(item)`, `Update(item)`, `Remove(id)`.

В інтерфейсі немає нічого «про файл» (`Save`, `Flush`): контракт містить лише операції, потрібні сервісу.
Бізнес-операції у `LendingService`: `AddBook`, `IssueCopy`, `ReturnCopy`. Інваріанти перевіряє сама сутність
(`BookCopy.Create/Issue/Return`), сховище лише гарантує унікальність id.

### Дві реалізації
| | InMemoryBookCopyStore | FileBookCopyStore |
|---|---|---|
| Де дані | `Dictionary` у пам'яті | кеш у пам'яті + JSON-файл |
| Шлях | — | `data/catalog.json` біля виконуваного файлу (`bin/Debug/net8.0/data/`) |
| Формат | — | JSON-масив `BookCopyDto` з відступами |
| Життя даних | зникають після виходу | зберігаються між запусками |
| Механіка | початкові дані з `SampleData` | дозавантаження при першому зверненні, запис на диск після кожної зміни (`Flush`) |

На диск іде DTO (`BookCopyDto`), а мапінг робить `BookCopy.ToDto()/FromDto()`: формат файлу не диктує форму домену.

### Схема залежностей
`Cli → LendingService → IBookCopyStore → InMemoryBookCopyStore | FileBookCopyStore`

### Запуск і вивід
```
dotnet run --project src/Cli            # Сховище: InMemoryBookCopyStore
dotnet run --project src/Cli -- --file  # Сховище: FileBookCopyStore
```
## Лабораторна 6: доменні події, делегати Action/Func, лямбди

### Що змінено
Сервіс `LendingService` тепер сам повідомляє про зміни стану подією, а що з цим робити, вирішує зовнішній код.
У `Core` немає ні `Console`, ні роботи з файлами логів.

| Файл | Що там |
|---|---|
| `Core/Events/LendingChangeKind.cs` | `enum`: `BookAdded`, `CopyIssued`, `CopyReturned` |
| `Core/Events/LendingChangedEventArgs.cs` | дані події: `CopyId`, `Kind`, `ReaderId`, `DueDate`, `At` |
| `Core/Services/LendingService.cs` | `event EventHandler<LendingChangedEventArgs>? Changed` і приватний `OnChanged` |
| `Cli/Program.cs` | підписка, відписка, лямбда-лічильник, лог у `logs/app.log` |

### Які зміни стану стали подіями
- `AddBook` → `BookAdded`;
- `IssueCopy` → `CopyIssued` (з `ReaderId` і `DueDate`);
- `ReturnCopy` → `CopyReturned`.

Подія піднімається лише після успішної зміни (`_store.Add/Update`). Якщо сутність кидає виняток через порушений
інваріант, повідомлення немає.

### Підписники
1. `OnLendingChanged` — локальний метод у `Program.cs`: друкує рядок у консоль і дописує його у `logs/app.log`.
   Це іменований метод, щоб можна було відписатися через `-=`.
2. Лямбда-лічильник `(_, _) => changes++` із замиканням над локальною змінною `changes`.

На тижні 10 підписником стане логер проєкту Api, на тижні 12 — компонент Blazor, який оновлює таблицю.
Змін у `Core` для цього не потрібно.

### Func замість події
`LendingService.Search(Func<BookCopy, bool> predicate)` використовується там, де треба отримати значення назад
(список), а подія призначена лише для сповіщення «щось сталося».

### Запуск
```
dotnet run --project src/Cli
```
Лог-файл створюється кодом через `Path.Combine(AppContext.BaseDirectory, "logs", "app.log")`
(`src/Cli/bin/Debug/net8.0/logs/app.log`).

Фрагмент виводу:
```
18:16:40 BookAdded    19549ae0
18:16:40 CopyIssued   19549ae0 видано читачу R-007 до 22.11.2026
Подій отримано: 2
  Помилка: Немає запису з id=NOPE.     (події немає, лічильник не змінився)
Подій отримано: 2
Подій отримано: 3                      (після -= консоль мовчить, +1 лише від лямбди)
```
У `logs/app.log` лише перші два рядки; операція після відписки в лог не потрапляє.

### Чому подія, а не прямий виклик логера
1. `Core` не залежить від консолі й файлової системи, тому його можна використати в Api і Blazor без змін.
2. Підписників можна додавати і прибирати, не змінюючи `Core`; тест може підписатися на подію і перевірити, що
   повідомлення сформовано, не читаючи консоль чи файл.

### Додаткові завдання
1. `INotificationSink` (`Core/Abstractions`) з реалізаціями `ConsoleSink` і `FileSink` (`Cli`); Cli передає канал
   сервісу через конструктор (ручний DI). Порівняння з подією: подія для багатьох незалежних слухачів,
   інтерфейс для одного явного каналу за контрактом.
2. `BookCsvImporter.Load(path, Action<string>? onSkipped)`: імпортер повідомляє викликача про кожен пропущений рядок.
3. `async`-обробник події це `async void`: виклик не може дочекатись завершення, а виняток після `await` не
   перехоплюється `try/catch` навколо `Invoke` і може завершити процес (детально на тижні 8).
   
### Без DI-контейнера
`Microsoft.Extensions.DependencyInjection` не підключено: залежності передаються вручну через конструктор у `Program.cs`.
Контейнер з'явиться на тижні 10 разом з Api.

## Лабораторна 7: звіти через LINQ

Звіти лежать у `Core/Reports/ReportService.cs`: вони приймають `IEnumerable<T>` і повертають `IReadOnlyList<DTO>`
(DTO описані в `Core/Reports/ReportDtos.cs`). `Cli` лише викликає звіти й друкує результат (`Cli/ReportCommands.cs`),
власних `Where`/`GroupBy` там немає. Без підкоманди `report-...` програма працює, як у лабораторних 3-6.

### Підкоманди

| Підкоманда | Що показує | Основні оператори |
|---|---|---|
| `report-top` | топ-5 читачів за кількістю видач | `GroupBy`, `Select`, `OrderByDescending`, `ThenBy`, `Take` |
| `report-overdue` | прострочені видачі (термін 14 днів) | `Where`, `OrderBy`, `ThenBy`, `Select` |
| `report-groups` | видання за десятиліттями | `GroupBy`, `Count`, `Sum` |
| `report-sum` | кількість видань, примірників, середнє, максимум | `Count`, `Sum`, `Average`, `Max` |
| `report-defer` | демонстрація відкладеного виконання | `Where`, `Count()`, `ToList()` |
| `report-join` | видачі з назвами видань | `Join` |
| `report-copies` | примірників на видання (включно з 0) | `GroupJoin` |
| `report-topbooks` | топ-3 видань за примірниками і за видачами | власний `TopBy` |
| `report-query` | той самий звіт про десятиліття в синтаксисі запитів | `from ... group ... into ... let ... orderby` |
| `report-bench` | час і пам'ять на 100 000 видач | `Stopwatch` |

Запуск: `dotnet run --project src/Cli -- report-top`.

### Стабільний порядок
Усі топи й сортування мають другий ключ (`ThenBy` за ідентифікатором або назвою), щоб за однакового значення першого
ключа порядок рядків не змінювався між запусками. Наприклад, у `report-top` читачі R-003 і R-004 мають по 3 видачі,
і їх розрізняє `ThenBy(ReaderId)`.

### Відкладене виконання (`report-defer`)
Запит `source.Where(c => !c.IsIssued)` лише описує фільтр і нічого не виконує. Він виконується щоразу, коли його
перебирають: під час першого `Count()` і під час другого. Між ними в `source` додано примірник, тому перший `Count()`
дав 18, а другий 19. `ToList()` виконує запит один раз і зберігає результат, тому знімок лишився 19 навіть після
`source.Clear()`, а сам запит після очищення дав 0. `Count` у списку це властивість із вже збереженим числом, а
`query.Count()` це метод, що проходить послідовність заново.

### Порожнє джерело
`Totals` перевіряє порожню послідовність до виклику `Average` і `Max` (вони на порожній послідовності кидають
`InvalidOperationException`, а `Sum` повертає 0). `PrintLines` для порожнього звіту друкує «(порожньо)».

### Додаткові завдання
1. **`Join` і `GroupJoin`.** `LoanLines` з'єднує видачі, примірники й видання (`report-join`). `CopiesPerBook`
   (`report-copies`) використовує `GroupJoin`, тому видання з 0 примірників лишаються у звіті, а `Join` їх би відкинув.
2. **Власний метод-розширення** `TopBy<T>(this IEnumerable<T>, Func<T,int>, int, Func<T,string>)` з обов'язковим
   другим ключем. Використано у двох звітах: `TopBooksByCopies` і `TopBooksByLoans` (`report-topbooks`).
3. **Синтаксис запитів.** `ByDecadeQuery` написано через `from ... group ... into ... let ... orderby ... select`
   (`report-query`). Результат збігається з методним `ByDecade` (перевірка `SequenceEqual` дає `True`). Запити зручніші,
   коли потрібні проміжна змінна (`let`) і кілька `orderby`; у простих ланцюжках `Where/Select/Take` методний синтаксис
   коротший.
4. **Stopwatch на 100 000 згенерованих видач** (`report-bench`, 20 запусків після прогріву, `seed = 42`):

   | Варіант | Час на запуск | Пам'ять на запуск |
   |---|---|---|
   | без проміжних `ToList` | 25,56 мс | 2 357 КБ |
   | `ToList` у кожному кроці | 23,96 мс | 3 153 КБ |

   Час практично однаковий (різниця в межах похибки вимірювання, бо основну роботу, `GroupBy` по 100 000 записів,
   виконують обидва варіанти). Зайві `ToList` збільшують використання пам'яті приблизно на 800 КБ: це копія списку зі
   100 000 посилань (100 000 × 8 байт). Висновок: проміжні `ToList` швидкості не додають, а лише витрачають пам'ять,
   тому матеріалізувати результат варто один раз, у кінці ланцюжка.


### Перевірка linux-x64 публікації в контейнері без .NET (опційне завдання)

```powershell
docker run --rm -it -v "${PWD}\publish\linux-x64:/app" mcr.microsoft.com/dotnet/runtime-deps:8.0 bash
cd /app
chmod +x ./Cli
./Cli
```

## Порівняння режимів публікації

| RID | Режим | Розмір publish | Потрібен runtime |
|---|---|---|---|
| win-x64 | self-contained | 72 МБ | ні |
| win-x64 | framework-dependent | 196 КБ | так (.NET 8) |
| win-x64 | self-contained + single-file | 65 МБ (1 exe) | ні |
| win-x64 | self-contained + trimmed | 19 МБ | ні |
| win-x64 | self-contained + trimmed + single-file | 12 МБ (1 exe) | ні |
| linux-x64 | self-contained | 71 МБ | ні |

Framework-dependent — у каталозі лише код застосунку та його залежності, тому він маленький, але на комп'ютері користувача має бути встановлений .NET Runtime потрібної версії.
Self-contained — у публікацію додається сам .NET Runtime, тому застосунок працює без встановленого .NET на цільовій машині, але каталог значно більший.

## Multi-targeting

Проєкти `Core` і `Cli` збираються під дві цільові платформи:

    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>

Після `dotnet build` у `bin/Debug/` кожного проєкту з'являються два
підкаталоги: `net8.0` і `net10.0` — проєкт компілюється окремо для кожного TFM.

У `EnvironmentInfo` є константа `BuildNote`, яка задається директивою
умовної компіляції, тому вивід відрізняється залежно від TFM:

```csharp
#if NET10_0_OR_GREATER
    private const string BuildNote = "збірка під net10.0";
#else
    private const string BuildNote = "збірка під net8.0";
#endif
```

```
dotnet run --project src/Cli -f net8.0    → Збірка : збірка під net8.0
dotnet run --project src/Cli -f net10.0   → Збірка : збірка під net10.0
```

Через кілька TFM у Cli команди `dotnet run` і `dotnet publish` потребують
параметра `-f` (інакше помилка «You must specify a target framework»).

Встановлені SDK (`dotnet --list-sdks`):

```
8.0.425 [C:\Program Files\dotnet\sdk]
10.0.401 [C:\Program Files\dotnet\sdk]
```

## Середовище

.NET SDK 8.0.425 та 10.0.401, Windows 11 x64