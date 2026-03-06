
# Техническое задание (ТЗ)

## Оглавление
- [Текущий статус реализации](#текущий-статус-реализации)
- [Общие требования](#общие-требования-тз)
- [Архитектура проекта](#архитектура-проекта-тз)
- [Модули системы](#модули-системы-тз)
- [Технологический стек](#технологический-стек-тз)
- [Паттерны и практики](#паттерны-и-практики-тз)
- [Структура проекта](#структура-проекта-тз)
- [План реализации](#план-реализации-тз)
- [Оптимизация и производительность](#оптимизация-и-производительность-тз)

---

## Текущий статус реализации {#текущий-статус-реализации}

**Последнее обновление:** 2026-02-16

### Что реализовано (Фаза 0 — частично)

| Компонент | Статус | Примечание |
|-----------|--------|------------|
| Проект Unity | Готов | Unity + URP, TextMeshPro установлен |
| Git | Настроен | .gitignore, .editorconfig |
| State Machine (каркас) | Готов | Интерфейс IState, enum State, класс StateMachine |
| Состояния (скелеты) | Готов | Loading, Initializing, Menu, LoadLevel, Level, UnLoadLevel — пустые реализации |
| AppBootstrap | Частично | MonoBehaviour, создает StateMachine в Awake(), но не запускает |
| UnityGameApp | Заглушка | Пустой класс с комментарием "Game Loop here" |
| Contexts | Не начато | Пустая директория |

### Что НЕ реализовано

- Core Game Logic (Molecule, MoleculeGraph, GameRules, TurnExecutor)
- Визуализация (MoleculeView, ConnectionView, GraphView)
- UI система (меню, HUD, экраны результата)
- Input система
- Audio система
- Save/Load
- Монетизация (Ads, IAP)
- Аналитика
- Контент (уровни, типы молекул)
- Режимы игры (бесконечный, раскраска)

### Текущая файловая структура (фактическая)

```
Assets/
├── Scipts/                          # ВНИМАНИЕ: опечатка в названии (Scripts)
│   ├── AppBootstrap.cs              # Точка входа (MonoBehaviour)
│   ├── UnityGameApp.cs              # Заглушка игрового цикла
│   ├── Contexts/                    # Пустая директория (подготовлена)
│   └── StateMachine/
│       ├── IState.cs                # Интерфейс состояния
│       ├── State.cs                 # Enum состояний (6 штук)
│       ├── StateMachine.cs          # Ядро стейт-машины (Dictionary<State, IState>)
│       └── States/
│           ├── LoadingState.cs      # Пустой скелет
│           ├── InitializingState.cs # Пустой скелет
│           ├── MenuState.cs         # Пустой скелет
│           ├── LoadLevelState.cs    # Пустой скелет
│           ├── LevelState.cs        # Пустой скелет
│           └── UnLoadLevelState.cs  # Пустой скелет
├── Scenes/
│   └── SampleScene.unity            # Стандартная сцена
├── Settings/                        # URP и настройки проекта
└── TextMesh Pro/                    # Библиотека UI текста
```

---

## Общие требования {#общие-требования-тз}

### Целевая платформа
- **Engine:** Unity LTS
- **Rendering:** URP (Universal Render Pipeline)
- **Platform:** Android (API Level 24+, т.е. Android 7.0+)
- **Target Devices:** Средние и флагманские смартфоны (2019+)
- **Orientation:** Portrait only (вертикальная)
- **Resolution:** 1080x1920 (reference), поддержка 9:16 до 9:21

### Технические ограничения

**Производительность:**
- **Target FPS:** 60 на средних устройствах
- **Minimum FPS:** 30 на слабых устройствах
- **APK Size:** Максимум 50-70 МБ (без Google Play Asset Delivery)
- **RAM Usage:** Максимум 300-400 МБ

**Батарея:**
- Минимальное энергопотребление (оптимизация GPU/CPU нагрузки)

### Качество кода

**Обязательно:**
- Code Style: следовать C# Coding Conventions
- Комментарии: для всех публичных API и сложных алгоритмов
- Naming: понятные имена переменных/методов (избегать аббревиатур)
- SOLID принципы (особенно Single Responsibility, Dependency Inversion)

**Рекомендуется:**
- XML документация для публичных классов
- Unit тесты для критичной логики (GameManager, MoleculeGraph)

---

## 🏛️ Архитектура проекта {#архитектура-проекта-тз}

### Общий подход

**Рекомендуется:** Модульная архитектура с разделением на слои (Layered Architecture)

```
┌─────────────────────────────────────┐
│         Presentation Layer          │  ← UI, View, Input
│  (UI Controllers, View Components)  │
├─────────────────────────────────────┤
│         Application Layer           │  ← Game Logic, State Management
│   (GameManager, LevelController)    │
├─────────────────────────────────────┤
│           Domain Layer              │  ← Core Business Logic
│  (Molecule, Graph, Rules Engine)    │
├─────────────────────────────────────┤
│            Data Layer               │  ← Persistence, Serialization
│   (SaveSystem, LevelLoader)         │
├─────────────────────────────────────┤
│       Infrastructure Layer          │  ← External Services
│ (Analytics, Ads, IAP, Audio)        │
└─────────────────────────────────────┘
```

### Ключевые принципы

1. **Separation of Concerns:**
    - Логика игры отделена от визуализации
    - Data отделена от Presentation

2. **Dependency Injection:**
    - Модули получают зависимости через конструкторы/методы
    - Избегать прямых ссылок на синглтоны (где возможно)
    - Использовать фреймворк RefleX

3. **Event-Driven Communication:**
    - Модули общаются через события (C# events или UnityEvent)
    - Минимизация прямых зависимостей

4. **Stateless где возможно:**
    - Избегать глобального мутабельного состояния
    - State хранится в конкретных компонентах (GameState, LevelState)

5. **Testability:**
    - Core Logic (Domain Layer) не зависит от Unity, что позволяет писать Unit тесты
    - Application Layer и Presentation Layer могут использовать Mock объекты для тестирования

6. Детерминированность:
    - одинаковый вход → одинаковый выход (особенно для Core Logic)

---

## 🧩 Модули системы {#модули-системы-тз}

### 1. Core Module (Ядро игры)

**Назначение:** Основная игровая логика, не зависящая от Unity

**Компоненты:**

**A) Data Structures:**
- `MoleculeData` — данные молекулы (ScriptableObject или POCO)
- `LevelData` — данные уровня (ScriptableObject)
- `GraphData` — структура графа

**Б) Domain Logic:**
- `Molecule` — runtime представление молекулы
- `MoleculeGraph` — граф молекул с логикой связей
- `GameRules` — правила игры (проверка win/lose, валидация ходов)
- `TurnExecutor` — выполнение хода (клик → абилка → world step)

**В) Abilities & Properties:**
- `IAbility` — интерфейс для on-click абилок
- `IPassiveProperty` — интерфейс для пассивных свойств
- Конкретные реализации: `HealNeighborsAbility` (для якоря), `ShieldProperty`, `VirusProperty`, и т.д.

**Паттерны:**
- **Strategy** — для абилок и свойств
- **Command** — для выполнения хода (возможность undo, replay)
- **Factory** — создание молекул и абилок и уровней

---

### 2. Game Management Module

**Назначение:** Управление игровым процессом, состояниями, потоком

**Компоненты:**

**A) State Management:**
- `GameStateMachine` — управление состояниями игры
    - States: MainMenu, ColoringMap (основная кампания — выбор уровня на рисунке), Gameplay, Pause, Victory, Defeat, EndlessMode
- `LevelController` — управление текущим уровнем
    - Загрузка уровня
    - Инициализация графа
    - Обработка игровых событий

**Б) Turn Management:**
- `TurnManager` — обработка ходов игрока
    - Валидация клика
    - Вызов TurnExecutor
    - Обновление состояния
    - Проверка win/lose

**В) Progression:**
- `ProgressionManager` — управление прогрессом игрока
    - Прогресс раскрасок (открытые фрагменты, завершённые рисунки)
    - Сохранение/загрузка прогресса

**Г) Scanner:**
- `ScannerManager` — управление абилкой сканера
    - Активация сканера
    - Подсветка молекулы + соседей
    - Показ значений
    - Перезарядка каждый уровень

**Паттерны:**
- **State Machine** — для управления состояниями
- **Observer** — уведомления о смене состояний
- **Facade** — упрощенный API для взаимодействия с игровым потоком

---

### 3. Presentation Module (Визуализация)

**Назначение:** Отрисовка игровых объектов, анимации, VFX

**Компоненты:**

**A) Views:**
- `MoleculeView` — визуальное представление молекулы
    - Sprite/Mesh для круга
    - TextMeshPro для счетчика
    - ParticleSystem для пульсации/эффектов
    - Обработка input (клик, зажатие)
    - Визуальное состояние (обычное, подсвеченное сканером, скрытое)
- `ConnectionView` — визуализация связи между молекулами
    - LineRenderer или custom Mesh
    - Shader для анимации волн

**Б) Graph Visualization:**
- `GraphLayouter` — расстановка молекул на экране (layout algorithm)
    - Force-Directed Layout (рекомендуется)
    - Spring Embedder
    - Или pre-defined позиции из LevelData
- `GraphView` — управление отрисовкой всего графа

**В) Animation & VFX:**
- `MoleculeAnimator` — анимации молекул (пульсация, клик, исчезновение)
- `ScannerEffects` — эффекты сканера (волна, подсветка, показ значений)
- `ParticleManager` — управление частицами (взрывы, конфетти)
- `ScreenEffects` — полноэкранные эффекты (flash, shake)

**Г) UI Components:**
- `HUD` — интерфейс во время игры (топ-бар, абилки, сканер кнопка)
- `ScannerButton` — UI кнопка сканера (активная/неактивная)
- `VictoryScreen`, `DefeatScreen` — экраны результата
- `MainMenu` — главное меню
- `EndlessModeHUD` — HUD для бесконечного режима (волна, рекорд)
- `ColoringModeScreen` — экран раскраски с силуэтом
- `MiniPuzzlePopup` — popup для мини-головоломок в раскраске

**Паттерны:**
- **MVC/MVP** — разделение логики и визуализации
- **Object Pool** — для частиц и эффектов
- **Observer** — подписка views на изменения модели

---

### 4. Data & Persistence Module

**Назначение:** Сохранение/загрузка данных, работа с уровнями

**Компоненты:**

**A) Level System:**
- `LevelDatabase` — ScriptableObject с массивом всех уровней
- `LevelLoader` — загрузка уровня по ID
- `LevelValidator` — проверка корректности данных уровня
- `ColoringImageDatabase` — ScriptableObject с раскрасками

**Б) Save System:**
- `SaveManager` — управление сохранениями
    - Прогресс раскрасок (пройденные уровни, открытые фрагменты, завершённые рисунки)
    - Настройки (звук, подсказки)
    - Рекорд бесконечного режима
- `SaveData` — структура данных для сохранения
- `ISerializer` — интерфейс сериализатора (JSON, Binary)

**В) Settings:**
- `GameSettings` — настройки игры (ScriptableObject)
    - Audio volumes
    - Tutorial enabled/disabled
    - и т.д.

**Паттерны:**
- **Repository** — для доступа к данным уровней
- **Strategy** — для выбора сериализатора

**Технологии:**
- **Сохранения:** PlayerPrefs (для простых данных) + JSON (для сложных)
    - Или: использовать Easy Save 3 (платный Asset Store)
- **Encryption (опционально):** ObscuredPrefs (Anti-Cheat Toolkit) для защиты от читерства

---

### 5. Input Module

**Назначение:** Обработка пользовательского ввода

**Компоненты:**

**A) Input Handling:**
- `InputManager` — единая точка входа для input
    - Клик на молекулу
    - Клик на кнопку сканера
    - Зажатие для подсказки
    - Swipe для навигации (опционально)
- `TouchDetector` — определение touch событий
    - Raycast к молекулам
    - Различение tap/hold

**Б) Scanner Input:**
- Режим сканера: после клика на кнопку сканера, следующий клик на молекулу активирует сканер

**Паттерны:**
- **Command** — действия от input как команды
- **Observer** — события input для подписчиков
- **State** — режимы input (обычный клик / клик для сканера)

**Технологии:**
- **Unity Input System** (новая, рекомендуется)

---

### 6. Audio Module

**Назначение:** Управление музыкой и звуками

**Компоненты:**

**A) Audio Management:**
- `AudioManager` — управление всеми звуками
    - Play/Stop музыки
    - Play SFX с pooling
    - Управление громкостью
- `MusicPlayer` — проигрывание музыкальных треков
    - Crossfade между треками
    - Loop
- `SFXPlayer` — проигрывание звуковых эффектов
    - 2D звуки (UI, gameplay)
    - Pitch variation для разнообразия

**Б) Audio Data:**
- `AudioClipLibrary` — ScriptableObject с references на все AudioClips
    - Организовано по категориям (UI, Gameplay, Music, Scanner)

**Новые звуки:**
- Звуки сканера (активация, волна, показ значений)
- Звуки бесконечного режима (переход на волну)
- Звуки раскраски (открытие фрагмента, завершение)

**Паттерны:**
- **Singleton/Service Locator** — для AudioManager
- **Object Pool** — для AudioSource компонентов (SFX)
- **Factory** — создание audio источников

**Технологии:**
- **Unity AudioSource** + **AudioMixer** для управления группами (Music, SFX, Master)

---

### 7. UI Module

**Назначение:** Интерфейс пользователя (меню, экраны, HUD)

**Компоненты:**

**A) Screen System:**
- `UIManager` — управление переключением экранов
    - Stack-based navigation (для back button)
    - Transition анимации
- `BaseScreen` — базовый класс для всех UI экранов
    - Show/Hide методы
    - Lifecycle (OnOpen, OnClose)

**Б) Screens:**
- `MainMenuScreen`
- `ColoringModeScreen` (основная кампания — экран с рисунком и молекулами-уровнями)
- `GameplayHUD` (с кнопкой сканера)
- `EndlessModeHUD` (с отображением волны)
- `VictoryScreen`
- `DefeatScreen`
- `SettingsScreen`
- `GlossaryScreen`
- `ColoringGalleryScreen`

**В) UI Components:**
- `ColoringFragmentDot` — молекула-точка на рисунке раскраски (клик = открыть уровень)
- `MoleculeTooltip` — всплывающая подсказка при зажатии молекулы
- `ScannerButton` — кнопка сканера (активная/серая)
- `WaveTransition` — анимированный текст перехода на волну (бесконечный режим)
- `MiniPuzzlePopup` — popup для мини-головоломок (раскраска)
- `ConfirmDialog` — диалоговое окно подтверждения

**Паттерны:**
- **MVC/MVP** — для сложных экранов
- **Observer** — обновление UI на события
- **Command** — кнопки как команды

**Технологии:**
- **Unity UI (Canvas)** с Canvas Scaler (reference resolution 1080x1920)
- **TextMeshPro** для текста (более качественный шрифт)
- **DOTween** или **LeanTween** для UI анимаций (рекомендуется DOTween)

---

### 8. Monetization Module

**Назначение:** Реклама и IAP

**Компоненты:**

**A) Ads:**
- `AdsManager` — управление рекламой
    - Инициализация SDK
    - Show Interstitial (с частотой)
    - Show Rewarded Video
    - Обработка callbacks (показано/пропущено/ошибка)
- `AdsConfig` — ScriptableObject с настройками
    - Ad Unit IDs (Android/iOS)
    - Частота показа Interstitial (каждые N уровней)

**Б) IAP:**
- `IAPManager` — управление покупками
    - Инициализация Unity IAP
    - Purchase flow
    - Restore purchases (для iOS)
- `IAPProduct` — класс для продуктов
    - ID, цена, название

**В) Revive System:**
- `ReviveManager` — логика ревайва
    - Предложение просмотра рекламы при поражении
    - Восстановление состояния игры (+2 ко всем молекулам)

**Паттерны:**
- **Singleton/Service Locator** — для Managers
- **Observer** — события покупок/просмотров рекламы
- **Strategy** — разные стратегии монетизации (test/production)

**Технологии:**
- **Google Mobile Ads SDK** (AdMob) для рекламы
- **Unity IAP** для покупок (обертка над Google Play Billing)

**⚠️ ВАЖНО:**
- Соблюдать GDPR/COPPA (consent для рекламы)
- Test Ads IDs для разработки
- Production Ads IDs только для релиза

---

### 9. Analytics Module

**Назначение:** Сбор метрик и событий

**Компоненты:**

**A) Analytics:**
- `AnalyticsManager` — отправка событий
    - Level start/complete
    - Revive used
    - Ad impression/click
    - IAP purchase
    - Scanner used
    - Endless mode stats (wave reached)
    - Coloring mode stats (fragments opened, skips)
    - и т.д.
- `AnalyticsEvent` — структура события (enum или класс)

**Б) Integration:**
- Обертки для разных провайдеров (Firebase, Unity Analytics)

**Паттерны:**
- **Singleton/Service Locator**
- **Adapter** — для разных analytics SDK

**Технологии:**
- **Firebase Analytics** (рекомендуется, бесплатно, мощно)
- **Unity Analytics** (встроенно, проще)

**События для отслеживания:**
- `level_start(level_id)`
- `level_complete(level_id, moves, time)`
- `level_fail(level_id, attempts)`
- `revive_used(level_id)`
- `scanner_used(level_id, molecule_id)`
- `endless_mode_start()`
- `endless_mode_end(wave_reached)`
- `coloring_fragment_opened(image_id, fragment_id)`
- `coloring_fragment_skipped(image_id, fragment_id)`
- `coloring_complete(image_id)`
- `ad_impression(type, placement)`
- `iap_purchase(product_id, price)`

---

### 10. Game Modes Module

**Назначение:** Специфическая логика для режимов игры

**Компоненты:**

**A) Coloring Mode (основная кампания):**

> Раскраска — это основной режим игры. Кнопка "ИГРАТЬ" в главном меню ведёт к текущей раскраске. Каждая молекула-точка на рисунке = отдельный уровень-головоломка. Прохождение уровня открывает фрагмент рисунка. Завершённый рисунок сохраняется в галерею, далее переход к следующему.

- `ColoringModeManager` — управление основной кампанией
    - Загрузка раскраски (силуэт + молекулы-фрагменты)
    - Открытие фрагментов при прохождении уровня
    - Сохранение прогресса
    - Переход к следующей раскраске при завершении
- `LevelController` — контроллер уровня-головоломки
    - Загрузка уровня (граф молекул из LevelData)
    - Обычные правила игры (клик, декремент, win/lose)
- `ColoringImage` — ScriptableObject с данными раскраски
    - Силуэт (спрайт или позиции)
    - Финальный арт
    - Позиции фрагментов (молекулы-точки)
    - Ссылки на LevelData для каждого фрагмента

**Б) Endless Mode (дополнительный режим):**
- `EndlessModeManager` — управление волновым бесконечным режимом
    - Спавн волн
    - Усложнение (количество молекул, типы, значения)
    - Подсчет рекорда
    - Переход между волнами
- `WaveGenerator` — генерация графа для волны
    - Количество молекул
    - Типы (пул зависит от номера волны)
    - Стартовые значения
    - Граф (сложность растет)

**Паттерны:**
- **Strategy** — разные стратегии генерации волн
- **Factory** — создание уровней
- **State** — состояния раскраски (в процессе, завершена)

---

### 11. Utilities Module

**Назначение:** Вспомогательные утилиты

**Компоненты:**

**A) Extensions:**
- `Vector2Extensions` — расширения для Vector2
- `ColorExtensions` — утилиты для работы с цветом
- `TransformExtensions` — расширения Transform

**Б) Helpers:**
- `CoroutineRunner` — запуск корутин вне MonoBehaviour
- `TimeUtils` — работа с временем (таймеры, cooldowns)
- `MathUtils` — математические утилиты (lerp, clamp, и т.д.)

**В) Debug Tools:**
- `DebugConsole` — внутриигровая консоль для чит-кодов (только в Development Build)
    - Пропустить уровень
    - Разблокировать все
    - Разблокировать бесконечный режим
    - Открыть все раскраски

**Паттерны:**
- **Singleton** — для CoroutineRunner
- **Static Utility Classes** — для Extensions/Helpers

---

## 🛠️ Технологический стек {#технологический-стек-тз}

### Основное

| Компонент | Технология | Обоснование |
|-----------|------------|-------------|
| **Engine** | Unity 2022.3 LTS | Стабильность, поддержка Android |
| **Rendering** | URP | Производительность на мобильных |
| **Language** | C# 9.0+ | Современные фичи (pattern matching, records) |
| **UI** | Unity UI + TextMeshPro | Стандарт для мобильных игр |
| **Анимации** | DOTween (платный) | Легкость использования, производительность |

### Дополнительные SDK

| SDK | Назначение | Обязательность |
|-----|------------|----------------|
| **Google Mobile Ads** | Реклама | Обязательно |
| **Unity IAP** | Покупки | Обязательно |
| **Firebase SDK** | Analytics, Crashlytics | Рекомендуется |
| **TextMeshPro** | Качественный текст | Обязательно (встроен в Unity) |
| **RefleX** | Dependency Injection | Рекомендуется (для управления зависимостями) |

### Опциональные плагины (Asset Store)

| Плагин | Назначение | Цена |
|--------|------------|------|
| **DOTween** | Анимации | бесплатная версия |
| **Shader Graph** | Кастомные шейдеры | Бесплатно (встроен в URP) |

**Рекомендация:** Обойтись без платных плагинов для MVP (кроме DOTween Free)

---

## 🎨 Паттерны и практики {#паттерны-и-практики-тз}

### Архитектурные паттерны

**1. MVC (Model-View-Controller) / MVP (Model-View-Presenter)**
- **Где:** UI Screens, MoleculeView
- **Зачем:** Разделение логики и визуализации
- **Пример:**
    - Model: `Molecule` (данные + логика)
    - View: `MoleculeView` (отрисовка)
    - Controller: `GameManager` (связывает Model и View)

**2. State Machine**
- **Где:** `GameStateMachine`, `TurnManager`, `InputManager` (режим сканера)
- **Зачем:** Управление сложными состояниями
- **Реализация:** Enum + switch или отдельные классы State

**3. Command Pattern**
- **Где:** `TurnExecutor`, Input actions
- **Зачем:** Инкапсуляция действий (возможность undo/redo, replay)
- **Пример:** `ClickMoleculeCommand`, `ExecuteTurnCommand`, `UseScannerCommand`

**4. Strategy Pattern**
- **Где:** Абилки (`IAbility`), пассивные свойства (`IPassiveProperty`), генерация волн
- **Зачем:** Гибкость в добавлении новых типов молекул и логики
- **Пример:**
  ```csharp
  public interface IAbility {
      void Execute(Molecule molecule, MoleculeGraph graph);
  }

  public class HealNeighborsAbility : IAbility { ... }
  public class TeleportAbility : IAbility { ... }
  ```

**5. Factory Pattern**
- **Где:** Создание молекул, абилок, уровней (мини-головоломки)
- **Зачем:** Централизованное создание объектов
- **Пример:** `MoleculeFactory.Create(MoleculeData data)`, `MiniPuzzleFactory.Generate(difficulty)`

**6. Observer Pattern (Events)**
- **Где:** Коммуникация между модулями
- **Зачем:** Избежать сильной связанности
- **Реализация:** C# events
- **Примеры событий:**
    - `OnMoleculeClicked`
    - `OnMoleculeDestroyed`
    - `OnScannerUsed`
    - `OnLevelComplete`
    - `OnGameOver`
    - `OnWaveComplete` (endless mode)
    - `OnFragmentOpened` (coloring mode)

**7. Object Pool**
- **Где:** Частицы, SFX AudioSources, MoleculeView (если переиспользуются)
- **Зачем:** Производительность (избежать Instantiate/Destroy)
- **Реализация:** кастомный

**8. RefleX / Service Locator**
- **Где:** `GameManager`, `AudioManager`, `SaveManager`, `ScannerManager`
- **Зачем:** Глобальный доступ к сервисам
- **⚠️ Осторожно:** Не злоупотреблять, предпочитать Dependency Injection где возможно

### SOLID принципы

**S - Single Responsibility:**
- Каждый класс отвечает за одну вещь
- Пример: `TurnExecutor` только выполняет ход, не управляет UI
- Пример: `ScannerManager` только управляет сканером, не занимается визуализацией

**O - Open/Closed:**
- Открыт для расширения, закрыт для модификации
- Пример: Новые типы молекул добавляются через наследование `IAbility`, а не изменение `TurnExecutor`

**L - Liskov Substitution:**
- Наследники должны заменяться базовым классом без поломки логики
- Пример: Любой `IAbility` можно использовать в `Molecule.ExecuteAbility()`

**I - Interface Segregation:**
- Много маленьких интерфейсов лучше одного большого
- Пример: `IAbility`, `IPassiveProperty` вместо одного `IMoleculeEffect`

**D - Dependency Inversion:**
- Зависеть от абстракций, а не конкретных реализаций
- Пример: `TurnExecutor` зависит от `IAbility`, а не от `HealNeighborsAbility`

### Практики кодирования

**1. Naming Conventions:**
- Classes/Structs: `PascalCase`
- Methods: `PascalCase`
- Private fields: `_camelCase` (с underscore)
- Public fields/properties: `PascalCase`
- Constants: `UPPER_SNAKE_CASE`

**2. Code Organization:**
- Один класс = один файл
- Файлы группируются по модулям (папки)
- Избегать "God Objects" (классы >500 строк)

**3. Comments:**
- XML комментарии для публичных API:
  ```csharp
  /// <summary>
  /// Executes on-click ability of the molecule
  /// </summary>
  /// <param name="graph">Current molecule graph</param>
  public void ExecuteAbility(MoleculeGraph graph) { ... }
  ```
- Inline комментарии для сложных алгоритмов

**4. Error Handling:**
- Использовать try-catch для critical paths (сохранения, загрузка уровней)
- Логировать ошибки через `Debug.LogError` или analytics
- Graceful degradation (игра не крашится, показывает сообщение пользователю)
- Обернуть отлов ошибок в отделтьный `ErrorHandler` для централизованного управления

**5. Magic Numbers:**
- Избегать хардкода значений, использовать константы:
  ```csharp
  // Плохо
  if (molecule.Value <= 2) { ... }

  // Хорошо
  private const int CRITICAL_VALUE_THRESHOLD = 2;
  if (molecule.Value <= CRITICAL_VALUE_THRESHOLD) { ... }
  ```

---

## Структура проекта (целевая) {#структура-проекта-тз}

> **Примечание:** Текущая фактическая структура описана в разделе [Текущий статус реализации](#текущий-статус-реализации). Ниже — целевая структура, к которой проект должен прийти.


```
Assets/
├── _Project/                       # Основная папка проекта
│   ├── Art/                        # Графика
│   │   ├── Sprites/                # 2D спрайты
│   │   │   ├── UI/                 # UI элементы
│   │   │   ├── Molecules/          # Иконки молекул (если используются)
│   │   │   ├── Icons/              # Иконки абилок, кнопок, сканера
│   │   │   └── Coloring/           # Силуэты и финальные арты раскрасок
│   │   ├── Shaders/                # Кастомные шейдеры
│   │   │   ├── MoleculeGlow.shader
│   │   │   ├── ConnectionWave.shader
│   │   │   └── ScannerWave.shader
│   │   ├── Materials/              # Материалы
│   │   │   ├── MoleculeMaterials/
│   │   │   ├── ConnectionMaterial.mat
│   │   │   └── ScannerMaterial.mat
│   │   └── VFX/                    # Particle Systems, VFX Graph
│   │       ├── ClickExplosion.prefab
│   │       ├── Confetti.prefab
│   │       └── ScannerWave.prefab
│   │
│   ├── Audio/                      # Звуки и музыка
│   │   ├── Music/
│   │   │   ├── MainTheme.mp3
│   │   │   ├── GameplayTheme.mp3
│   │   │   ├── EndlessTheme.mp3
│   │   │   └── ColoringTheme.mp3
│   │   ├── SFX/
│   │   │   ├── UI/                 # Звуки UI
│   │   │   ├── Gameplay/           # Игровые звуки
│   │   │   ├── Scanner/            # Звуки сканера
│   │   │   ├── Endless/            # Звуки бесконечного режима
│   │   │   └── Coloring/           # Звуки раскраски
│   │   └── AudioClipLibrary.asset  # ScriptableObject с библиотекой
│   │
│   ├── Data/                       # Данные (ScriptableObjects)
│   │   ├── Levels/                 # Уровни
│   │   │   ├── Level_001.asset
│   │   │   ├── Level_002.asset
│   │   │   └── ...
│   │   ├── LevelDatabase.asset     # База всех уровней
│   │   ├── ColoringImages/         # Данные раскрасок
│   │   │   ├── Coloring_01.asset
│   │   │   └── ...
│   │   ├── ColoringImageDatabase.asset
│   │   ├── GameSettings.asset      # Настройки игры
│   │   └── MoleculeTypes/          # Конфигурации типов молекул (если нужно)
│   │
│   ├── Fonts/                      # Шрифты (TextMeshPro)
│   │   └── MainFont SDF.asset
│   │
│   ├── Prefabs/                    # Prefabs
│   │   ├── Gameplay/
│   │   │   ├── Molecule.prefab     # Базовый префаб молекулы
│   │   │   └── Connection.prefab   # Префаб связи
│   │   ├── UI/
│   │   │   ├── Screens/            # UI экраны
│   │   │   ├── Components/         # Переиспользуемые UI компоненты
│   │   │   ├── Tooltips/
│   │   │   └── Popups/             # MiniPuzzlePopup и др
│   │   └── Managers/               # Менеджеры (если prefabs)
│   │       ├── GameManager.prefab
│   │       ├── AudioManager.prefab
│   │       └── ScannerManager.prefab
│   │
│   ├── Scenes/                     # Сцены
│   │   ├── Main.unity              # Вся логика в одной сцене
│   │
│   └── Scripts/                    # Код (C#)
│       ├── Core/                   # Ядро (не зависит от Unity)
│       │   ├── Data/               # Data structures
│       │   │   ├── MoleculeData.cs
│       │   │   ├── LevelData.cs
│       │   │   ├── GraphData.cs
│       │   │   └── ColoringImageData.cs
│       │   ├── Domain/             # Бизнес-логика
│       │   │   ├── Molecule.cs
│       │   │   ├── MoleculeGraph.cs
│       │   │   ├── GameRules.cs
│       │   │   └── TurnExecutor.cs
│       │   ├── Abilities/          # Абилки
│       │   │   ├── IAbility.cs
│       │   │   ├── HealNeighborsAbility.cs
│       │   │   └── TeleportAbility.cs
│       │   └── Properties/         # Пассивные свойства
│       │       ├── IPassiveProperty.cs
│       │       ├── ShieldProperty.cs
│       │       └── VirusProperty.cs
│       │
│       ├── Gameplay/               # Игровая логика (Unity-зависимая)
│       │   ├── GameManager.cs
│       │   ├── LevelController.cs
│       │   ├── TurnManager.cs
│       │   ├── GameStateMachine.cs
│       │   ├── ProgressionManager.cs
│       │   └── ScannerManager.cs
│       │
│       ├── GameModes/              # Режимы игры
│       │   ├── Endless/
│       │   │   ├── EndlessModeManager.cs
│       │   │   └── WaveGenerator.cs
│       │   └── Coloring/
│       │       ├── ColoringModeManager.cs
│       │       └── MiniPuzzleController.cs
│       │
│       ├── Presentation/           # Визуализация
│       │   ├── Views/
│       │   │   ├── MoleculeView.cs
│       │   │   ├── ConnectionView.cs
│       │   │   └── GraphView.cs
│       │   ├── Animation/
│       │   │   ├── MoleculeAnimator.cs
│       │   │   ├── ScannerEffects.cs
│       │   │   └── ScreenEffects.cs
│       │   └── Layout/
│       │       └── GraphLayouter.cs
│       │
│       ├── UI/                     # UI
│       │   ├── Screens/
│       │   │   ├── BaseScreen.cs
│       │   │   ├── MainMenuScreen.cs
│       │   │   ├── GameplayHUD.cs
│       │   │   ├── EndlessModeHUD.cs
│       │   │   ├── ColoringModeScreen.cs
│       │   │   └── ...
│       │   ├── Components/
│       │   │   ├── ColoringFragmentDot.cs
│       │   │   ├── MoleculeTooltip.cs
│       │   │   ├── ScannerButton.cs
│       │   │   ├── WaveTransition.cs
│       │   │   └── MiniPuzzlePopup.cs
│       │   └── UIManager.cs
│       │
│       ├── Data/                   # Persistence
│       │   ├── SaveManager.cs
│       │   ├── SaveData.cs
│       │   ├── LevelLoader.cs
│       │   └── ISerializer.cs
│       │
│       ├── Input/                  # Ввод
│       │   ├── InputManager.cs
│       │   └── TouchDetector.cs
│       │
│       ├── Audio/                  # Аудио
│       │   ├── AudioManager.cs
│       │   ├── MusicPlayer.cs
│       │   └── SFXPlayer.cs
│       │
│       ├── Monetization/           # Монетизация
│       │   ├── Ads/
│       │   │   ├── AdsManager.cs
│       │   │   └── AdsConfig.cs
│       │   ├── IAP/
│       │   │   ├── IAPManager.cs
│       │   │   └── IAPProduct.cs
│       │   └── ReviveManager.cs
│       │
│       ├── Analytics/              # Аналитика
│       │   ├── AnalyticsManager.cs
│       │   └── AnalyticsEvent.cs
│       │
│       └── Utilities/              # Утилиты
│           ├── Extensions/
│           │   ├── Vector2Extensions.cs
│           │   └── ColorExtensions.cs
│           ├── Helpers/
│           │   ├── CoroutineRunner.cs
│           │   └── MathUtils.cs
│           └── Debug/
│               └── DebugConsole.cs
│
├── Plugins/                        # Сторонние плагины
│   ├── Android/                    # Android-специфичные (если есть)
│   └── iOS/                        # iOS-специфичные (будущее)
│
└── ThirdParty/                     # SDK (Firebase, Ads, и т.д.)
    ├── GoogleMobileAds/
    └── Firebase/
```

---

## 🗓️ План реализации {#план-реализации-тз}

### Этап 0: Настройка проекта (1-2 дня)

**Задачи:**
1. Создать проект Unity 2022.3 LTS
2. Настроить URP
3. Установить TextMeshPro
4. Импортировать DOTween (Free)
5. Настроить структуру папок (см. выше)
6. Настроить Git (.gitignore для Unity)
7. Настроить Build Settings (Android, Portrait only)

**Результат:** Чистый проект, готовый к разработке

---

### Фаза 1: Прототип (2-3 недели)

#### Спринт 1.1: Core Logic (1 неделя)

**Задачи:**
1. **Data Structures:**
    - `MoleculeData` (ScriptableObject или POCO)
    - `LevelData` (ScriptableObject)
    - Создать 5 тестовых уровней (JSON или ScriptableObject)

2. **Domain Logic:**
    - `Molecule` класс (ID, value, neighbors, type)
    - `MoleculeGraph` класс (Add/Remove molecules, GetNeighbors)
    - `GameRules` (CheckWin, CheckLose)

3. **Turn Execution:**
    - `TurnExecutor` (ExecuteTurn method)
    - Логика: клик → абилка → удаление → декремент → пассивки → проверка win/lose
    - Только простой тип молекул (без абилок пока)

**Результат:** Работающая логика игры без визуала (можно протестировать через Unit тесты)

#### Спринт 1.2: Базовая визуализация (1 неделя)

**Задачи:**
1. **MoleculeView:**
    - Простой круг (Sprite или Mesh)
    - TextMeshPro для счетчика
    - Обработка клика (OnMouseDown)

2. **ConnectionView:**
    - LineRenderer между молекулами
    - Без анимации (просто статичные линии)

3. **GraphView:**
    - Создание MoleculeView для каждой молекулы в графе
    - Расстановка по позициям (из LevelData или простой layout)
    - Создание ConnectionView для всех связей

4. **GameManager (базовый):**
    - Загрузка уровня
    - Инициализация GraphView
    - Обработка кликов (через TurnManager)
    - Простой UI: кнопка "Restart", текст "Win/Lose"

**Результат:** Играбельный прототип с 5 уровнями (только простые молекулы)

#### Спринт 1.3: Полировка прототипа (3-5 дней)

**Задачи:**
1. **Анимации:**
    - Клик на молекулу: scale up → down
    - Исчезновение: fade out + простой particle explosion
    - Обновление счетчика: "-1" летит вверх

2. **Звуки (базовые):**
    - Клик
    - Исчезновение
    - Win/Lose

3. **Feedback:**
    - Дать поиграть 2-3 друзьям (не геймерам!)
    - Засеки время прохождения уровней
    - Спроси: "Когда было сложно? Когда скучно?"

**Результат:** Прототип с базовыми анимациями и звуками, готовый для тестирования

---

### Фаза 2: Кор-луп (2-3 недели)

#### Спринт 2.1: Система видимости + Сканер (1 неделя)

**Задачи:**
1. **Visibility System:**
    - `Molecule.IsVisible()` метод
    - `MoleculeView` скрывает/показывает значение в зависимости от IsVisible

2. **Scanner System:**
    - `ScannerManager` — управление сканером
    - Кнопка сканера в HUD
    - Режим выбора молекулы после клика на кнопку
    - Подсветка молекулы + соседей
    - Показ значений с анимацией
    - `ScannerEffects` — визуальные эффекты (волна, подсветка)

3. **Tutorial Levels:**
    - Создать уровни 1-3 (tutorial)
    - `TutorialManager` — показ подсказок (текст на экране)
    - Первое появление механики → подсказка

4. **Tooltip System:**
    - Зажатие молекулы → всплывающая подсказка с описанием типа
    - `MoleculeTooltip` UI компонент

**Результат:** Система видимости работает, сканер реализован, tutorial уровни обучают игрока

#### Спринт 2.2: Паразит + новые уровни (1 неделя)

**Задачи:**
1. **Parasite Type:**
    - Декремент зависит от количества соседей
    - Визуал: другой цвет (малиновый)
    - Обновить `Molecule` класс для поддержки кастомного декремента

2. **Levels 4-15:**
    - Создать 12 уровней (простые + паразиты)
    - Постепенное усложнение: больше молекул, сложнее графы
    - Уровень 5-6: обучение сканеру

3. **Экран раскраски (основная кампания):**
    - Экран с силуэтом рисунка, молекулы-точки = уровни
    - Клик на молекулу -> открывает уровень (головоломку)
    - При прохождении -> фрагмент рисунка открывается

**Результат:** 15 уровней, два типа молекул, экран раскраски, сканер

#### Спринт 2.3: Сохранения + UI (3-5 дней)

1. **Save System:**
    - `SaveManager` — сохранение прогресса
    - SaveData: пройденные уровни, открытые фрагменты раскрасок
    - Загрузка при старте игры

2. **UI Polish:**
    - Экран победы (кнопки)
    - Экран поражения (кнопка restart)
    - Главное меню (play button → экран раскраски)

**Результат:** сохранения, полноценный UI для основного лупа

---

### Фаза 3: Расширение контента (3-4 недели)

#### Спринт 3.1: Щит + Якорь (1 неделя)

**Задачи:**
1. **Abilities System:**
    - `IAbility` интерфейс
    - Якорь: абилка лечения соседей (+1 всем)
    - Обновить `TurnExecutor` для вызова абилок

2. **Shield Property:**
    - `IPassiveProperty` интерфейс
    - `ShieldProperty` — не уменьшается N ходов
    - Интеграция в `Molecule`

3. **Levels 16-25:**
    - Уровни с щитом и якорем
    - Обучение новым механикам (подсказки)

**Результат:** 4 типа молекул (простая, паразит, щит, якорь), 25 уровней

#### Спринт 3.2: Дополнительные типы молекул (опционально, 1 неделя)

**Задачи:**
1. **Virus Type:**
    - Пассивка: заражает соседей
    - Сложная логика, тестирование

2. **Teleport Type (опционально):**
    - Абилка: меняется местами с соседом
    - Визуал, анимации

3. **Levels 26-30:**
    - Уровни с новыми типами
    - Комбинации разных типов

**Результат:** 30 уровней, 5-6 типов молекул

#### Спринт 3.3: Визуал + анимации (1-2 недели)

**Задачи:**
1. **Molecule Visuals:**
    - Неоновые материалы (Glow shader)
    - Пульсация (scale animation)
    - Цвета для каждого типа

2. **Connection Animations:**
    - Shader для волн на связях
    - Цветовое смещение

3. **Scanner Visuals:**
    - Волна расходится от молекулы
    - Подсветка молекулы + соседей
    - Анимация показа значений

4. **VFX:**
    - Частицы при клике (цвет = цвет молекулы)
    - Конфетти на победе
    - Screen flash на поражении

5. **Audio:**
    - Музыка (main theme, gameplay theme)
    - SFX для каждого типа молекулы
    - SFX для сканера
    - UI sounds

**Результат:** Полный визуал и звук, игра выглядит законченной

---

### Фаза 4: Контент кампании + бесконечный режим (2-3 недели)

#### Спринт 4.1: Полноценная кампания-раскраска (1-2 недели)

> Раскраска — это основная кампания игры. Уровни, созданные в Фазах 1-3, привязываются к молекулам-точкам на рисунках.

**Задачи:**
1. **Coloring Mode (основная кампания):**
    - `ColoringModeManager` — управление раскраской
    - `ColoringImage` ScriptableObject (силуэт, финальный арт, позиции фрагментов)
    - Привязка уровней (LevelData) к фрагментам раскрасок
    - Создать 3-5 раскрасок (по 5-10 уровней в каждой = 20-30 уровней)

2. **Прогресс и галерея:**
    - Сохранение прогресса раскраски (открытые фрагменты)
    - `ColoringGalleryScreen` — галерея завершенных рисунков
    - Переход к следующей раскраске при завершении

3. **UI:**
    - `ColoringModeScreen` — главный экран кампании
    - Показ силуэта с молекулами-точками (каждая = уровень)
    - Прогресс (X/Y фрагментов открыто)

**Результат:** Полноценная кампания из 3-5 раскрасок, каждая с уникальным рисунком

#### Спринт 4.2: Бесконечный режим (1 неделя)

**Задачи:**
1. **Endless Mode Logic:**
    - `EndlessModeManager` — волновая система
    - `WaveGenerator` — генерация графа для волны
    - Усложнение: +2 молекулы, +1 к значениям, новые типы каждые 5 волн
    - Переход между волнами (пауза 2 сек, анимация)

2. **Leaderboard:**
    - Локальный рекорд (сохраняется в SaveData)
    - Интеграция Google Play Games Services (опционально)

3. **UI:**
    - `EndlessModeHUD` — показ волны, рекорда
    - `WaveTransition` — анимированный текст "Волна X"
    - Экран поражения с достигнутой волной

**Результат:** Рабочий бесконечный режим

---

### Фаза 5: Монетизация и полировка (1-2 недели)

#### Спринт 5.1: Реклама (3-5 дней)

**Задачи:**
1. **AdMob Integration:**
    - Импортировать Google Mobile Ads SDK
    - Настроить Ad Unit IDs (test и production)
    - `AdsManager` — инициализация, показ Interstitial, Rewarded Video

2. **Interstitial Ads:**
    - Показ каждые 2-3 уровня
    - Логика частоты (не чаще 1 раза в минуту)

3. **Rewarded Video:**
    - Ревайв при поражении

**Результат:** Работающая реклама

#### Спринт 5.2: IAP + ревайв (3-5 дней)

**Задачи:**
1. **Unity IAP Integration:**
    - Импортировать Unity IAP
    - Настроить продукты в Google Play Console
    - `IAPManager` — purchase flow

2. **"Remove Ads" IAP:**
    - Покупка отключает Interstitial
    - Сохранение статуса покупки

3. **Revive System:**
    - `ReviveManager` — предложение ревайва
    - Логика восстановления (+2 ко всем молекулам)
    - UI экран ревайва

**Результат:** IAP и ревайв работают

#### Спринт 5.3: Полировка (3-5 дней)

**Задачи:**
1. **Balancing:**
    - Тестирование всех уровней
    - Балансировка частоты рекламы
    - Балансировка бесконечного режима (сложность волн)
    - Балансировка раскраски (сложность мини-головоломок)

2. **Bug Fixing:**
    - Исправление багов из тестирования

3. **UI/UX:**
    - Финальная полировка интерфейса
    - Transitions между экранами
    - Feedback (vibration на Android при клике — опционально)

4. **Settings Screen:**
    - Звук, музыка (слайдеры)
    - Подсказки (вкл/выкл)
    - Сброс прогресса

5. **Glossary:**
    - Экран глоссария с описанием всех типов молекул
    - Описание сканера

**Результат:** Игра готова к закрытому бета-тесту

---

### Фаза 6: Тестирование и оптимизация (2-3 недели)

#### Спринт 6.1: Closed Beta (1 неделя)

**Задачи:**
1. **Подготовка:**
    - Собрать APK
    - Загрузить в Google Play (Internal Testing track)
    - Пригласить 10-20 тестеров (друзья, знакомые)

2. **Сбор фидбека:**
    - Форма обратной связи (Google Form)
    - Вопросы:
        - Какие уровни слишком сложные/легкие?
        - Понятен ли сканер? Часто ли используется?
        - Интересен ли бесконечный режим?
        - Понятна ли раскраска?
        - Где непонятно?
        - Есть ли баги?
        - Раздражает ли реклама?

3. **Аналитика:**
    - Настроить Firebase Analytics
    - Отслеживать:
        - Completion rate уровней
        - Где игроки застревают
        - Частота использования сканера
        - Средняя волна в бесконечном режиме
        - Completion rate раскрасок
        - Session length

**Результат:** Собран фидбек от тестеров

#### Спринт 6.2: Итерация по фидбеку (1 неделя)

**Задачи:**
1. **Балансировка:**
    - Упростить/усложнить уровни по фидбеку
    - Балансировка бесконечного режима (если слишком сложно/легко)
    - Балансировка раскраски (сложность мини-головоломок)

2. **Фиксы:**
    - Исправить баги, найденные тестерами
    - Улучшить UX (если были жалобы)
    - Улучшить обучение сканеру (если непонятно)

3. **Оптимизация:**
    - Профилирование (Unity Profiler)
    - Оптимизация узких мест (если FPS <60)
    - Уменьшение APK size (сжатие текстур, аудио)

**Результат:** Игра отполирована по фидбеку

#### Спринт 6.3: Подготовка к релизу (3-5 дней)

**Задачи:**
1. **Store Assets:**
    - Иконка приложения (512x512, круглая для Android)
    - Скриншоты (минимум 2, рекомендуется 4-8)
        - Основной геймплей
        - Сканер в действии
        - Бесконечный режим
        - Режим раскраски
    - Feature Graphic (1024x500)
    - Описание (краткое и полное)
    - Промо-видео (опционально, 30 сек)

2. **Legal:**
    - Privacy Policy (обязательно, если собираешь данные/реклама)
    - Terms of Service (опционально)

3. **Final Build:**
    - Собрать release APK (или AAB)
    - Проверить на разных устройствах
    - Убедиться, что production Ad IDs используются

**Результат:** Все готово к публикации

---

### Фаза 7: Релиз (1 неделя)

#### День 1-2: Публикация

**Задачи:**
1. Загрузить APK/AAB в Google Play Console
2. Заполнить Store Listing (описание, скриншоты)
3. Настроить ценообразование (бесплатно + IAP)
4. Отправить на ревью

**Результат:** Игра на ревью в Google Play

#### День 3-7: Мониторинг и hotfix

**Задачи:**
1. **Мониторинг:**
    - Отслеживать аналитику (installs, retention, crashes)
    - Читать отзывы в Google Play
    - Мониторить:
        - Использование сканера
        - Популярность бесконечного режима
        - Completion rate раскрасок

2. **Hotfix (если нужно):**
    - Исправить критические баги
    - Выпустить hotfix версию

3. **Marketing (опционально):**
    - Пост в соцсетях
    - Попросить друзей оставить отзывы

**Результат:** Игра живая, работает стабильно

---

## ⚡ Оптимизация и производительность {#оптимизация-и-производительность-тз}

### Целевые метрики

- **FPS:** 60 на средних устройствах, 30+ на слабых
- **APK Size:** <50 МБ
- **RAM:** <400 МБ
- **Battery:** Минимальное потребление (оптимизация GPU/CPU)

### Стратегии оптимизации

#### 1. Rendering

**A) URP Optimization:**
- Использовать URP 2D Renderer (легче чем 3D)
- Отключить ненужные фичи (Shadows, Post-Processing если не используется)
- Batch спрайты (Sprite Atlas)

**Б) Draw Calls:**
- Минимизировать количество материалов (использовать Sprite Atlas)
- Batching: Static Batching для UI, Dynamic Batching для молекул (если возможно)
- Target: <50 draw calls на gameplay сцене

**В) Overdraw:**
- Избегать наложения прозрачных объектов
- Использовать Occlusion Culling (если граф большой)

#### 2. Scripting

**A) Object Pooling:**
- Пулы для:
    - Частицы (Particle Systems)
    - AudioSource (SFX)
    - MoleculeView (если переиспользуются между уровнями)
    - MiniPuzzlePopup (раскраска)

**Б) Avoid Allocations:**
- Избегать `new` в Update/FixedUpdate
- Переиспользовать коллекции (List, Dictionary)
- Использовать `StringBuilder` вместо `string +`

**В) Update Optimization:**
- Минимум логики в `Update()`
- Использовать корутины для длительных операций
- Избегать `Find()`, `GetComponent()` в Update (кешировать ссылки)

#### 3. Assets

**A) Textures:**
- Формат: ASTC (Android), ETC2 fallback
- Размеры: степени двойки (512x512, 1024x1024)
- Mipmap: выключен для UI, включен для игровых объектов
- Compression: максимальная (качество vs размер)

**Б) Audio:**
- Формат: MP3
- Музыка: Streaming (не загружать в RAM)
- SFX: Decompress On Load (короткие) или Compressed In Memory (длинные)
- Битрейт: 128 kbps (музыка), 96 kbps (SFX)

### В) Shaders:

- Использовать простые шейдеры (избегать сложных calculations)
- Shader Graph: оптимизировать граф (минимум нод)

#### 4. Memory

**A) Managed Heap:**
- Target: <200 МБ
- Избегать частых allocations (GC spikes = лаги)
- Профилировать через Memory Profiler

**Б) Unloading:**
- Выгружать assets между уровнями (`Resources.UnloadUnusedAssets()`)

#### 5. APK Size

**A) Compression:**
- Enable "Split APKs by target architecture" (armeabi-v7a, arm64-v8a)
- Stripping Level: Medium или High
- IL2CPP вместо Mono (меньше размер)

**Б) Asset Optimization:**
- Удалить неиспользуемые assets
- Сжать текстуры/аудио
- Не использовать встроенные Unity assets (Standard Assets и т.д.)

**В) Code Optimization:**
- Code Stripping: Enabled
- Managed Stripping Level: Medium

### Профилирование

**Инструменты:**
- **Unity Profiler** — CPU, GPU, Memory, Rendering
- **Frame Debugger** — анализ draw calls
- **Memory Profiler** — поиск утечек памяти

**Тестирование на устройствах:**
- Минимум 3 устройства:
    - Слабое (2019 год, ~2GB RAM)
    - Среднее (2021 год, 4GB RAM)
    - Флагман (2023 год, 8GB RAM)

---

## 🔒 Безопасность и Anti-Cheat

**Для релиза (минимум):**
- Обфускация кода (IL2CPP + высокий stripping level)
- Не хранить критичные данные в PlayerPrefs (легко читаются)

**Опционально (для апдейтов):**
- Anti-Cheat Toolkit (Asset Store) — ObscuredPrefs для сохранений
- Server-side validation для IAP

---

## 📊 Аналитика (детально)

### Ключевые метрики

**Retention:**
- Day 1: >40% (хорошо), >50% (отлично)
- Day 7: >20% (хорошо), >30% (отлично)
- Day 30: >10% (хорошо), >15% (отлично)

**Session:**
- Средняя длина: 5-10 минут (казуальная игра)
- Частота сессий: 1-3 раза в день

**Monetization:**
- ARPU (Average Revenue Per User): >$0.05 для рекламы, >$0.10 с IAP
- Ad fill rate: >90%
- IAP conversion: >1% (хорошо), >3% (отлично)

### События для отслеживания

**Progression:**
- `level_start(level_id, attempt_number)`
- `level_complete(level_id, moves, time_seconds)`
- `level_fail(level_id, attempts, reason)` (reason: timeout, molecule_died)
- `tutorial_step(step_number, completed)`

**Monetization:**
- `ad_request(type, placement)`
- `ad_impression(type, placement, revenue)`
- `ad_click(type, placement)`
- `ad_failed(type, placement, error)`
- `revive_offered(level_id)`
- `revive_accepted(level_id)` / `revive_declined(level_id)`
- `iap_purchase_started(product_id)`
- `iap_purchase_completed(product_id, price, currency)`
- `iap_purchase_failed(product_id, error)`

**Engagement:**
- `endless_mode_start()`
- `endless_mode_end(wave_reached, molecules_killed)`
- `coloring_mode_start(image_id)`
- `coloring_mode_fragment_unlocked(image_id, fragment_id)`
- `coloring_mode_complete(image_id, time_seconds)`

**Churn Points:**
- Отслеживать на каких уровнях игроки застревают (completion rate <30%)

---

## ✅ Чеклист перед релизом

### Технический

- [ ] Все уровни протестированы (проходимы)
- [ ] Нет критичных багов
- [ ] FPS >30 на слабых устройствах
- [ ] APK size <50 МБ
- [ ] Crash rate <1% (протестировать на разных устройствах)
- [ ] Реклама работает (test ads заменены на production)
- [ ] IAP работает (протестировать покупку и restore)
- [ ] Аналитика настроена и отправляет события
- [ ] Сохранения работают (прогресс не теряется)
- [ ] Звуки/музыка работают, можно отключить в настройках
- [ ] Safe Area учтен (для notch-экранов)
- [ ] Back button (Android) работает корректно

### Legal & Store

- [ ] Privacy Policy создана и доступна (обязательно для AdMob)
- [ ] Иконка приложения (512x512)
- [ ] Скриншоты (минимум 2, рекомендуется 4-8)
- [ ] Feature Graphic (1024x500)
- [ ] Описание написано (краткое и полное)
- [ ] Возрастной рейтинг указан (скорее всего 3+)
- [ ] Категория выбрана (Puzzle)
- [ ] Контент рейтинг заполнен (в Google Play Console)

### QA

- [ ] Closed Beta проведен (10+ тестеров)
- [ ] Фидбек учтен, баги исправлены
- [ ] Протестировано на 3+ разных устройствах
- [ ] Retention Day 1 >30% (в beta)
- [ ] Нет негативных отзывов о сложности/багах

---

**Версия документа:** 1.1
**Дата создания:** 2026-02-16
**Последнее обновление:** 2026-02-16
**Статус:** In Progress — Фаза 0 (частично завершена)
