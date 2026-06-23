# Pages

Экраны приложения. Каждая страница наследует `ContentPage` и получает зависимости через конструктор (primary constructor с DI) + резервный пустой конструктор для создания через Shell XAML. `BindingContext` устанавливается в конструкторе.

---

## MainPage.xaml / MainPage.xaml.cs

Главный экран: поиск Steam Workshop, история запросов, добавление в избранное.

### Зависимости
`MainViewModel`, `ThemeService`, `ThemeViewModel`, `SteamApiService`, `LinkService`

### Список категорий
Предопределён в коде: `"Все категории"`, `QoL`, `Gameplay`, `Graphics`, `Performance`, `Combat`, `Overhaul`, `Vanilla Expanded`, `UI`, `Weapons`, `Animals`, `Buildings`, `Storyteller`. Отображается через `Picker`.

### Логика поиска (`PerformSearch`)

1. Если запрос и категория пусты — показывает подсказку
2. Если есть запрос — вызывает `SteamApiService.SearchModsAsync(query)`
3. Если выбрана категория (не "Все") — дополнительно фильтрует результаты по наличию категории в `Title`, `Description` или `Tags`
4. Добавляет результаты в `MainViewModel.SteamResults` и обновляет лейбл счётчика

### Обработчики событий

| Обработчик | Действие |
|---|---|
| `OnFindButtonClicked` | Анимация + `PerformSearch` |
| `OnCategoryChanged` | `PerformSearch` при смене пикера |
| `OnSteamClicked` | Открывает Steam URL из `CommandParameter` |
| `OnNexusClicked` | Анимация + открывает Nexus через `LinkService` |
| `OnAddToFavoritesClicked` | Анимация + `MainViewModel.AddToFavoritesCommand` |
| `OnThemeClicked` | `DisplayActionSheet` с выбором темы и акцента, применяет через `ThemeService` |

При инициализации сразу вызывает `MainViewModel.RefreshRadarCommand` для загрузки рекомендаций на главный экран.

---

## FavoritesPage.xaml / FavoritesPage.xaml.cs

Список сохранённых модов с управлением.

### Зависимости
`FavoritesViewModel`, `ThemeService`, `ThemeViewModel`

### Обработчики событий

| Обработчик | Действие |
|---|---|
| `OnSteamClicked` | Анимация + `FavoritesViewModel.OpenSteamCommand` |
| `OnNexusClicked` | Анимация + `FavoritesViewModel.OpenNexusCommand` |
| `OnRemoveFavoriteClicked` | Анимация + `FavoritesViewModel.RemoveCommand` |

Все три получают `FavoriteItem` из `Button.CommandParameter`.

---

## MyModsPage.xaml / MyModsPage.xaml.cs

Экран локальных модов. Позволяет выбрать папку и просматривать установленные моды с тегами.

### Зависимости
`MyModsViewModel`, `ThemeService`, `ThemeViewModel`

### Обработчики событий

| Обработчик | Действие |
|---|---|
| `OnSelectFolderClicked` | Анимация + `MyModsViewModel.PickFolderCommand` |

Фильтрация по тегам и Pull-to-Refresh реализованы в XAML через биндинги к `MyModsViewModel`.

---

## VanillaPage.xaml / VanillaPage.xaml.cs

Список модов организации Vanilla Expanded с постепенной загрузкой картинок.

### Зависимости
`GithubService`, `LinkService`, `ThemeViewModel`

### Поля состояния
- `_allMods` — `ObservableCollection<VanillaMod>`, базовый список для фильтрации
- `_searchTimer` — `System.Timers.Timer` для debounce-поиска

### Логика загрузки (`LoadAsync`)

1. Показывает skeleton-лоадер (`LoadingContainer`)
2. Загружает список из `GithubService.GetVanillaModsAsync()`
3. Отображает список
4. После отображения в фоне запускается `GithubService.EnrichWithSteamAsync` — каждый обогащённый мод обновляется в коллекции через `onUpdated` callback

### Debounce-поиск

`OnVanillaSearchChanged` сбрасывает и перезапускает `Timer` на 350 мс. По истечении на главном потоке вызывается `ApplyFilter`, которая устанавливает `ItemsSource` в отфильтрованный список или восстанавливает `_allMods`.

### Обработчики событий

| Обработчик | Действие |
|---|---|
| `OnVanillaRefreshing` | Pull-to-Refresh → `LoadAsync` |
| `OnVanillaSearchChanged` | Запуск debounce-таймера |
| `OnGithubClicked` | Открывает GitHub URL из `CommandParameter` |

---

## RadarPage.xaml / RadarPage.xaml.cs

Самый насыщенный экран. Содержит пять независимых секций, каждая загружается параллельно в `Task.Run` без блокировки UI.

### Зависимости
`MainViewModel`, `ThemeViewModel`, `SteamApiService`, `StorageService`, `LinkService`

### Секция 1: Мод дня

**Поля:** `_modOfDay` — текущий мод

**`LoadModOfDayAsync(string? category)`** — выбирает случайную категорию из `_randomCategories`, запрашивает Steam, берёт случайный мод из результатов. Показывает `ActivityIndicator` пока идёт загрузка.

| Обработчик | Действие |
|---|---|
| `OnRerollClicked` | Перезагружает мод дня |
| `OnModDaySteamClicked` | Открывает URL мода в Steam |
| `OnModDayFavClicked` | Добавляет в избранное через `StorageService`, обновляет `FavoritesCount` |

### Секция 2: ДНК колонии

**`LoadDnaAsync()`** — анализирует избранное по шести категориям через Regex:

| Ключ | Паттерны |
|---|---|
| combat | `combat\|weapon\|fight\|gun\|rimmu\|yayo` |
| qol | `qol\|ui\|menu\|hud\|interface\|dubs\|rimhud` |
| graphics | `graphic\|texture\|visual\|shader\|wall light` |
| performance | `performance\|fish\|thread\|fps\|optim` |
| vanilla | `vanilla expanded\|ve-\|oskar` |
| overhaul | `overhaul\|rewrite\|expansion\|big` |

Результаты выводятся в прогрессбары. Ведущая категория определяет архетип игрока (текстовая метка).

### Секция 3: Мод vs Мод

**Поля:** `_vsModA`, `_vsModB`, `_vsWinner`

**`LoadVsModsAsync()`** — загружает два случайных мода из двух случайных категорий.

**`HandleVoteAsync(SteamMod?)`** — победитель автоматически добавляется в избранное, показывается результат, кнопки голосования блокируются.

| Обработчик | Действие |
|---|---|
| `OnVoteAClicked` / `OnVoteBClicked` | Фиксирует голос, добавляет победителя в избранное |
| `OnVsRerollClicked` | Перезагружает пару модов |
| `OnVsWinnerSteamClicked` | Открывает победителя в Steam |

### Секция 4: Квест дня

**`LoadQuestAsync()`** — выбирает квест из 6 вариантов по сиду `год * 1000 + номерДняВГоду` (квест одинаков весь день для всех). Прогресс считается по количеству подходящих модов в избранном. При выполнении — показывает `QuestAchieveFrame`.

### Секция 5: Популярные по категории

**`LoadPopularAsync(string category)`** — запрашивает Steam, результаты заполняют `CollectionView`. `ActivityIndicator` показывается до завершения.

| Обработчик | Действие |
|---|---|
| `OnPopularCategoryChanged` | Перезагружает список при смене `Picker` |
| `OnRefreshPopularClicked` | Ручное обновление |
| `OnPopularSteamClicked` | Открывает мод в Steam |
| `OnPopularFavClicked` | Добавляет мод в избранное |
| `OnBuildSteamClicked` | Открывает поиск по запросу в Steam Workshop |

### Вспомогательная модель

**`DailyQuest`** (определена в конце файла):
- `Title`, `Desc` — заголовок и описание квеста
- `Target` — целевое количество модов
- `Category` — категория для подсчёта прогресса (`"any"` = любые)
