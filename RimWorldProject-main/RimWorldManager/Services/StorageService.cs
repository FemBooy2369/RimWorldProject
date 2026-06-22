using System.Text.Json;
using Microsoft.Maui.Storage;
using RimworldModManager.Models;

namespace RimworldModManager.Services;

public class StorageService
{
    private readonly string _favoritesPath;
    private readonly string _historyPath;

    public event Action? FavoritesChanged;

    public StorageService()
    {
        var dir = FileSystem.AppDataDirectory;
        _favoritesPath = Path.Combine(dir, "favorites.json");
        _historyPath = Path.Combine(dir, "history.json");
    }

    public async Task<List<FavoriteItem>> LoadFavoritesAsync()
    {
        if (!File.Exists(_favoritesPath)) return new();
        var json = await File.ReadAllTextAsync(_favoritesPath);
        try
        {
            var items = JsonSerializer.Deserialize<List<FavoriteItem>>(json);
            return items ?? new();
        }
        catch
        {
            var oldList = JsonSerializer.Deserialize<List<string>>(json) ?? new();
            return oldList.Select(s => new FavoriteItem { Title = s, PreviewUrl = "" }).ToList();
        }
    }

    public async Task SaveFavoritesAsync(List<FavoriteItem> favorites)
    {
        var json = JsonSerializer.Serialize(favorites);
        await File.WriteAllTextAsync(_favoritesPath, json);
        FavoritesChanged?.Invoke();
    }

    public async Task<List<string>> LoadHistoryAsync()
    {
        if (!File.Exists(_historyPath)) return new();
        var json = await File.ReadAllTextAsync(_historyPath);
        return JsonSerializer.Deserialize<List<string>>(json) ?? new();
    }

    public async Task AddToHistoryAsync(string modName)
    {
        var history = await LoadHistoryAsync();
        history.Remove(modName);
        history.Insert(0, modName);
        if (history.Count > 20) history = history.Take(20).ToList();
        var json = JsonSerializer.Serialize(history);
        await File.WriteAllTextAsync(_historyPath, json);
    }

    public async Task ClearHistoryAsync()
    {
        await File.WriteAllTextAsync(_historyPath, "[]");
    }
}
