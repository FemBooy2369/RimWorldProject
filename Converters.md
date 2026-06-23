# Converters

Конвертеры значений для XAML-биндингов. Реализуют `IValueConverter`. Зарегистрированы как статические ресурсы в `Styles.xaml` или напрямую в XAML страниц.

---

## GreaterThanZeroConverter.cs

Преобразует целое число в булево значение. Используется для управления видимостью элементов, зависящих от наличия данных.

```csharp
public class GreaterThanZeroConverter : IValueConverter
{
    public object Convert(object? value, ...) =>
        value is int count && count > 0;
}
```

### Применение

Типичный сценарий — показать секцию только если в коллекции есть элементы:

```xml
<StackLayout IsVisible="{Binding FavoritesCount,
    Converter={StaticResource GreaterThanZeroConverter}}">
```

`ConvertBack` не реализован (бросает `NotImplementedException`) — используется только в одну сторону.

---

## InverseBoolConverter.cs

Инвертирует булево значение. Нужен там, где стандартного биндинга недостаточно для отображения «обратного» состояния.

```csharp
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, ...) =>
        value is bool b ? !b : true;
}
```

### Поведение при некорректном значении

Если `value` не является `bool`, возвращает `true` (безопасный fallback — элемент виден по умолчанию).

### Применение

Типичный сценарий — показать placeholder пока данные загружаются:

```xml
<!-- Показываем список когда НЕ загружается -->
<CollectionView IsVisible="{Binding IsLoading,
    Converter={StaticResource InverseBoolConverter}}" />

<!-- Показываем лоадер когда загружается -->
<ActivityIndicator IsVisible="{Binding IsLoading}" />
```

`ConvertBack` не реализован — используется только в одну сторону.
