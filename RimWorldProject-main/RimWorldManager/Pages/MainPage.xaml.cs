using Microsoft.Maui.Dispatching;
using RimworldModManager.Extensions;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;
using System.Linq;

namespace RimworldModManager.Pages;

public partial class MainPage : ContentPage
{
    private readonly ThemeService _themeService;
    private readonly ThemeViewModel _themeViewModel;
    private readonly SteamApiService _steamApiService;
    private readonly LinkService _linkService;
    private readonly MainViewModel _mainViewModel;

    private readonly List<string> _categories = new()
    {
        "Все категории", "QoL", "Gameplay", "Graphics", "Performance",
        "Combat", "Overhaul", "Vanilla Expanded", "UI", "Weapons",
        "Animals", "Buildings", "Storyteller"
    };

    public MainPage(MainViewModel vm, ThemeService themeService, ThemeViewModel themeViewModel,
                    SteamApiService steamApiService, LinkService linkService)
    {
        InitializeComponent();
        _mainViewModel = vm;
        _themeService = themeService;
        _themeViewModel = themeViewModel;
        _steamApiService = steamApiService;
        _linkService = linkService;

        BindingContext = new { Main = vm, Theme = themeViewModel };
        CategoryPicker.ItemsSource = _categories;
        CategoryPicker.SelectedIndex = 0;

        _themeService.ThemeChanged += () => MainThread.BeginInvokeOnMainThread(ApplyTheme);
        ApplyTheme();
        _mainViewModel.RefreshRadarCommand.Execute(null);
    }

    public MainPage() : this(
        MauiProgram.Current!.Services.GetService<MainViewModel>()!,
        MauiProgram.Current!.Services.GetService<ThemeService>()!,
        MauiProgram.Current!.Services.GetService<ThemeViewModel>()!,
        MauiProgram.Current!.Services.GetService<SteamApiService>()!,
        MauiProgram.Current!.Services.GetService<LinkService>()!)
    { }

    private string CurrentCategory =>
        CategoryPicker.SelectedIndex > 0 ? _categories[CategoryPicker.SelectedIndex] : string.Empty;

    private void ApplyTheme()
    {
        BackgroundColor = _themeService.GetBackground();
    }

    private async void OnFindButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();
        await PerformSearch();
    }

    private async void OnCategoryChanged(object sender, EventArgs e)
    {
        await PerformSearch();
    }

    private async Task PerformSearch()
    {
        string query = SearchEntry.Text?.Trim() ?? "";
        string category = CurrentCategory;

        _mainViewModel.SteamResults.Clear();

        if (string.IsNullOrWhiteSpace(query) && (string.IsNullOrEmpty(category) || category == "Все категории"))
        {
            ResultsLabel.Text = "Введите запрос или выберите категорию";
            return;
        }

        List<SteamMod> results = string.IsNullOrWhiteSpace(query)
            ? await _steamApiService.SearchModsAsync(category)
            : await _steamApiService.SearchModsAsync(query);

        if (!string.IsNullOrEmpty(category) && category != "Все категории")
        {
            var catLower = category.ToLowerInvariant();
            results = results.Where(m =>
                m.Title.ToLowerInvariant().Contains(catLower) ||
                m.Description.ToLowerInvariant().Contains(catLower) ||
                m.Tags.Any(t => t.ToLowerInvariant().Contains(catLower))
            ).ToList();
        }

        foreach (var mod in results)
        {
            mod.Compatibility = "1.5 / 1.6";
            _mainViewModel.SteamResults.Add(mod);
        }

        ResultsLabel.Text = results.Count > 0 ? $"Найдено: {results.Count}" : "Ничего не найдено";
    }

    private async void OnSteamClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string url && !string.IsNullOrEmpty(url))
            await Launcher.OpenAsync(new Uri(url));
    }

    private async void OnNexusClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string title && !string.IsNullOrEmpty(title))
        {
            await btn.PlayNeonClickAnimation();
            await Launcher.OpenAsync(new Uri(_linkService.GetNexusUrl(title)));
        }
    }

    private async void OnAddToFavoritesClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is SteamMod mod)
        {
            await btn.PlayNeonClickAnimation();
            _mainViewModel.AddToFavoritesCommand.Execute(mod);
        }
    }

    // 🎨 Кнопка выбора темы
    private async void OnThemeClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();

        var action = await DisplayActionSheet("🎨 Настройка темы колонии", "Отмена", null,
            "🌑 Тёмная тема",
            "☀️ Светлая тема",
            "💚 Зелёный акцент",
            "🟠 Оранжевый акцент",
            "🔴 Красный акцент",
            "💟 Розовый акцент",
            "🌈 Случайная тема");

        switch (action)
        {
            case "🌑 Тёмная тема": _themeService.SetTheme(AppColorTheme.Dark); break;
            case "☀️ Светлая тема": _themeService.SetTheme(AppColorTheme.Light); break;
            case "💚 Зелёный акцент": _themeService.SetAccent(AccentColor.Green); break;
            case "🟠 Оранжевый акцент": _themeService.SetAccent(AccentColor.Orange); break;
            case "🔴 Красный акцент": _themeService.SetAccent(AccentColor.Red); break;
            case "💟 Розовый акцент": _themeService.SetAccent(AccentColor.Pink); break;
            case "🌈 Случайная тема":
                var rnd = (AccentColor)new Random().Next(4);
                _themeService.SetAccent(rnd);
                await DisplayAlert("🎲", $"Акцент изменён на: {rnd}", "OK");
                break;
        }
    }
}