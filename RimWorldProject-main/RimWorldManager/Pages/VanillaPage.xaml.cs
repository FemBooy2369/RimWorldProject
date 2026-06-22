using RimworldModManager.Services;
using RimworldModManager.ViewModels;
using System.Collections.ObjectModel;
using System.Timers;

namespace RimworldModManager.Pages;

public partial class VanillaPage : ContentPage
{
    private readonly GithubService _githubService;
    private readonly LinkService _linkService;
    private ObservableCollection<VanillaMod> _allMods = new();
    private System.Timers.Timer? _searchTimer;

    public VanillaPage(GithubService githubService, LinkService linkService, ThemeViewModel themeViewModel)
    {
        InitializeComponent();
        BindingContext = new { Theme = themeViewModel };
        _githubService = githubService;
        _linkService = linkService;

        ModsCollection.ItemsSource = _allMods;
        VanillaRefreshView.Refreshing += OnVanillaRefreshing;

        _ = LoadAsync();
    }

    // Пустой конструктор — ОБЯЗАТЕЛЕН, т.к. AppShell.xaml создаёт страницу как <pages:VanillaPage />
    public VanillaPage() : this(
        MauiProgram.Current!.Services.GetService<GithubService>()!,
        MauiProgram.Current!.Services.GetService<LinkService>()!,
        MauiProgram.Current!.Services.GetService<ThemeViewModel>()!)
    {
    }

    private async Task LoadAsync()
    {
        LoadingContainer.IsVisible = true;
        ModsCollection.IsVisible = false;
        VanillaRefreshView.IsRefreshing = true;

        var githubList = await _githubService.GetVanillaModsAsync();

        _allMods.Clear();
        foreach (var mod in githubList)
            _allMods.Add(mod);

        ModsCollection.IsVisible = true;
        LoadingContainer.IsVisible = false;
        VanillaRefreshView.IsRefreshing = false;
    }

    private async void OnVanillaRefreshing(object? sender, EventArgs e)
    {
        await LoadAsync();
    }

    private void OnVanillaSearchChanged(object sender, TextChangedEventArgs e)
    {
        _searchTimer?.Stop();
        _searchTimer = new System.Timers.Timer(350) { AutoReset = false };
        _searchTimer.Elapsed += (s, args) =>
            MainThread.BeginInvokeOnMainThread(() => ApplyFilter(e.NewTextValue));
        _searchTimer.Start();
    }

    private void ApplyFilter(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            ModsCollection.ItemsSource = _allMods;
            return;
        }

        var filter = searchText.ToLowerInvariant();
        var filtered = _allMods.Where(m =>
            m.Name.ToLowerInvariant().Contains(filter) ||
            m.Description.ToLowerInvariant().Contains(filter)
        ).ToList();

        ModsCollection.ItemsSource = filtered;
    }

    private async void OnGithubClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string url && !string.IsNullOrEmpty(url))
            await Launcher.OpenAsync(new Uri(url));
    }
}