using System.ComponentModel;
using System.Runtime.CompilerServices;
using RimworldModManager.Services;

namespace RimworldModManager.ViewModels;

public class ThemeViewModel : INotifyPropertyChanged
{
    private readonly ThemeService _themeService;

    public Color Accent => _themeService.GetAccent();
    public Color AccentFaded => _themeService.GetAccentFaded();
    public Color Background => _themeService.GetBackground();
    public Color Card => _themeService.GetCard();
    public Color CardAlt => _themeService.GetCardAlt();
    public Color Input => _themeService.GetInput();
    public Color TextPrimary => _themeService.GetTextPrimary();
    public Color TextMuted => _themeService.GetTextMuted();
    public Color Border => _themeService.GetBorder();
    public Color BtnText => _themeService.GetBtnText();
    public string Heart => _themeService.CurrentHeart;

    public ThemeViewModel(ThemeService themeService)
    {
        _themeService = themeService;
        _themeService.ThemeChanged += () => MainThread.BeginInvokeOnMainThread(NotifyAll);
    }

    private void NotifyAll()
    {
        OnPropertyChanged(nameof(Accent));
        OnPropertyChanged(nameof(AccentFaded));
        OnPropertyChanged(nameof(Background));
        OnPropertyChanged(nameof(Card));
        OnPropertyChanged(nameof(CardAlt));
        OnPropertyChanged(nameof(Input));
        OnPropertyChanged(nameof(TextPrimary));
        OnPropertyChanged(nameof(TextMuted));
        OnPropertyChanged(nameof(Border));
        OnPropertyChanged(nameof(BtnText));
        OnPropertyChanged(nameof(Heart));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}