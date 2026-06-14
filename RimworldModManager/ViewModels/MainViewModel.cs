using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using RimworldModManager.Services;

namespace RimworldModManager.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly LinkService _linkService;
    private readonly StorageService _storageService;
    private readonly SteamApiService _steamApiService;

    private string _modName = string.Empty;
    private string _playgroundUrl = string.Empty;
    private string _topModsUrl = string.Empty;
    private string _steamUrl = string.Empty;
    private bool _linksVisible;
    private bool _historyVisible;
    private bool _resultsVisible;
    private bool _isLoading;

    public string ModName
    {
        get => _modName;
        set { _modName = value; OnPropertyChanged(); }
    }
    public string PlaygroundUrl
    {
        get => _playgroundUrl;
        set { _playgroundUrl = value; OnPropertyChanged(); }
    }
    public string TopModsUrl
    {
        get => _topModsUrl;
        set { _topModsUrl = value; OnPropertyChanged(); }
    }
    public string SteamUrl
    {
        get => _steamUrl;
        set { _steamUrl = value; OnPropertyChanged(); }
    }
    public bool LinksVisible
    {
        get => _linksVisible;
        set { _linksVisible = value; OnPropertyChanged(); }
    }
    public bool HistoryVisible
    {
        get => _historyVisible;
        set { _historyVisible = value; OnPropertyChanged(); }
    }
    public bool ResultsVisible
    {
        get => _resultsVisible;
        set { _resultsVisible = value; OnPropertyChanged(); }
    }
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> History { get; } = new();
    public ObservableCollection<SteamMod> SteamResults { get; } = new();

    public ICommand GenerateCommand { get; }
    public ICommand OpenCommand { get; }
    public ICommand CopyCommand { get; }
    public ICommand CopyAllCommand { get; }
    public ICommand AddToFavoritesCommand { get; }
    public ICommand SelectHistoryCommand { get; }
    public ICommand ClearHistoryCommand { get; }
    public ICommand ToggleHistoryCommand { get; }
    public ICommand OpenSteamModCommand { get; }
    public ICommand OpenTopModsModCommand { get; }
    public ICommand OpenPlaygroundModCommand { get; }

    public MainViewModel(LinkService linkService, StorageService storageService, SteamApiService steamApiService)
    {
        _linkService = linkService;
        _storageService = storageService;
        _steamApiService = steamApiService;

        GenerateCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(ModName)) return;
            HistoryVisible = false;
            await _storageService.AddToHistoryAsync(ModName);
            await LoadHistoryAsync();
            await SearchSteamAsync();
        });

        OpenCommand = new Command<string>(async url =>
        {
            if (!string.IsNullOrEmpty(url))
                await Launcher.OpenAsync(new Uri(url));
        });

        CopyCommand = new Command<string>(async url =>
        {
            if (!string.IsNullOrEmpty(url))
                await Clipboard.SetTextAsync(url);
        });

        CopyAllCommand = new Command(async () =>
        {
            var all = $"playground.ru:\n{PlaygroundUrl}\n\ntop-mods.ru:\n{TopModsUrl}\n\nSteam:\n{SteamUrl}";
            await Clipboard.SetTextAsync(all);
        });

        AddToFavoritesCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(ModName)) return;
            var favs = await _storageService.LoadFavoritesAsync();
            if (!favs.Contains(ModName))
            {
                favs.Add(ModName);
                await _storageService.SaveFavoritesAsync(favs);
            }
        });

        SelectHistoryCommand = new Command<string>(name =>
        {
            ModName = name;
            HistoryVisible = false;
            GenerateCommand.Execute(null);
        });

        ClearHistoryCommand = new Command(async () =>
        {
            await _storageService.ClearHistoryAsync();
            History.Clear();
            HistoryVisible = false;
        });

        ToggleHistoryCommand = new Command(async () =>
        {
            await LoadHistoryAsync();
            HistoryVisible = !HistoryVisible;
        });

        // Открыть конкретный мод в Steam
        OpenSteamModCommand = new Command<SteamMod>(async mod =>
        {
            if (mod != null)
                await Launcher.OpenAsync(new Uri(mod.Url));
        });

        // Открыть мод на top-mods.ru по названию
        OpenTopModsModCommand = new Command<SteamMod>(async mod =>
        {
            if (mod != null)
                await Launcher.OpenAsync(new Uri(_linkService.GetTopModsUrl(mod.Title)));
        });

        // Открыть мод на playground через Google
        OpenPlaygroundModCommand = new Command<SteamMod>(async mod =>
        {
            if (mod != null)
                await Launcher.OpenAsync(new Uri(_linkService.GetPlaygroundUrl(mod.Title)));
        });

        _ = LoadHistoryAsync();
    }

    private async Task SearchSteamAsync()
    {
        if (string.IsNullOrWhiteSpace(ModName)) return;
        IsLoading = true;
        ResultsVisible = false;
        SteamResults.Clear();

        var results = await _steamApiService.SearchModsAsync(ModName);
        foreach (var mod in results) SteamResults.Add(mod);

        IsLoading = false;
        ResultsVisible = SteamResults.Count > 0;
    }

    private async Task LoadHistoryAsync()
    {
        var history = await _storageService.LoadHistoryAsync();
        History.Clear();
        foreach (var item in history) History.Add(item);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}