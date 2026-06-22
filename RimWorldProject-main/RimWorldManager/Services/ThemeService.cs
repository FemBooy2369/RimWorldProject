using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;

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
    public Color GetMenuBg() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#0D0D0D") : Color.FromArgb("#FAFAFA");
    public Color GetMenuSelected() => CurrentTheme == AppColorTheme.Dark ? Color.FromArgb("#1A1A1A") : Color.FromArgb("#EFEFEF");

    private static readonly Dictionary<AccentColor, (string Primary, string Secondary, string Light, string Dark)> Palettes = new()
    {
        { AccentColor.Green,  ("#00C853", "#00FF7F", "#6FFFB0", "#00692E") },
        { AccentColor.Orange, ("#E67300", "#FF8C00", "#FFB347", "#7A3D00") },
        { AccentColor.Red,    ("#D6291C", "#FF3B3B", "#FF8080", "#7A1410") },
        { AccentColor.Pink,   ("#D90073", "#FF00FF", "#FF66FF", "#80003D") },
    };

    public Color GetAccent() => Color.FromArgb(Palettes[CurrentAccent].Primary);
    public Color GetAccentSecondary() => Color.FromArgb(Palettes[CurrentAccent].Secondary);
    public Color GetAccentLight() => Color.FromArgb(Palettes[CurrentAccent].Light);
    public Color GetAccentDark() => Color.FromArgb(Palettes[CurrentAccent].Dark);

    public Color GetAccentFaded()
    {
        var c = GetAccent();
        return new Color(c.Red, c.Green, c.Blue, 0.10f);
    }
}