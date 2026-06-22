using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using RimworldModManager.Models;
using RimworldModManager.Services;

namespace RimworldModManager.ViewModels;

public class FavoritesViewModel : INotifyPropertyChanged
{
    private readonly StorageService _storageService;
    private readonly LinkService _linkService;

    public ObservableCollection<FavoriteItem> Favorites { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand OpenSteamCommand { get; }
    public ICommand OpenNexusCommand { get; }

    public FavoritesViewModel(StorageService storageService, LinkService linkService)
    {
        _storageService = storageService;
        _linkService = linkService;

        LoadCommand = new Command(async () => await ReloadFavoritesAsync());

        RemoveCommand = new Command<FavoriteItem>(async item =>
        {
            if (item == null) return;
            var favs = await _storageService.LoadFavoritesAsync();
            favs.RemoveAll(f => f.Title == item.Title);
            await _storageService.SaveFavoritesAsync(favs);
        });

        OpenSteamCommand = new Command<FavoriteItem>(async item =>
        {
            if (item != null && !string.IsNullOrEmpty(item.Title))
                await Launcher.OpenAsync(new Uri(_linkService.GetSteamUrl(item.Title)));
        });

        OpenNexusCommand = new Command<FavoriteItem>(async item =>
        {
            if (item != null && !string.IsNullOrEmpty(item.Title))
                await Launcher.OpenAsync(new Uri(_linkService.GetNexusUrl(item.Title)));
        });

        _storageService.FavoritesChanged += async () =>
            await MainThread.InvokeOnMainThreadAsync(ReloadFavoritesAsync);

        LoadCommand.Execute(null);
    }

    private async Task ReloadFavoritesAsync()
    {
        var favs = await _storageService.LoadFavoritesAsync();
        Favorites.Clear();
        foreach (var f in favs)
            Favorites.Add(f);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
