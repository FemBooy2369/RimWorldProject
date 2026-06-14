using Microsoft.Maui.Storage;

namespace RimworldModManager.Services;

public enum AppColorTheme { Dark, Light }
public enum AccentColor { Green, Orange, Red, Pink }

public class ThemeService
{
    public AppColorTheme CurrentTheme { get; private set; } = AppColorTheme.Dark;
    public AccentColor CurrentAccent { get; private set; } = AccentColor.Green;

    public event Action? ThemeChanged;

    private const string ThemeKey = "app_theme";
    private const string AccentKey = "app_accent";

    public static readonly Dictionary<AccentColor, string> HeartEmoji = new()
    {
        { AccentColor.Green,  "💚" },
        { AccentColor.Orange, "🧡" },
        { AccentColor.Red,    "❤️" },
        { AccentColor.Pink,   "💟" },
    };

    public string CurrentHeart => HeartEmoji[CurrentAccent];

    public ThemeService()
    {
        CurrentTheme = (AppColorTheme)Preferences.Get(ThemeKey, 0);
        var saved = Preferences.Get(AccentKey, 0);
        CurrentAccent = saved >= 0 && saved <= 3 ? (AccentColor)saved : AccentColor.Green;
    }

    public void SetTheme(AppColorTheme theme)
    {
        CurrentTheme = theme;
        Preferences.Set(ThemeKey, (int)theme);
        ThemeChanged?.Invoke();
    }

    public void SetAccent(AccentColor accent)
    {
        CurrentAccent = accent;
        Preferences.Set(AccentKey, (int)accent);
        ThemeChanged?.Invoke();
    }

    public Color GetBackground() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#0A0A0A") : Color.FromArgb("#F5F5F5");
    public Color GetCard() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#141414") : Color.FromArgb("#FFFFFF");
    public Color GetCardAlt() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#1A1A1A") : Color.FromArgb("#F0F0F0");
    public Color GetInput() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#1E1E1E") : Color.FromArgb("#EEEEEE");
    public Color GetTextPrimary() => CurrentTheme == AppColorTheme.Dark ? Colors.White : Colors.Black;
    public Color GetTextMuted() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#AAAAAA") : Color.FromArgb("#555555");
    public Color GetBorder() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#2A2A2A") : Color.FromArgb("#DDDDDD");
    public Color GetBtnText() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#0A0A0A") : Colors.White;

    public Color GetAccent() => CurrentAccent switch
    {
        AccentColor.Green => Color.FromArgb("#00FF7F"),
        AccentColor.Orange => Color.FromArgb("#FF8C00"),
        AccentColor.Red => Color.FromArgb("#FF3B3B"),
        AccentColor.Pink => Color.FromArgb("#FF69B4"),
        _ => Color.FromArgb("#00FF7F"),
    };

    public Color GetAccentFaded() => CurrentAccent switch
    {
        AccentColor.Green => Color.FromArgb("#2200FF7F"),
        AccentColor.Orange => Color.FromArgb("#22FF8C00"),
        AccentColor.Red => Color.FromArgb("#22FF3B3B"),
        AccentColor.Pink => Color.FromArgb("#22FF69B4"),
        _ => Color.FromArgb("#2200FF7F"),
    };
}