# Models

Модели данных, используемые для хранения и передачи информации между слоями приложения.

---

## FavoriteItem.cs

Представляет один сохранённый мод в списке избранного.

```csharp
public class FavoriteItem
{
    public string Title { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
}
```

### Поля

| Поле | Описание |
|---|---|
| `Title` | Название мода — используется как уникальный идентификатор при проверке дублей и открытии ссылок |
| `PreviewUrl` | URL превью-изображения из Steam. Может быть пустой строкой, в XAML в таком случае подставляется `no_image.jpg` |

### Хранение

Сериализуется в `favorites.json` через `System.Text.Json`. `StorageService` поддерживает обратную совместимость со старым форматом `List<string>` (только названия).

### Использование

- `StorageService` — читает и записывает `List<FavoriteItem>`
- `FavoritesViewModel.Favorites` — `ObservableCollection<FavoriteItem>` для отображения
- `MainViewModel` и `RadarPage` — создают экземпляры при добавлении из поиска или Radar

---

## SteamMod

*Определена в `Services/SteamApiService.cs`*

Представляет мод из Steam Workshop, полученный через API.

| Поле | Описание |
|---|---|
| `PublishedFileId` | Уникальный ID в Workshop |
| `Title` | Название |
| `PreviewUrl` | URL изображения |
| `Description` | Очищенное от HTML описание (до 120 символов) |
| `Compatibility` | Версия игры (`"1.5 / 1.6"`) |
| `Tags` | Теги из Workshop |
| `Url` | Вычисляется из `PublishedFileId` |

---

## VanillaMod

*Определена в `Services/GithubService.cs`*

Представляет репозиторий из организации Vanilla-Expanded, обогащённый данными из Steam.

| Поле | Описание |
|---|---|
| `Name` | Имя репозитория |
| `Description` | Описание (обновляется из Steam при обогащении) |
| `HtmlUrl` | Ссылка на GitHub |
| `PreviewUrl` | Превью (по умолчанию `"no_image.jpg"`, заменяется при обогащении) |
| `Stars` | Звёзды репозитория |
| `IsEnriched` | Флаг завершённого обогащения из Steam |
| `Compatibility` | `"1.5 / 1.6"` |

---

## ModInfo

*Определена в `Services/ModModelService.cs`*

Представляет установленный локальный мод, прочитанный из `About/About.xml`.

| Поле | Описание |
|---|---|
| `Name` | Название мода |
| `Author` | Автор |
| `Version` | Целевая версия из `<targetVersion>` |
| `FolderName` | Имя директории мода |
| `Tags` | Авто-назначенные теги |
| `IsCompatibleWithCurrentGame` | `true`, если Version содержит `1.5` или `1.6` (или пустое) |

---

## ModRecommendation

*Определена в `ViewModels/MainViewModel.cs`*

Рекомендация мода для секции Radar на главном экране.

| Поле | Описание |
|---|---|
| `Title` | Название мода |
| `MatchPercent` | Процент совпадения с избранным (40–95%) |
| `Reason` | Причина рекомендации (строка, например `"Quality of Life"`) |
| `SteamUrl` | URL для поиска в Steam |

---

## DailyQuest

*Определена в `Pages/RadarPage.xaml.cs`*

Данные квеста дня для секции Radar.

| Поле | Описание |
|---|---|
| `Title` | Заголовок квеста |
| `Desc` | Описание задания |
| `Target` | Целевое количество модов |
| `Category` | Категория для подсчёта прогресса (`"any"`, `"combat"`, `"qol"` и др.) |
