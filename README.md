# 🔐 PassVault — Менеджер паролей

Веб‑приложение для удобного хранения и управления паролями от различных сайтов.
Все пароли собраны в одном месте, доступ через браузер, запуск одной командой через Docker.

## 🚀 Возможности

- 🔓 **Регистрация и вход** — собственный аккаунт через ASP.NET Identity
- 🔑 **Хранение паролей** — добавление, просмотр, редактирование, удаление
- 🔍 **Поиск и фильтрация** — по названию сайта, логину, URL и категории
- 📁 **Категории** — Email, Соцсети, Банки, Работа, Покупки и пользовательские
- 🏷️ **Теги (N:N)** — несколько тегов на пароль, фильтрация по тегам
- ⭐ **Избранное** — пометка часто используемых паролей
- 🎲 **Генератор паролей** — сильные пароли в один клик
- 👁 **Просмотр пароля** — скрытие/показ + копирование
- 📊 **Дашборд** — статистика по хранилищу
- 📱 **Адаптивный дизайн** — работает на любом устройстве

## 🛠 Технологический стек

| Слой | Технология |
| --- | --- |
| Backend | **.NET 8**, ASP.NET Core, C# |
| UI | **Blazor Server** + Razor компоненты |
| ORM | **Entity Framework Core** (Code‑First, миграции) |
| База данных | **PostgreSQL 16** (отдельный контейнер) |
| Аутентификация | ASP.NET Core Identity |
| Валидация | **FluentValidation** + DataAnnotations |
| Контейнеризация | **Docker** + Docker Compose |
| Тесты | xUnit + FluentAssertions + EF InMemory |

## 📦 Структура проекта

```
PasswordManager/
├── PasswordManager.Web/           # Основное Blazor Server приложение
│   ├── Components/                # Razor‑компоненты
│   │   ├── Account/Pages/         # Login, Register, Logout, Manage
│   │   ├── Layout/                # MainLayout, навигация
│   │   ├── Pages/                 # Home, PasswordList, PasswordForm, Favorites, TagsPage
│   │   └── Shared/                # Общие компоненты
│   ├── Data/                      # EF Core
│   │   ├── Entities/              # ApplicationUser, PasswordEntry, Category, Tag, UserSettings
│   │   ├── Migrations/            # InitialCreate, AddTagsAndSettings
│   │   ├── Repositories/          # IPasswordRepository, ICategoryRepository, ITagRepository, IUserSettingsRepository
│   │   └── ApplicationDbContext.cs
│   ├── Services/                  # PasswordService, TagService, UserSettingsService
│   ├── Validators/                # PasswordEntryValidator (FluentValidation)
│   ├── wwwroot/css/app.css        # Стили
│   ├── Program.cs                 # DI + конфигурация
│   └── Dockerfile                 # Multi‑stage build
├── PasswordManager.Tests/         # xUnit тесты
├── docker-compose.yml             # Запуск стека (app + postgres)
├── PasswordManager.sln
└── README.md
```

## 🐳 Запуск через Docker (рекомендуется)

```bash
cd PasswordManager

# Собрать и запустить (app + postgres)
docker compose up -d --build

# Открыть в браузере
# → http://localhost:8080
```

Стек поднимает 2 контейнера в общей сети `passvault-net`:

| Контейнер | Образ | Порт | Volume |
| --- | --- | --- | --- |
| `passvault-postgres` | `postgres:16-alpine` | `5432` | `passvault_data` |
| `passvault-app` | `passvault-image` (multi‑stage build) | `8080` | `passvault_keys` |

Приложение стартует только после успешного `pg_isready` через
`depends_on: condition: service_healthy`. На первом запуске
EF Core автоматически применит миграции (`InitialCreate` + `AddTagsAndSettings`).

### Остановка / очистка

```bash
docker compose down              # остановить
docker compose down -v           # остановить + удалить данные БД
```

### Подключение к БД из хоста

```bash
psql -h localhost -p 5432 -U postgres -d passvault
# пароль: postgres
```

## 💻 Локальная разработка

### Требования

- .NET 8 SDK
- PostgreSQL 16 (локально или через `docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=passvault postgres:16-alpine`)
- IDE: Visual Studio 2022 / Rider / VS Code

### Запуск

```bash
# Восстановление пакетов
dotnet restore

# Применение миграций
dotnet ef database update --project PasswordManager.Web

# Запуск
dotnet run --project PasswordManager.Web

# Открыть в браузере
# → https://localhost:5001
```

В `appsettings.Development.json` connection string указывает на `Host=localhost`,
в продакшене (`appsettings.json`) — на `Host=postgres` (имя сервиса в compose).

### Создание миграции

```bash
dotnet ef migrations add MigrationName --project PasswordManager.Web
dotnet ef database update --project PasswordManager.Web
```

## 🧪 Тесты

```bash
dotnet test
# С отчётом о покрытии:
dotnet test --collect:"XPlat Code Coverage"
```

Тесты покрывают:
- `PasswordRepository` — CRUD, поиск, фильтрация, изоляция между пользователями
- `CategoryRepository` — CRUD, изоляция
- `TagRepository` — CRUD, навигация N:N в обе стороны
- `UserSettingsRepository` — get-or-create семантика 1:1
- `PasswordEntryValidator` — валидация всех полей
- `PasswordService` — бизнес‑логика и работа с категориями/тегами

## 📐 Архитектура

### Dependency Injection

Все зависимости регистрируются в `Program.cs` через `IServiceCollection`:

- `ApplicationDbContext` — Scoped (на запрос/circuit)
- `IPasswordRepository`, `ICategoryRepository`, `ITagRepository`, `IUserSettingsRepository` — Scoped репозитории
- `PasswordService`, `TagService`, `UserSettingsService` — Scoped сервисы бизнес‑логики
- `IValidator<>` — авто‑регистрация через `AddValidatorsFromAssemblyContaining`

### EF Core (Code‑First)

Сущности и отношения (все три типа реляций):

```
ApplicationUser (Identity)
    ├── 1:1 → UserSettings              ← один блок настроек на пользователя
    ├── 1:N → Category                  ← пользовательские категории
    │           └── 1:N → PasswordEntry (FK = SetNull при удалении категории)
    ├── 1:N → PasswordEntry
    │           └── N:N ↔ Tag           ← через join‑таблицу PasswordEntryTags
    └── 1:N → Tag
```

| Тип отношения | Где |
| --- | --- |
| **1:1** | `ApplicationUser` ↔ `UserSettings` |
| **1:N** | `User` → `PasswordEntries`, `User` → `Categories`, `User` → `Tags`, `Category` → `PasswordEntries` |
| **N:N** | `PasswordEntry` ↔ `Tag` через `PasswordEntryTags` |

Конфигурация — Fluent API в `OnModelCreating()`:
- ограничения длины полей
- уникальные индексы (`UserSettings.UserId`, `Tags.(UserId, Name)`)
- каскадное удаление и `SetNull` для опциональных FK

### Миграции

| Миграция | Что делает |
| --- | --- |
| `20260501000000_InitialCreate` | Identity‑таблицы + `Categories` + `PasswordEntries` + индексы |
| `20260501010000_AddTagsAndSettings` | `Tags`, `UserSettings`, join‑таблица `PasswordEntryTags` |

Миграции написаны под PostgreSQL (типы `text`, `character varying`,
`timestamp with time zone`, генерация ключей через `IdentityByDefaultColumn`).
В `Program.cs` есть retry на 10 попыток × 3 сек на случай, если контейнер
БД ещё не готов в момент старта приложения.

## 🔒 Безопасность

> **Внимание:** в учебной версии пароли хранятся **без шифрования**, согласно ТЗ.
> Для продакшена обязательно нужно:
> - шифровать поле `Password` (AES + master‑key из переменной окружения / KMS)
> - использовать HTTPS и HSTS (включено)
> - двухфакторную аутентификацию (TOTP — тривиально расширить через Identity)

## 📄 Лицензия

MIT — учебный проект.
