# Molecularity — .NET Console → Unity Migration Plan

> ⚠️ **АКТУАЛИЗАЦИЯ (2026-06-17).** План ниже — исходный; код частично разошёлся с ним.
> Фактические правила и статус — в [`GAME-CORE.md`](GAME-CORE.md). Ключевые отличия кода от плана:
> - Декремент паразита решено делать **через пассивку `ModifyDelta`**, а не через отдельный
>   `IDecrementStrategy` (этого интерфейса в коде нет).
> - **Сканер заменён системой предметов** (`PlayerInventory` + RevealAll/PlusOneAll/Freeze/
>   ChainBreak/Undo). Класса `Scanner` из плана нет.
> - Добавлены **снапшоты графа** и **Undo** (в плане их не было).
> - Ещё не сделано: декремент Parasite, абилка Anchor, рефактор Undo (мгновенный),
>   проводка предметов в консольный цикл, JSON-уровни, **юнит-тесты** (пока пусто).

**Дата:** 2026-03-06
**Идея:** Реализовать игровую логику как чистый .NET проект (консольное приложение), затем подключить Unity только как слой визуализации, не трогая Core.

---

## Концепция подхода

```
┌─────────────────────────────────────────────────────┐
│                 Molecularity.Core                    │
│   (чистый .NET, никакого Unity, никаких MonoBehaviour)│
│                                                      │
│  Molecule · MoleculeGraph · TurnExecutor · GameRules │
│  Abilities · PassiveProperties · LevelData           │
└───────────────────┬─────────────────────────────────┘
                    │  интерфейс IRenderer / IInputProvider
          ┌─────────┴──────────┐
          │                    │
┌─────────▼────────┐  ┌────────▼─────────────────────┐
│  Console Runner  │  │       Unity Runner             │
│  (ConsoleRenderer│  │  (MonoBehaviour Views,         │
│  + ConsoleInput) │  │   GraphView, MoleculeView...)  │
└──────────────────┘  └──────────────────────────────-┘
```

**Правило:** Molecularity.Core не знает, кто его рендерит.
Консоль и Unity реализуют одни и те же интерфейсы.

---

## Структура решения

```
Molecularity/
├── Molecularity.Core/          # .NET Standard 2.1 library
│   ├── Data/
│   ├── Domain/
│   ├── Abilities/
│   ├── Rules/
│   └── Interfaces/
├── Molecularity.Console/       # .NET Console App (runner)
│   ├── Rendering/
│   └── Input/
├── Molecularity.Tests/         # xUnit / NUnit
└── UnityProject/               # Существующий Unity-проект
    └── Assets/_Project/
        └── Core → symlink или Assembly Reference на Molecularity.Core
```

---

## ФАЗА 1 — Molecularity.Core

Цель: полностью рабочая игровая логика, покрытая тестами, без каких-либо зависимостей от Unity.

---

### 1.1 Создание проекта

- [ ] Создать папку `dotnet/` в корне репозитория
- [ ] Создать solution: `dotnet new sln -n Molecularity`
- [ ] Создать проект `Molecularity.Core` (`dotnet new classlib --framework netstandard2.1`)
- [ ] Создать проект `Molecularity.Console` (`dotnet new console`)
- [ ] Создать проект `Molecularity.Tests` (`dotnet new xunit` или `nunit`)
- [ ] Добавить все проекты в solution
- [ ] Добавить `.gitignore` для `bin/`, `obj/`
- [ ] Добавить ссылку Console → Core, Tests → Core

---

### 1.2 Data Layer (данные уровней)

#### MoleculeType

- [ ] Создать `enum MoleculeType { Simple, Parasite, Shield, Anchor }`

#### MoleculeConfig

- [ ] Создать `record MoleculeConfig(int Id, MoleculeType Type, int InitialValue, bool IsInitiallyRevealed)`
  - `Id` — уникальный идентификатор в пределах уровня
  - `InitialValue` — начальный счётчик
  - `IsInitiallyRevealed` — видно ли значение игроку с самого начала

#### ConnectionConfig

- [ ] Создать `record ConnectionConfig(int FromId, int ToId)`

#### LevelConfig

- [ ] Создать `record LevelConfig(int LevelId, List<MoleculeConfig> Molecules, List<ConnectionConfig> Connections)`
- [ ] Добавить метод валидации: `Validate()` — проверяет что все ссылки на Id корректны, граф связен

#### LevelRepository

- [ ] Создать интерфейс `ILevelRepository { LevelConfig Get(int levelId); IReadOnlyList<int> GetAllIds(); }`
- [ ] Реализовать `HardcodedLevelRepository : ILevelRepository` — уровни заданы прямо в коде (для разработки)
- [ ] Реализовать `JsonLevelRepository : ILevelRepository` — загрузка уровней из JSON-файлов

---

### 1.3 Domain Layer — Molecule

#### Molecule (runtime)

- [ ] Создать класс `Molecule`
  - `int Id`
  - `MoleculeType Type`
  - `int Value` (текущий счётчик, изменяется в процессе)
  - `bool IsRevealed` (видно ли значение)
  - `bool IsAlive` (не удалена с поля)
  - `IAbility? Ability` (on-click абилка, может быть null)
  - `IPassiveProperty? Passive` (пассивка, может быть null)
- [ ] Добавить метод `ApplyDelta(int delta)` — изменяет Value на delta (может быть отрицательным)
- [ ] Добавить метод `Reveal()` — делает IsRevealed = true
- [ ] Добавить метод `Remove()` — делает IsAlive = false
- [ ] Запрет создания напрямую — только через `MoleculeFactory`

#### MoleculeFactory

- [ ] Создать `MoleculeFactory`
- [ ] Метод `Create(MoleculeConfig config) : Molecule` — создаёт молекулу нужного типа с нужными абилками/пассивками
- [ ] Маппинг `MoleculeType` → `IAbility` + `IPassiveProperty` (через switch)

---

### 1.4 Domain Layer — MoleculeGraph

- [ ] Создать класс `MoleculeGraph`
- [ ] Внутреннее представление: `Dictionary<int, Molecule> _molecules` + `Dictionary<int, HashSet<int>> _adjacency`
- [ ] Метод `AddMolecule(Molecule m)` — добавляет молекулу в граф
- [ ] Метод `AddConnection(int fromId, int toId)` — добавляет двустороннюю связь
- [ ] Метод `GetMolecule(int id) : Molecule`
- [ ] Метод `GetNeighbors(int id) : IReadOnlyList<Molecule>` — возвращает только живых соседей
- [ ] Метод `GetAliveNeighborCount(int id) : int`
- [ ] Метод `GetAllAlive() : IReadOnlyList<Molecule>` — все живые молекулы
- [ ] Метод `RemoveMolecule(int id)` — помечает молекулу удалённой и открывает значения соседей (правило видимости)
- [ ] Метод `IsEmpty() : bool` — все молекулы удалены?
- [ ] Тест: AddMolecule → GetAllAlive возвращает нужное число
- [ ] Тест: RemoveMolecule → соседи становятся Revealed
- [ ] Тест: GetNeighbors не возвращает удалённых
- [ ] Тест: GetAliveNeighborCount корректен после удалений

---

### 1.5 Abilities (Strategy Pattern)

#### IAbility

- [ ] Создать интерфейс `IAbility { void Execute(Molecule source, MoleculeGraph graph); }`

#### NoAbility

- [ ] Создать `NoAbility : IAbility` — пустая реализация (для Simple)

#### HealNeighborsAbility (Anchor)

- [ ] Создать `HealNeighborsAbility : IAbility`
- [ ] `Execute`: все живые соседи получают `+1` к Value
- [ ] Тест: после Execute соседи имеют `initialValue + 1`

---

### 1.6 Passive Properties (Strategy Pattern)

#### IPassiveProperty

- [ ] Создать интерфейс `IPassiveProperty { void OnTurnStart(Molecule owner, MoleculeGraph graph); }`

#### NoPassive

- [ ] Создать `NoPassive : IPassiveProperty` — пустая реализация

#### ShieldProperty (Shield)

- [ ] Создать `ShieldProperty : IPassiveProperty`
- [ ] Внутреннее состояние: `int _shieldTurnsLeft` (инициализируется из конфига, например 2)
- [ ] `OnTurnStart`: если `_shieldTurnsLeft > 0` → декремент не применяется к этой молекуле (блокирует стандартный декремент), `_shieldTurnsLeft--`; иначе — не блокирует
- [ ] Метод `bool IsShielded()` для использования в TurnExecutor
- [ ] Тест: первые N ходов Value не меняется, после — меняется как обычно

---

### 1.7 Decrement Strategy

#### IDecrementStrategy

- [ ] Создать интерфейс `IDecrementStrategy { int Calculate(Molecule molecule, MoleculeGraph graph); }`

#### FlatDecrement (Simple, Shield, Anchor)

- [ ] `Calculate` → всегда возвращает `-1`

#### NeighborCountDecrement (Parasite)

- [ ] `Calculate` → возвращает `-(количество живых соседей)`
- [ ] Тест: паразит с 0 соседями → декремент 0; с 3 → декремент 3

---

### 1.8 TurnExecutor

- [ ] Создать класс `TurnExecutor`
- [ ] Зависимости в конструкторе: `MoleculeGraph graph`
- [ ] Метод `TurnResult Execute(int clickedMoleculeId)`:
  1. Получить `Molecule clicked = graph.GetMolecule(id)` — валидация что живая
  2. Выполнить `clicked.Ability.Execute(clicked, graph)` — on-click абилка
  3. Удалить молекулу: `graph.RemoveMolecule(id)` (включая Reveal соседей)
  4. Для каждой оставшейся живой молекулы:
     - Если `ShieldProperty.IsShielded()` — пропустить декремент
     - Иначе применить `DecrementStrategy.Calculate()` → `molecule.ApplyDelta(delta)`
  5. Для каждой оставшейся живой молекулы применить `Passive.OnTurnStart()`
  6. Вернуть `TurnResult`
- [ ] Тест: клик на Simple → молекула удалена, остальные уменьшены на 1
- [ ] Тест: клик на Anchor → соседи получают +1 перед удалением, затем общий декремент
- [ ] Тест: паразит с 2 соседями → декремент 2 в этот ход

#### TurnResult

- [ ] Создать `record TurnResult(bool IsWin, bool IsLoss, int? KilledByMoleculeId, List<MoleculeValueChange> Changes)`
- [ ] `MoleculeValueChange` — id молекулы + delta + новое значение

---

### 1.9 GameRules

- [ ] Создать `static class GameRules`
- [ ] Метод `static bool CheckWin(MoleculeGraph graph)` → `graph.IsEmpty()`
- [ ] Метод `static CheckLossResult CheckLoss(MoleculeGraph graph)` → есть ли живая молекула с `Value <= 0`
  - Возвращает `(bool IsLoss, int? CulpritId)`
- [ ] Тест: пустой граф → Win
- [ ] Тест: молекула с Value 0 → Loss, возвращает её Id
- [ ] Тест: молекула с Value 1 → не Loss

---

### 1.10 LevelBuilder

- [ ] Создать `LevelBuilder`
- [ ] Метод `static MoleculeGraph Build(LevelConfig config)`:
  1. Создать `MoleculeGraph`
  2. Для каждого `MoleculeConfig` — вызвать `MoleculeFactory.Create()`, добавить в граф
  3. Для каждого `ConnectionConfig` — добавить связь
  4. Вернуть готовый граф
- [ ] Тест: Build из LevelConfig с 3 молекулами → граф содержит 3 молекулы

---

### 1.11 Scanner

- [ ] Создать класс `Scanner`
- [ ] Состояние: `bool _used`
- [ ] Метод `ScanResult Use(int targetId, MoleculeGraph graph)`:
  - Если `_used` → бросить `InvalidOperationException`
  - Открыть значение целевой молекулы (`Reveal()`)
  - Открыть значения всех живых соседей (`Reveal()`)
  - Установить `_used = true`
  - Вернуть `ScanResult(int targetValue, List<(int id, int value)> neighborValues)`
- [ ] Метод `bool IsAvailable()` → `!_used`
- [ ] Метод `Reset()` — сбросить `_used = false` (вызывается при начале нового уровня)
- [ ] Тест: Use → целевая и соседи становятся Revealed
- [ ] Тест: повторный Use → исключение
- [ ] Тест: после Reset → Use снова работает

---

### 1.12 GameSession

- [ ] Создать класс `GameSession` — оркестрирует один уровень от начала до конца
- [ ] Конструктор: `GameSession(LevelConfig config)`
- [ ] Свойства: `MoleculeGraph Graph`, `Scanner Scanner`, `GameStatus Status`
- [ ] Метод `TurnResult MakeTurn(int moleculeId)`:
  - Валидация: `Status == InProgress`, молекула жива
  - Вызов `TurnExecutor.Execute(moleculeId)`
  - Проверка `GameRules.CheckLoss()` → если да, Status = Lost
  - Проверка `GameRules.CheckWin()` → если да, Status = Won
  - Вернуть `TurnResult`
- [ ] Метод `ScanResult UseScannerOn(int moleculeId)`
- [ ] Enum `GameStatus { InProgress, Won, Lost }`
- [ ] Тест: полное прохождение простого уровня из 3 молекул → статус Won
- [ ] Тест: молекула доходит до 0 → статус Lost, TurnResult содержит CulpritId

---

## ФАЗА 2 — Console Runner

Цель: играбельная консольная версия, проверка логики на живом геймплее.

---

### 2.1 Интерфейсы рендеринга

- [ ] Создать `IGameRenderer` в Core:
  ```csharp
  interface IGameRenderer {
      void RenderGraph(MoleculeGraph graph);
      void RenderTurnResult(TurnResult result);
      void RenderScanResult(ScanResult result);
      void RenderVictory();
      void RenderDefeat(int culpritId);
      void RenderMessage(string message);
  }
  ```
- [ ] Создать `IInputProvider` в Core:
  ```csharp
  interface IInputProvider {
      int RequestMoleculeId(IReadOnlyList<Molecule> aliveMolecules);
      PlayerAction RequestAction(); // Click или Scanner
  }
  enum PlayerAction { Click, UseScanner, Quit }
  ```

---

### 2.2 ConsoleRenderer

- [ ] Создать `ConsoleRenderer : IGameRenderer` в `Molecularity.Console`
- [ ] `RenderGraph`: вывести список живых молекул:
  ```
  [1] Simple  | value: 3  | neighbors: 2,3
  [2] Parasite| value: ??  | neighbors: 1
  [3] Simple  | value: 5  | neighbors: 1
  ```
  - Если `IsRevealed == false` → показать `??` вместо числа
  - Цветовая кодировка через `Console.ForegroundColor`:
    - Simple → Cyan, Parasite → Magenta, Shield → Blue, Anchor → DarkMagenta
    - Если Value == 1 → вывести красным
- [ ] `RenderTurnResult`: вывести изменения значений по каждой молекуле
  ```
  >> Молекула 1 удалена
  >> [2]: 4 → 2 (паразит, 2 соседа)
  >> [3]: 5 → 4
  ```
- [ ] `RenderVictory`: ASCII-арт или просто "=== ПОБЕДА ==="
- [ ] `RenderDefeat`: "=== ПОРАЖЕНИЕ === Молекула {id} достигла 0"
- [ ] `RenderScanResult`: подсветить значения молекулы и соседей

---

### 2.3 ConsoleInputProvider

- [ ] Создать `ConsoleInputProvider : IInputProvider`
- [ ] `RequestAction`: показать меню:
  ```
  Что делаешь? [c] клик  [s] сканер  [q] выйти
  ```
  - Читать символ, вернуть `PlayerAction`
- [ ] `RequestMoleculeId`: показать список живых молекул с Id, читать число
  - Валидация: повторять запрос если введён неверный Id или Id мёртвой молекулы

---

### 2.4 ConsoleGameLoop

- [ ] Создать `ConsoleGameLoop`
- [ ] Конструктор: `ConsoleGameLoop(ILevelRepository repo, IGameRenderer renderer, IInputProvider input)`
- [ ] Метод `Run()`:
  1. Вывести список доступных уровней
  2. Запросить выбор уровня
  3. Создать `GameSession(levelConfig)`
  4. Цикл пока `session.Status == InProgress`:
     - Рендерить граф
     - Запросить действие
     - Если Click → запросить Id → `session.MakeTurn(id)` → рендерить результат
     - Если Scanner → запросить Id → `session.UseScannerOn(id)` → рендерить результат
     - Если Quit → выйти
  5. Рендерить финальный экран (победа/поражение)
  6. Спросить "сыграть ещё?"

---

### 2.5 Тестовые уровни (HardcodedLevelRepository)

- [ ] Уровень 1 (Tutorial): 3 молекулы, граф A-B-C, значения 3,2,1, все видны
- [ ] Уровень 2 (Видимость): 5 молекул, звезда, центр скрыт
- [ ] Уровень 3 (Таймеры): 4 молекулы, ромб, значения 5,3,2,1
- [ ] Уровень 4 (Паразит): 4 молекулы, паразит в центре, 3 простых вокруг
- [ ] Уровень 5 (Щит): 5 молекул, щит в центре связан со всеми
- [ ] Уровень 6 (Якорь): 4 молекулы, якорь + 3 простых, anchor в центре

---

### 2.6 JSON уровни (JsonLevelRepository)

- [ ] Определить формат JSON:
  ```json
  {
    "levelId": 1,
    "molecules": [
      { "id": 1, "type": "Simple", "initialValue": 3, "isInitiallyRevealed": true }
    ],
    "connections": [
      { "fromId": 1, "toId": 2 }
    ]
  }
  ```
- [ ] Реализовать `JsonLevelRepository` через `System.Text.Json`
- [ ] Загрузка из папки `levels/` рядом с исполняемым файлом
- [ ] Создать JSON-файлы для 6 тестовых уровней

---

## ФАЗА 3 — Unit Tests

Цель: покрыть все критические пути тестами перед миграцией в Unity.

---

### 3.1 MoleculeGraph Tests

- [ ] Тест: `RemoveMolecule` раскрывает IsRevealed у всех живых соседей
- [ ] Тест: `GetNeighbors` не возвращает удалённые молекулы
- [ ] Тест: `IsEmpty` возвращает true только когда все молекулы удалены
- [ ] Тест: `GetAliveNeighborCount` корректно уменьшается при удалениях

### 3.2 TurnExecutor Tests

- [ ] Тест: клик на Simple → она удалена, остальные получили -1
- [ ] Тест: клик на последнюю молекулу → граф пуст (Win)
- [ ] Тест: ход когда молекула с Value 1 → она умирает (Loss)
- [ ] Тест: Anchor → соседи +1, потом общий декремент -1 (итого 0 или +0)
- [ ] Тест: Parasite с 2 соседями → декремент -2 (не -1)
- [ ] Тест: Shield первые N ходов → не получает декремент, после — получает

### 3.3 GameRules Tests

- [ ] Тест: Win только когда граф пуст
- [ ] Тест: Loss когда Value <= 0 у живой молекулы
- [ ] Тест: Loss возвращает правильный CulpritId

### 3.4 Scanner Tests

- [ ] Тест: Use раскрывает target и соседей
- [ ] Тест: двойной Use → исключение
- [ ] Тест: Reset → Use снова доступен

### 3.5 GameSession Integration Tests

- [ ] Тест: полное прохождение Level 1 — статус Won
- [ ] Тест: намеренный проигрыш (пропустить опасную молекулу) — статус Lost
- [ ] Тест: использование Scanner меняет IsRevealed

### 3.6 Decrement Strategy Tests

- [ ] Тест: `NeighborCountDecrement` с 0 соседями → 0
- [ ] Тест: `NeighborCountDecrement` с 3 соседями → 3
- [ ] Тест: `FlatDecrement` всегда → 1

---

## ФАЗА 4 — Unity Integration

Цель: подключить `Molecularity.Core` к Unity без изменения Core-кода.

---

### 4.1 Подключение Core к Unity

- [ ] Скомпилировать `Molecularity.Core` как DLL (`dotnet build -c Release`)
- [ ] Скопировать `Molecularity.Core.dll` в `Assets/Plugins/MolecularityCore/`
- [ ] Убедиться что DLL видна в Unity Inspector
- [ ] Альтернатива: Assembly Definition с общими исходниками (через symlink или git subtree)

### 4.2 UnityGameRenderer

- [ ] Создать `UnityGameRenderer : MonoBehaviour, IGameRenderer`
- [ ] `RenderGraph(MoleculeGraph graph)` → вызывает `GraphView.Rebuild(graph)`
- [ ] `RenderTurnResult(TurnResult result)` → запускает анимации через `MoleculeAnimator`
- [ ] `RenderVictory()` → показывает `VictoryScreen`
- [ ] `RenderDefeat(culpritId)` → показывает `DefeatScreen`, подсвечивает молекулу

### 4.3 UnityInputProvider

- [ ] Создать `UnityInputProvider : MonoBehaviour, IInputProvider`
- [ ] Обрабатывает клики по молекулам через Raycast / Physics2D
- [ ] `RequestMoleculeId` — ждёт клика по молекуле, возвращает её Id
- [ ] `RequestAction` — ждёт либо клика по молекуле (Click) либо нажатия на кнопку сканера (UseScanner)

### 4.4 UnityGameLoop

- [ ] Создать `UnityGameLoop : MonoBehaviour`
- [ ] Хранит `GameSession _session`
- [ ] Метод `StartLevel(LevelConfig config)` — инициализирует сессию, рендерит граф
- [ ] Подписывается на события от `UnityInputProvider`
- [ ] При получении хода — вызывает `_session.MakeTurn(id)`, рендерит результат
- [ ] Состояние игры читается из `_session.Status`

### 4.5 GraphView (Unity)

- [ ] Создать `GraphView : MonoBehaviour`
- [ ] Хранит пул `MoleculeView` объектов
- [ ] Метод `Rebuild(MoleculeGraph graph)` — для каждой живой молекулы инстанциирует/обновляет `MoleculeView`
- [ ] Позиции молекул из `LevelConfig` (поле `Vector2 Position` в `MoleculeConfig`)
- [ ] Отрисовывает `ConnectionView` для каждой связи

### 4.6 MoleculeView (Unity)

- [ ] Создать `MoleculeView : MonoBehaviour`
- [ ] Принимает `Molecule molecule` — отображает тип, значение (если revealed), цвет по типу
- [ ] Анимация пульсации (DOTween loop)
- [ ] Метод `UpdateState(Molecule m)` — обновить текст и видимость значения
- [ ] Метод `PlayClickAnimation()` → анимация исчезновения + частицы
- [ ] Метод `PlayDecrementAnimation(int delta)` → текст "-1" летит вверх

### 4.7 ScriptableObject LevelConfig

- [ ] Создать `LevelConfigSO : ScriptableObject` — Unity-обёртка над `LevelConfig`
- [ ] Поля: список молекул (с позицией на экране), список связей
- [ ] Custom Editor или стандартные поля в Inspector
- [ ] Метод `ToLevelConfig() : LevelConfig` — конвертация в Core-тип

### 4.8 UnityLevelRepository

- [ ] Создать `UnityLevelRepository : ILevelRepository`
- [ ] Загружает `LevelConfigSO` из `Resources/Levels/` через `Resources.Load`
- [ ] Конвертирует через `ToLevelConfig()`

---

## Чеклист готовности к переходу на Unity

- [ ] Все тесты в `Molecularity.Tests` проходят
- [ ] Консольная версия играбельна (6 уровней)
- [ ] Все типы молекул реализованы и протестированы: Simple, Parasite, Shield, Anchor
- [ ] Scanner работает корректно
- [ ] Win/Loss определяется правильно во всех крайних случаях
- [ ] LevelConfig загружается из JSON
- [ ] Core не содержит ни одной ссылки на Unity namespace

---

## Принципы, которые нельзя нарушать

1. **Core не зависит от Unity** — ни одного `using UnityEngine` в `Molecularity.Core`
2. **Логика не в View** — `MoleculeView` только рендерит, не считает
3. **TurnExecutor — единственное место** где происходит ход
4. **GameRules — статический** без состояния, чистые функции
5. **ILevelRepository скрывает источник данных** — Core не знает, JSON это или ScriptableObject

---

**Версия:** 1.0
**Статус:** Draft
