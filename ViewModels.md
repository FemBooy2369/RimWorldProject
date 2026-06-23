# ViewModels

Слой представления в MVVM-архитектуре. Все ViewModel реализуют `INotifyPropertyChanged` вручную (без сторонних фреймворков). Команды используют встроенный `Microsoft.Maui.Controls.Command`.

---

## MainViewModel.cs

Центральная ViewModel. Обслуживает `MainPage` (поиск Steam) и предоставляет агрегированные данные (`FavoritesCount`, `MyModsCount`) для дашборда.

### Зависимости
`LinkService`, `StorageService`, `SteamApiService`, `ModFolderService`

### Свойства

| Свойство | Тип | Описание |
|---|---|---|
| `ModName` | `string` | Текущий поисковый запрос |
| `IsLoading` | `bool` | Идёт ли запрос к Steam |
| `HistoryVisible` | `bool` | Показана ли панель истории |
| `ResultsVisible` | `bool` | Есть ли результаты поиска |
| `FavoritesCount` | `int` | Количество элементов в избранном |
| `MyModsCount` | `int` | Количество установленных модов |
| `RadarCount` | `int` | Количество рекомендаций в Radar |

### Коллекции

- `History` — `ObservableCollection<string>`, последние 20 поисков
- `SteamResults` — `ObservableCollection<SteamMod>`, результаты текущего поиска
- `ModRadar` — `ObservableCollection<ModRecommendation>`, рекомендации на главной

### Команды

| Команда | Действие |
|---|---|
| `GenerateCommand` | Добавляет `ModName` в историю, запускает поиск Steam |
| `AddToFavoritesCommand<SteamMod>` | Добавляет мод в `favorites.json`, обновляет Radar |
| `SelectHistoryCommand<string>` | Устанавливает `ModName` и запускает `GenerateCommand` |
| `ClearHistoryCommand` | Очищает файл истории и коллекцию |
| `ToggleHistoryCommand` | Переключает видимость панели истории |
| `OpenSteamModCommand<SteamMod>` | Открывает URL мода в браузере |
| `OpenNexusModCommand<SteamMod>` | Открывает Nexus Mods в браузере |
| `RefreshRadarCommand` | Перезагружает рекомендации |

### Логика рекомендаций (`LoadModRadarAsync`)

Алгоритм:
1. Загружает избранное и сканирует папку с модами
2. Обновляет `FavoritesCount` и `MyModsCount`
3. Фильтрует локальные моды, которых ещё нет в избранном
4. Для каждого вычисляет `MatchPercent` — процент совпадения по подстроке названия с каждым избранным (базовое значение 40%, +18% за каждое совпадение, максимум 95%)
5. Определяет `Reason` по тегам мода
6. Берёт топ-6 по `MatchPercent` и заполняет `ModRadar`

### Модели

**`ModRecommendation`**
- `Title` — название мода
- `MatchPercent` — процент совместимости
- `Reason` — причина рекомендации (строка)
- `SteamUrl` — ссылка для поиска в Steam

---

## FavoritesViewModel.cs

Управление списком избранных модов.

### Зависимости
`StorageService`, `LinkService`

### Коллекции
- `Favorites` — `ObservableCollection<FavoriteItem>`, отображается в `FavoritesPage`

### Команды

| Команда | Действие |
|---|---|
| `LoadCommand` | Загружает список из `StorageService` в коллекцию |
| `RemoveCommand<FavoriteItem>` | Удаляет элемент из JSON и перезагружает |
| `OpenSteamCommand<FavoriteItem>` | Открывает Steam Workshop поиск по названию |
| `OpenNexusCommand<FavoriteItem>` | Открывает Nexus Mods поиск по названию |

Подписывается на `StorageService.FavoritesChanged` — при любом изменении избранного (в том числе из Radar или поиска) автоматически вызывает `ReloadFavoritesAsync` на главном потоке.

---

## MyModsViewModel.cs

Управление локальными модами RimWorld.

### Зависимости
`ModFolderService`

### Свойства

| Свойство | Тип | Описание |
|---|---|---|
| `FolderPath` | `string?` | Путь к папке с модами |
| `HasFolder` | `bool` | `true`, если путь выбран (управляет видимостью секций в UI) |

### Коллекции
- `Mods` — `ObservableCollection<ModInfo>`, отсканированные моды

### Команды

| Команда | Действие |
|---|---|
| `PickFolderCommand` | Windows: открывает `FolderPicker`, сохраняет путь, сканирует папку |
| `RefreshCommand` | Повторно сканирует текущую папку |

`PickFolderCommand` использует `#if WINDOWS` для инициализации `FolderPicker` через WinRT Interop — на Android заглушка.

---

## ThemeViewModel.cs

Реактивная обёртка над `ThemeService` для биндингов в XAML.

### Зависимости
`ThemeService`

### Свойства (только чтение)

Все свойства возвращают `Color` из `ThemeService`:

`Accent`, `AccentSecondary`, `AccentLight`, `AccentDark`, `AccentFaded`, `Background`, `Card`, `CardAlt`, `Input`, `TextPrimary`, `TextMuted`, `Border`, `BtnText`, `MenuBg`, `MenuSelected`

При получении события `ThemeService.ThemeChanged` вызывает `NotifyAll()` на главном потоке — все привязки в XAML обновляются одновременно без ручного вызова на каждой странице.

Регистрируется как **синглтон** в DI, поэтому один экземпляр `ThemeViewModel` шарится между всеми страницами через `BindingContext`.
