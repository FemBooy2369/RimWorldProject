using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using RimworldModManager.Services;

namespace RimworldModManager.ViewModels;

public class FavoritesViewModel : INotifyPropertyChanged
{
    private readonly StorageService _storageService;
    private readonly LinkService _linkService;

    public ObservableCollection<string> Favorites { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand OpenSteamCommand { get; }
    public ICommand OpenTopModsCommand { get; }
    public ICommand OpenPlaygroundCommand { get; }

    public FavoritesViewModel(StorageService storageService, LinkService linkService)
    {
        _storageService = storageService;
        _linkService = linkService;

        LoadCommand = new Command(async () =>
        {
            var favs = await _storageService.LoadFavoritesAsync();
            Favorites.Clear();
            foreach (var f in favs) Favorites.Add(f);
        });

        RemoveCommand = new Command<string>(async name =>
        {
            var favs = await _storageService.LoadFavoritesAsync();
            favs.Remove(name);
            await _storageService.SaveFavoritesAsync(favs);
            Favorites.Remove(name);
        });

        OpenSteamCommand = new Command<string>(async name =>
        {
            if (!string.IsNullOrEmpty(name))
                await Launcher.OpenAsync(new Uri(_linkService.GetSteamUrl(name)));
        });

        OpenTopModsCommand = new Command<string>(async name =>
        {
            if (!string.IsNullOrEmpty(name))
                await Launcher.OpenAsync(new Uri(_linkService.GetTopModsUrl(name)));
        });

        OpenPlaygroundCommand = new Command<string>(async name =>
        {
            if (!string.IsNullOrEmpty(name))
                await Launcher.OpenAsync(new Uri(_linkService.GetPlaygroundUrl(name)));
        });

        LoadCommand.Execute(null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}