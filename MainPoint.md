# Точка входа и конфигурация

---

## MauiProgram.cs

Точка конфигурации приложения. Создаёт `MauiApp` через builder-паттерн и настраивает DI-контейнер.

```csharp
public static class MauiProgram
{
    public static MauiApp? Current { get; private set; }
    public static MauiApp CreateMauiApp()
}
```

`Current` — статическое свойство, которое хранит ссылку на созданное приложение. Используется во всех пустых конструкторах страниц для разрешения зависимостей:

```csharp
// Резервный конструктор для создания через Shell XAML
public MainPage() : this(
    MauiProgram.Current!.Services.GetService<MainViewModel>()!, ...)
{ }
```

### Регистрация сервисов (Singleton)

| Тип | Описание |
|---|---|
| `ThemeService` | Управление темой, общее состояние |
| `ThemeViewModel` | Реактивные цвета, один экземпляр на всё приложение |
| `SteamApiService` | HTTP-клиент Steam API |
| `GithubService` | HTTP-клиент GitHub API |
| `LinkService` | Генерация URL |
| `ModFolderService` | Сканирование папки с модами |
| `StorageService` | JSON-хранилище избранного и истории |
| `MainViewModel` | Логика поиска и рекомендаций |
| `FavoritesViewModel` | Управление избранным |
| `MyModsViewModel` | Управление локальными модами |

### Регистрация страниц (Transient)

`MainPage`, `VanillaPage`, `MyModsPage`, `FavoritesPage`, `RadarPage` — новый экземпляр при каждом запросе.

### Шрифты

Подключается `OpenSans-Regular.ttf` с псевдонимом `"OpenSansRegular"`.

---

## App.xaml.cs

Минимальная реализация — только переопределение `CreateWindow` для создания окна с `AppShell`.

```csharp
protected override Window CreateWindow(IActivationState? activationState)
    => new Window(new AppShell());
```

---

## AppShell.xaml.cs

Shell-навигация с TabBar. Динамически перекрашивает нижнюю панель навигации при смене темы.

### Логика

Подписывается на `ThemeService.ThemeChanged` и вызывает `ApplyTabBarColors()` на главном потоке.

**`ApplyTabBarColors()`** устанавливает:
- `TabBarBackgroundColor` → `ThemeService.GetMenuBg()`
- `TabBarForegroundColor` и `TabBarTitleColor` → `ThemeService.GetAccent()` (выбранный акцентный цвет)
- `TabBarUnselectedColor` → `ThemeService.GetTextMuted()`

`BindingContext` установлен в `ThemeViewModel` — для биндингов в `AppShell.xaml`.
