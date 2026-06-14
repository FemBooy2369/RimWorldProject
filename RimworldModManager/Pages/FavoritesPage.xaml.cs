using RimworldModManager.Services;
using RimworldModManager.ViewModels;

namespace RimworldModManager.Pages;

public partial class FavoritesPage : ContentPage
{
    private readonly ThemeService _themeService;

    public FavoritesPage(FavoritesViewModel vm, ThemeService themeService)
    {
        InitializeComponent();
        BindingContext = vm;
        _themeService = themeService;
        _themeService.ThemeChanged += () => MainThread.BeginInvokeOnMainThread(ApplyTheme);
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var accent = _themeService.GetAccent();
        BackgroundColor = _themeService.GetBackground();
        FavTitleLabel.Text = $"{_themeService.CurrentHeart} Избранное";
        FavTitleLabel.TextColor = accent;
        EmptyLabel.TextColor = _themeService.GetTextMuted();
    }
}