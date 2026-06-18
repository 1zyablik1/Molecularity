# Molecularity

Минималистичная мобильная головоломка на графе молекул. Финальная цель — приложение
на Unity; игровое ядро (кор-луп) написано как чистое .NET-решение, чтобы отрабатывать
логику без редактора и покрывать тестами.

## Структура репозитория

```
Molecularity.sln          # .NET-решение (Core + Console + Tests; сюда же добавляются .NET-тулзы)
docs/                     # документация (канон: GAME-CORE.md)
src/
  Molecularity.Core/      # движок одной игровой сессии (netstandard2.1, без Unity)
  Molecularity.Console/   # консольный dev-раннер
tests/
  Molecularity.Tests/     # xUnit-тесты
tools/                    # author-time тулзы: Solver, LevelTool (см. tools/README.md)
unity/                    # Unity-клиент (визуализация + мета) — появится позже
content/
  levels/                 # JSON-уровни — общий источник правды
```

## Документация

| Файл | О чём |
|------|-------|
| **[docs/GAME-CORE.md](docs/GAME-CORE.md)** | **Канон правил ядра** — единый источник правды. |
| [docs/DOC.md](docs/DOC.md) | Game Design Document. |
| [docs/TECHDOC.md](docs/TECHDOC.md) | ТЗ под целевую Unity-архитектуру. |
| [docs/DOTNETDOC.md](docs/DOTNETDOC.md) | План «.NET-ядро → Unity». |
| [docs/RECOM.md](docs/RECOM.md) | Рекомендации по процессу. |

## Сборка и тесты

```bash
dotnet build Molecularity.sln
dotnet test  Molecularity.sln
```

## Запуск консольного раннера

```bash
dotnet run --project src/Molecularity.Console
```
