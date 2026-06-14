using Microsoft.Maui.Platform;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;

namespace RimworldModManager.Pages;

public partial class MainPage : ContentPage
{
    private readonly ThemeService _themeService;
    private readonly ThemeViewModel _themeViewModel;

    public MainPage(MainViewModel vm, ThemeService themeService, ThemeViewModel themeViewModel)
    {
        InitializeComponent();
        _themeService = themeService;
        _themeViewModel = themeViewModel;
        BindingContext = new { Main = vm, Theme = themeViewModel };
        _themeService.ThemeChanged += () => MainThread.BeginInvokeOnMainThread(ApplyTheme);
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var accent = _themeService.GetAccent();
        var bg = _themeService.GetBackground();
        var card = _themeService.GetCard();
        var border = _themeService.GetBorder();
        var text = _themeService.GetTextPrimary();
        var muted = _themeService.GetTextMuted();
        var input = _themeService.GetInput();
        var btnText = _themeService.GetBtnText();

        BackgroundColor = bg;

        TitleLabel.TextColor = accent;
        ThemeButton.BorderColor = accent;
        ThemeButton.TextColor = accent;
        ThemeButton.BackgroundColor = card;

        SearchFrame.BackgroundColor = card;
        SearchFrame.BorderColor = accent;
        SearchLabel.TextColor = accent;
        SearchEntry.BackgroundColor = input;
        SearchEntry.TextColor = text;
        FindButton.BackgroundColor = accent;
        FindButton.TextColor = btnText;

        FavoriteButton.Text = _themeService.CurrentHeart;
        FavoriteButton.TextColor = accent;
        FavoriteButton.BorderColor = accent;
        FavoriteButton.BackgroundColor = card;

        HistoryFrame.BackgroundColor = card;
        HistoryFrame.BorderColor = border;
        HistoryLabel.TextColor = muted;

        ResultsLabel.TextColor = accent;
        ScrollTopBtn.BorderColor = accent;
        ScrollTopBtn.TextColor = accent;
        ScrollTopBtn.BackgroundColor = card;
    }

    private async void OnThemeClicked(object sender, EventArgs e)
    {
        var action = await DisplayActionSheet("Настройки темы", "Закрыть", null,
            "🌑 Тёмная", "☀️ Светлая",
            "💚 Зелёный", "🟠 Оранжевый", "🔴 Красный", "💟 PeackMe");

        switch (action)
        {
            case "🌑 Тёмная": _themeService.SetTheme(AppColorTheme.Dark); break;
            case "☀️ Светлая": _themeService.SetTheme(AppColorTheme.Light); break;
            case "💚 Зелёный": _themeService.SetAccent(AccentColor.Green); break;
            case "🟠 Оранжевый": _themeService.SetAccent(AccentColor.Orange); break;
            case "🔴 Красный": _themeService.SetAccent(AccentColor.Red); break;
            case "💟 PeackMe": _themeService.SetAccent(AccentColor.Pink); break;
        }
    }

    private async void OnScrollTopClicked(object sender, EventArgs e)
    {
        await MainScrollView.ScrollToAsync(0, 0, true);
    }
}