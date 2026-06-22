using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using RimworldModManager.Models;
using RimworldModManager.Services;

namespace RimworldModManager.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly LinkService _linkService;
    private readonly StorageService _storageService;
    private readonly SteamApiService _steamApiService;
    private readonly ModFolderService _modFolderService;

    private string _modName = string.Empty;
    private bool _historyVisible;
    private bool _resultsVisible;
    private bool _isLoading;

    public string ModName { get => _modName; set { _modName = value; OnPropertyChanged(); } }
    public bool HistoryVisible { get => _historyVisible; set { _historyVisible = value; OnPropertyChanged(); } }
    public bool ResultsVisible { get => _resultsVisible; set { _resultsVisible = value; OnPropertyChanged(); } }
    public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

    public int FavoritesCount { get;  set; }
    public int MyModsCount { get; private set; }
    public int RadarCount { get; private set; }

    public ObservableCollection<string> History { get; } = new();
    public ObservableCollection<SteamMod> SteamResults { get; } = new();
    public ObservableCollection<ModRecommendation> ModRadar { get; } = new();

    public ICommand GenerateCommand { get; }
    public ICommand AddToFavoritesCommand { get; }
    public ICommand SelectHistoryCommand { get; }
    public ICommand ClearHistoryCommand { get; }
    public ICommand ToggleHistoryCommand { get; }
    public ICommand OpenSteamModCommand { get; }
    public ICommand OpenNexusModCommand { get; }
    public ICommand RefreshRadarCommand { get; }

    public MainViewModel(LinkService linkService, StorageService storageService,
                        SteamApiService steamApiService, ModFolderService modFolderService)
    {
        _linkService = linkService;
        _storageService = storageService;
        _steamApiService = steamApiService;
        _modFolderService = modFolderService;

        GenerateCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(ModName)) return;
            await _storageService.AddToHistoryAsync(ModName);
            await LoadHistoryAsync();
            await SearchSteamAsync();
        });

        AddToFavoritesCommand = new Command<SteamMod>(async mod =>
        {
            if (mod == null || string.IsNullOrWhiteSpace(mod.Title)) return;
            var favs = await _storageService.LoadFavoritesAsync();
            if (!favs.Any(f => f.Title == mod.Title))
            {
                favs.Add(new FavoriteItem { Title = mod.Title, PreviewUrl = mod.PreviewUrl });
                await _storageService.SaveFavoritesAsync(favs);
                await LoadModRadarAsync();
            }
        });

        SelectHistoryCommand = new Command<string>(name =>
        {
            ModName = name;
            GenerateCommand.Execute(null);
        });

        ClearHistoryCommand = new Command(async () =>
        {
            await _storageService.ClearHistoryAsync();
            History.Clear();
        });

        ToggleHistoryCommand = new Command(async () =>
        {
            await LoadHistoryAsync();
            HistoryVisible = !HistoryVisible;
        });

        OpenSteamModCommand = new Command<SteamMod>(async mod => { if (mod != null) await Launcher.OpenAsync(new Uri(mod.Url)); });
        OpenNexusModCommand = new Command<SteamMod>(async mod => { if (mod != null) await Launcher.OpenAsync(new Uri(_linkService.GetNexusUrl(mod.Title))); });
        RefreshRadarCommand = new Command(async () => await LoadModRadarAsync());

        _ = LoadHistoryAsync();
        _ = LoadModRadarAsync();
    }

    private async Task LoadModRadarAsync()
    {
        ModRadar.Clear();

        var favs = await _storageService.LoadFavoritesAsync();
        FavoritesCount = favs.Count;
        OnPropertyChanged(nameof(FavoritesCount));

        var path = _modFolderService.GetSavedFolderPath();
        var myMods = !string.IsNullOrEmpty(path)
            ? _modFolderService.ScanFolder(path)
            : new List<ModInfo>();

        MyModsCount = myMods.Count;
        OnPropertyChanged(nameof(MyModsCount));

        if (favs.Count == 0 && myMods.Count == 0)
        {
            RadarCount = 0;
            OnPropertyChanged(nameof(RadarCount));
            return;
        }

        var favTitles = favs.Select(f => f.Title).ToList();

        var recommendations = myMods
            .Where(m => !favTitles.Contains(m.Name))
            .Select(m => new ModRecommendation
            {
                Title = m.Name,
                MatchPercent = CalculateMatchPercent(m, favTitles),
                Reason = GetMatchReason(m),
                SteamUrl = _linkService.GetSteamUrl(m.Name)
            })
            .OrderByDescending(r => r.MatchPercent)
            .Take(6)
            .ToList();

        foreach (var rec in recommendations)
            ModRadar.Add(rec);

        RadarCount = ModRadar.Count;
        OnPropertyChanged(nameof(RadarCount));
    }

    private int CalculateMatchPercent(ModInfo mod, List<string> favorites)
    {
        if (favorites.Count == 0) return 45;
        int matches = favorites.Count(f =>
            mod.Name.ToLowerInvariant().Contains(f.ToLowerInvariant()) ||
            f.ToLowerInvariant().Contains(mod.Name.ToLowerInvariant()));
        return Math.Min(95, 40 + matches * 18);
    }

    private string GetMatchReason(ModInfo mod)
    {
        if (mod.Tags.Any(t => t.ToLower().Contains("vanilla"))) return "Vanilla Expanded";
        if (mod.Tags.Contains("QoL")) return "Quality of Life";
        if (mod.Tags.Contains("Performance")) return "Оптимизация";
        if (mod.Tags.Contains("Graphics")) return "Графика";
        return "Рекомендация";
    }

    private async Task SearchSteamAsync()
    {
        IsLoading = true;
        ResultsVisible = false;
        SteamResults.Clear();

        if (!string.IsNullOrWhiteSpace(ModName))
        {
            var results = await _steamApiService.SearchModsAsync(ModName);
            foreach (var mod in results) SteamResults.Add(mod);
        }

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

public class ModRecommendation
{
    public string Title { get; set; } = string.Empty;
    public int MatchPercent { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string SteamUrl { get; set; } = string.Empty;
}
