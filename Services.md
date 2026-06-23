# Services

Сервисный слой приложения. Содержит всю бизнес-логику, работу с внешними API и локальным хранилищем. Все сервисы регистрируются как синглтоны в `MauiProgram.cs` и внедряются через DI.

---

## SteamApiService.cs

Интеграция с Steam Web API для поиска модов RimWorld.

### Модели

**`SteamMod`** — данные одного мода из Steam Workshop:
- `PublishedFileId` — уникальный ID файла в Workshop
- `Title` — название мода
- `PreviewUrl` — URL превью-изображения
- `Description` — короткое описание (до 120 символов, HTML очищен)
- `Tags` — теги из Workshop
- `Compatibility` — совместимость с версией игры (по умолчанию `"1.5 / 1.6"`)
- `Url` (вычислимое) — ссылка на страницу в Steam

### Методы

**`SearchModsAsync(string query)`**

Отправляет запрос к `IPublishedFileService/QueryFiles/v1` с параметрами:
- `appid=294100` (RimWorld)
- `numperpage=20`
- `return_previews`, `return_metadata`, `return_short_description`, `return_tags` = true

Возвращает `List<SteamMod>`. HTML-теги из описания удаляются через Regex. При пустом запросе подставляется `"rimworld"`. При любой ошибке сети возвращает пустой список.

> ⚠️ API-ключ хранится в константе `ApiKey` прямо в коде. Перед публикацией вынеси его в конфигурацию или переменную окружения.

---

## GithubService.cs

Загрузка модов из GitHub-организации [Vanilla-Expanded](https://github.com/Vanilla-Expanded) с последующим обогащением данными из Steam.

### Модели

**`VanillaMod`** — мод из GitHub:
- `Name` — имя репозитория
- `Description` — описание репозитория (или Steam-описание после обогащения)
- `HtmlUrl` — ссылка на репозиторий
- `PreviewUrl` — URL превью (по умолчанию `"no_image.jpg"`, обновляется из Steam)
- `Stars` — количество звёзд репозитория
- `IsEnriched` — флаг, что данные из Steam уже подгружены
- `Compatibility` — `"1.5 / 1.6"`

### Методы

**`GetVanillaModsAsync()`**

Обходит все страницы репозиториев организации (по 30 штук) через `GET /orgs/Vanilla-Expanded/repos`. Возвращает список, отсортированный по убыванию звёзд.

**`EnrichWithSteamAsync(List<VanillaMod> mods, Action<VanillaMod> onUpdated)`**

Обрабатывает первые 40 модов батчами по 5 с задержкой 600 мс между батчами (чтобы не превысить rate limit Steam). Для каждого мода ищет соответствие в Steam по имени и обновляет `PreviewUrl` и `Description`. Вызывает `onUpdated` после каждого обогащённого мода — UI обновляется постепенно.

---

## StorageService.cs

Локальное хранилище для избранного и истории поиска. Данные хранятся в JSON-файлах в `FileSystem.AppDataDirectory`.

### Файлы
- `favorites.json` — список `FavoriteItem`
- `history.json` — список строк (названия запросов)

### События
- `FavoritesChanged` — срабатывает после каждого сохранения избранного. `FavoritesViewModel` подписывается и перезагружает список.

### Методы

| Метод | Описание |
|---|---|
| `LoadFavoritesAsync()` | Десериализует `favorites.json`. Поддерживает обратную совместимость со старым форматом `List<string>` |
| `SaveFavoritesAsync(List<FavoriteItem>)` | Сериализует и сохраняет, затем вызывает `FavoritesChanged` |
| `LoadHistoryAsync()` | Читает список строк из `history.json` |
| `AddToHistoryAsync(string)` | Добавляет запрос в начало, удаляет дубликат если есть, обрезает до 20 записей |
| `ClearHistoryAsync()` | Записывает `[]` в файл истории |

---

## ModFolderService.cs (ModModelService.cs)

Сканирование локальной папки с модами RimWorld и парсинг их метаданных.

### Модели

**`ModInfo`** — данные установленного мода:
- `Name`, `Author`, `Version`, `FolderName`
- `Tags` — авто-назначенные теги
- `IsCompatibleWithCurrentGame` — `true`, если версия содержит `"1.5"` или `"1.6"` (или не указана)

### Методы

**`GetSavedFolderPath()` / `SaveFolderPath(string)`**

Читают и записывают путь к папке через `Preferences` по ключу `"mods_folder_path"`.

**`ScanFolder(string folderPath)`**

Перебирает поддиректории. Для каждой проверяет наличие `About/About.xml`, парсит значения тегов `<name>`, `<author>`, `<targetVersion>` через `IndexOf` (без LINQ-зависимостей). Вызывает `AutoAssignTags` и возвращает список, отсортированный по имени.

**`AutoAssignTags(string name, string author, string xml)`** *(private)*

Присваивает теги по Regex-шаблонам на конкатенации всех трёх строк:

| Тег | Паттерн |
|---|---|
| Performance | `performance\|optimization\|fps\|lag` |
| QoL | `qol\|quality of life\|ui\|interface\|menu` |
| Graphics | `graphic\|texture\|visual\|shader` |
| Overhaul | `overhaul\|rewrite\|big update\|expansion` |
| Combat | `combat\|weapon\|fight` |
| Vanilla Expanded | `vanilla expanded\|ve-` |
| Gameplay | `storyteller\|difficulty\|raid` |

Если ни один тег не подошёл — присваивается `"Other"`.

---

## ThemeService.cs

Управление темой оформления: тёмная/светлая и четыре акцентных цвета.

### Перечисления
- `AppColorTheme` — `Dark`, `Light`
- `AccentColor` — `Green`, `Orange`, `Red`, `Pink`

### Хранение
Настройки сохраняются в `Preferences` по ключам `"app_theme"` и `"app_accent"` и восстанавливаются при следующем запуске.

### Событие
`ThemeChanged` — Action, вызывается после `SetTheme` и `SetAccent`. `AppShell` и все страницы подписываются и перекрашивают UI.

### Цветовые методы

Фоновые цвета зависят от `CurrentTheme`:

| Метод | Dark | Light |
|---|---|---|
| `GetBackground()` | `#0A0A0A` | `#F5F5F5` |
| `GetCard()` | `#141414` | `#FFFFFF` |
| `GetInput()` | `#1E1E1E` | `#EEEEEE` |
| `GetTextPrimary()` | White | Black |
| `GetTextMuted()` | `#AAAAAA` | `#555555` |

Акцентные цвета определяются по палитре `Palettes`:

| AccentColor | Primary | Secondary | Light | Dark |
|---|---|---|---|---|
| Green | `#00C853` | `#00FF7F` | `#6FFFB0` | `#00692E` |
| Orange | `#E67300` | `#FF8C00` | `#FFB347` | `#7A3D00` |
| Red | `#D6291C` | `#FF3B3B` | `#FF8080` | `#7A1410` |
| Pink | `#D90073` | `#FF00FF` | `#FF66FF` | `#80003D` |

`GetAccentFaded()` возвращает основной акцент с прозрачностью 10% — используется для фоновых подсветок карточек.

---

## LinkService.cs

Вспомогательный сервис для генерации поисковых URL.

| Метод | Результат |
|---|---|
| `GetSteamUrl(string modName)` | `https://steamcommunity.com/workshop/browse/?appid=294100&searchtext=<encoded>` |
| `GetNexusUrl(string modName)` | `https://www.nexusmods.com/rimworld/search/?gsearch=<encoded>` |

`modName` кодируется через `Uri.EscapeDataString`.
