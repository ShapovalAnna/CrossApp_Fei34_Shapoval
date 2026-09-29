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
    │   ├── Core.csproj          (TargetFrameworks: net8.0;net10.0)
    │   └── EnvironmentInfo.cs   (EnvironmentReport + EnvironmentInfo, namespace Core)
    └── Cli/
        ├── Cli.csproj           (ProjectReference на Core)
        └── Program.cs           (лише виклик Core і форматування виводу)
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

## Запуск

```
chcp 65001
dotnet build
dotnet run --project src/Cli
dotnet run --project src/Cli -- --json
```
## Перевірка структури (самоперевірка)

```
dotnet sln list
type src\Cli\Cli.csproj
dotnet build src/Core/Core.csproj
dir src\Core\bin\Debug
```

`dotnet sln list` показує два проєкти (Cli і Core), `Cli.csproj` містить
ProjectReference на Core, Core збирається окремо, а в `bin\Debug`
є два підкаталоги — `net8.0` і `net10.0` (multi-targeting).

## Команди, якими додано Core і посилання

```bash
dotnet new classlib -n Core -o src/Core -f net8.0
dotnet sln add src/Core/Core.csproj
dotnet add src/Cli/Cli.csproj reference src/Core/Core.csproj
```

Перевірка: `src/Cli/Cli.csproj` містить
`<ProjectReference Include="..\Core\Core.csproj" />`.

## Публікація

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true  -o publish/self-contained
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o publish/framework-dependent
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true -o publish/linux-x64
```

Додатково опубліковано (опційне завдання):

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/single-file
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true -o publish/trimmed
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true -o publish/trimmed-single-file
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

Бібліотека `Core` збирається під дві цільові платформи:

    <TargetFrameworks>net8.0;net10.0</TargetFrameworks>

Після `dotnet build` у `src/Core/bin/Debug/` з'являються два підкаталоги:
`net8.0` і `net10.0` — проєкт компілюється окремо для кожного TFM.

`Cli` має один TFM (`net8.0`) і під час збірки автоматично використовує
відповідну збірку `Core` для net8.0. Тому команди `dotnet publish`
не потребують параметра `-f`.

Встановлені SDK (`dotnet --list-sdks`): 8.0.x, 10.0.x

## Середовище

.NET SDK 8.0, Windows 11 x64
