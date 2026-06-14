using System.Text.Json;
using Microsoft.Maui.Storage;

namespace RimworldModManager.Services;

public class StorageService
{
    private readonly string _favoritesPath;
    private readonly string _historyPath;

    public StorageService()
    {
        var dir = FileSystem.AppDataDirectory;
        _favoritesPath = Path.Combine(dir, "favorites.json");
        _historyPath = Path.Combine(dir, "history.json");
    }

    public async Task<List<string>> LoadFavoritesAsync()
    {
        if (!File.Exists(_favoritesPath)) return new();
        var json = await File.ReadAllTextAsync(_favoritesPath);
        return JsonSerializer.Deserialize<List<string>>(json) ?? new();
    }

    public async Task SaveFavoritesAsync(List<string> favorites)
    {
        var json = JsonSerializer.Serialize(favorites);
        await File.WriteAllTextAsync(_favoritesPath, json);
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