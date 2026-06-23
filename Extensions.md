# Extensions

Методы расширения для стандартных элементов управления MAUI.

---

## ButtonExtensions.cs

Добавляет анимацию нажатия для кнопок приложения.

```csharp
public static class ButtonExtensions
{
    public static async Task PlayNeonClickAnimation(this Button button)
}
```

### Описание анимации

Последовательность из двух параллельных эффектов:

1. **Scale** — кнопка сжимается до 92% за 80 мс (`CubicOut`), затем возвращается в исходный размер за 120 мс (`CubicIn`)
2. **Fade** — прозрачность снижается до 85% за 60 мс, затем восстанавливается за 80 мс

Суммарная длительность — около 200 мс, что создаёт ощущение физического нажатия.

### Использование

Вызывается в code-behind перед выполнением основного действия:

```csharp
private async void OnButtonClicked(object sender, EventArgs e)
{
    if (sender is Button btn)
        await btn.PlayNeonClickAnimation();

    // основное действие
}
```

Метод безопасен при `null`-кнопке — досрочно выходит через проверку `if (button == null) return`.

### Где применяется

Почти все интерактивные кнопки в приложении: поиск, добавление в избранное, открытие ссылок, переключение темы, реролл в Radar, голосование VS, кнопки папки в MyMods.
