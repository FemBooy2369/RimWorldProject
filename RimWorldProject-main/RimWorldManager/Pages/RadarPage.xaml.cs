using Microsoft.Maui.Dispatching;
using RimworldModManager.Extensions;
using RimworldModManager.Models;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;
using System.Text.RegularExpressions;

namespace RimworldModManager.Pages;

/// <summary>
/// Страница Mod Radar — разведывательный центр колонии.
/// </summary>
public partial class RadarPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly SteamApiService _steamApiService;
    private readonly StorageService _storageService;
    private readonly LinkService _linkService;

    // ── Состояние: Мод дня ───────────────────────────────────────────────────
    private SteamMod? _modOfDay;
    // ── Состояние: Мод vs Мод ────────────────────────────────────────────────
    private SteamMod? _vsModA;
    private SteamMod? _vsModB;
    private SteamMod? _vsWinner;
    // ── Состояние: Квест дня ─────────────────────────────────────────────────
    private DailyQuest? _currentQuest;

    // ── Категории ────────────────────────────────────────────────────────────
    private readonly List<string> _popularCategories = new()
    {
        "QoL", "Performance", "Combat", "Graphics", "Overhaul",
        "Weapons", "Animals", "Buildings", "Storyteller", "Vanilla Expanded"
    };

    private readonly string[] _randomCategories =
    {
        "combat", "vanilla expanded", "performance", "graphics",
        "weapons", "qol", "building", "animals", "storyteller"
    };

    public RadarPage(MainViewModel viewModel, ThemeViewModel themeViewModel,
                     SteamApiService steamApiService, StorageService storageService,
                     LinkService linkService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _steamApiService = steamApiService;
        _storageService = storageService;
        _linkService = linkService;

        BindingContext = new { Theme = themeViewModel, Radar = viewModel };
        PopularCategoryPicker.ItemsSource = _popularCategories;
        PopularCategoryPicker.SelectedIndex = 0;

        // Параллельная загрузка секций
        _ = LoadModOfDayAsync();
        _ = LoadVsModsAsync();
        _ = LoadPopularAsync(_popularCategories[0]);
        _ = LoadDnaAsync();
        _ = LoadQuestAsync();
    }

    public RadarPage() : this(
        MauiProgram.Current!.Services.GetService<MainViewModel>()!,
        MauiProgram.Current!.Services.GetService<ThemeViewModel>()!,
        MauiProgram.Current!.Services.GetService<SteamApiService>()!,
        MauiProgram.Current!.Services.GetService<StorageService>()!,
        MauiProgram.Current!.Services.GetService<LinkService>()!)
    { }

    // ═════════════════════════════════════════════════════════════════════════
    // Мод дня
    // ═════════════════════════════════════════════════════════════════════════
    private async Task LoadModOfDayAsync(string? category = null)
    {
        ModDayLoader.IsVisible = true;
        ModDayLoader.IsRunning = true;
        ModDayContent.IsVisible = false;

        try
        {
            var query = category ?? _randomCategories[new Random().Next(_randomCategories.Length)];
            var results = await _steamApiService.SearchModsAsync(query);

            if (results.Count > 0)
            {
                _modOfDay = results[new Random().Next(results.Count)];
                ModDayTitle.Text = _modOfDay.Title;
                ModDayDesc.Text = _modOfDay.Description;
                ModDayImage.Source = string.IsNullOrEmpty(_modOfDay.PreviewUrl)
                    ? "no_image.jpg"
                    : _modOfDay.PreviewUrl;
            }
            else
            {
                ModDayTitle.Text = "Не удалось загрузить";
                ModDayDesc.Text = "Проверь соединение с интернетом";
            }
        }
        catch
        {
            ModDayTitle.Text = "Ошибка загрузки";
            ModDayDesc.Text = string.Empty;
        }
        finally
        {
            ModDayLoader.IsRunning = false;
            ModDayLoader.IsVisible = false;
            ModDayContent.IsVisible = true;
        }
    }

    private async void OnRerollClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();
        await LoadModOfDayAsync();
    }

    private async void OnModDaySteamClicked(object sender, EventArgs e)
    {
        if (_modOfDay is not null)
            await Launcher.OpenAsync(new Uri(_modOfDay.Url));
    }

    // ❤️ Кнопка Избранное для Мода дня
    private async void OnModDayFavClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
            await btn.PlayNeonClickAnimation();

        if (_modOfDay is null) return;

        var favs = await _storageService.LoadFavoritesAsync();

        if (!favs.Any(f => f.Title == _modOfDay.Title))
        {
            favs.Add(new FavoriteItem
            {
                Title = _modOfDay.Title,
                PreviewUrl = _modOfDay.PreviewUrl ?? ""
            });

            await _storageService.SaveFavoritesAsync(favs);

            _viewModel.FavoritesCount = favs.Count;

            await DisplayAlert("✅ Добавлено!",
                $"Мод «{_modOfDay.Title}» успешно добавлен в избранное колонии.", "OK");
        }
        else
        {
            await DisplayAlert("❤️ Уже в избранном",
                "Этот мод уже служит твоей колонии.", "OK");
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    // DNA Колонии
    // ═════════════════════════════════════════════════════════════════════════
    private async Task LoadDnaAsync()
    {
        var favs = await _storageService.LoadFavoritesAsync();
        if (favs.Count == 0)
        {
            DnaArchetypeLabel.Text = "🎮 Добавь моды в избранное для анализа";
            return;
        }

        var scores = new Dictionary<string, int>
        {
            ["combat"] = 0,
            ["qol"] = 0,
            ["graphics"] = 0,
            ["performance"] = 0,
            ["vanilla"] = 0,
            ["overhaul"] = 0,
        };

        foreach (var item in favs)
        {
            var lower = item.Title.ToLowerInvariant();
            if (Regex.IsMatch(lower, @"combat|weapon|fight|gun|rimmu|yayo")) scores["combat"]++;
            if (Regex.IsMatch(lower, @"qol|ui|menu|hud|interface|dubs|rimhud")) scores["qol"]++;
            if (Regex.IsMatch(lower, @"graphic|texture|visual|shader|wall light")) scores["graphics"]++;
            if (Regex.IsMatch(lower, @"performance|fish|thread|fps|optim")) scores["performance"]++;
            if (Regex.IsMatch(lower, @"vanilla expanded|ve-|oskar")) scores["vanilla"]++;
            if (Regex.IsMatch(lower, @"overhaul|rewrite|expansion|big")) scores["overhaul"]++;
        }

        int total = scores.Values.Sum() == 0 ? 1 : scores.Values.Sum();

        void SetBar(ProgressBar bar, Label lbl, int val)
        {
            var pct = (float)val / total;
            bar.Progress = pct;
            lbl.Text = $"{(int)(pct * 100)}%";
        }

        SetBar(DnaCombatBar, DnaCombatPct, scores["combat"]);
        SetBar(DnaQolBar, DnaQolPct, scores["qol"]);
        SetBar(DnaGraphicsBar, DnaGraphicsPct, scores["graphics"]);
        SetBar(DnaPerfBar, DnaPerfPct, scores["performance"]);
        SetBar(DnaVanillaBar, DnaVanillaPct, scores["vanilla"]);
        SetBar(DnaOverhaulBar, DnaOverhaulPct, scores["overhaul"]);

        var top = scores.OrderByDescending(k => k.Value).First().Key;
        DnaArchetypeLabel.Text = top switch
        {
            "combat" => "⚔️ Воин — ты живёшь ради битвы",
            "qol" => "✨ Перфекционист — максимум удобства",
            "graphics" => "🎨 Эстет — красота прежде всего",
            "performance" => "⚡ Инженер — FPS превыше всего",
            "vanilla" => "🌿 Ванилла-фанат — верен оригиналу",
            "overhaul" => "🏗️ Строитель — любишь глобальные перемены",
            _ => "🎮 Исследователь — тебе всего понемногу"
        };
    }

    private async void OnDnaRefreshClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();
        await LoadDnaAsync();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Мод vs Мод
    // ═════════════════════════════════════════════════════════════════════════
    private async Task LoadVsModsAsync()
    {
        VsLoader.IsVisible = true;
        VsLoader.IsRunning = true;
        VsContent.IsVisible = false;
        VsResultFrame.IsVisible = false;

        try
        {
            var rng = new Random();
            var cat1 = _randomCategories[rng.Next(_randomCategories.Length)];
            var cat2 = _randomCategories[rng.Next(_randomCategories.Length)];

            var results1 = await _steamApiService.SearchModsAsync(cat1);
            var results2 = await _steamApiService.SearchModsAsync(cat2);

            if (results1.Count > 0) _vsModA = results1[rng.Next(results1.Count)];
            if (results2.Count > 0) _vsModB = results2[rng.Next(results2.Count)];

            if (_vsModA is not null)
            {
                ModATitle.Text = _vsModA.Title;
                ModAImage.Source = string.IsNullOrEmpty(_vsModA.PreviewUrl) ? "no_image.jpg" : _vsModA.PreviewUrl;
            }
            if (_vsModB is not null)
            {
                ModBTitle.Text = _vsModB.Title;
                ModBImage.Source = string.IsNullOrEmpty(_vsModB.PreviewUrl) ? "no_image.jpg" : _vsModB.PreviewUrl;
            }
        }
        catch { }
        finally
        {
            VsLoader.IsRunning = false;
            VsLoader.IsVisible = false;
            VsContent.IsVisible = true;
        }
    }

    private async Task HandleVoteAsync(SteamMod? winner)
    {
        if (winner is null) return;
        _vsWinner = winner;

        var favs = await _storageService.LoadFavoritesAsync();
        if (!favs.Any(f => f.Title == winner.Title))
        {
            favs.Add(new FavoriteItem { Title = winner.Title, PreviewUrl = winner.PreviewUrl ?? "" });
            await _storageService.SaveFavoritesAsync(favs);
        }

        VsResultLabel.Text = $"🏆 Победитель: {winner.Title}";
        VsResultFrame.IsVisible = true;
        VoteAButton.IsEnabled = false;
        VoteBButton.IsEnabled = false;
    }

    private async void OnVoteAClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();
        await HandleVoteAsync(_vsModA);
    }

    private async void OnVoteBClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();
        await HandleVoteAsync(_vsModB);
    }

    private async void OnVsRerollClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();
        VoteAButton.IsEnabled = true;
        VoteBButton.IsEnabled = true;
        await LoadVsModsAsync();
    }

    private async void OnVsWinnerSteamClicked(object sender, EventArgs e)
    {
        if (_vsWinner is not null)
            await Launcher.OpenAsync(new Uri(_vsWinner.Url));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Квест дня
    // ═════════════════════════════════════════════════════════════════════════
    private async Task LoadQuestAsync()
    {
        var favs = await _storageService.LoadFavoritesAsync();
        var today = DateTime.Today;
        var seed = today.Year * 1000 + today.DayOfYear;
        var rng = new Random(seed);

        var quests = new List<DailyQuest>
        {
            new() { Title = "🔫 Воин дня", Desc = "Добавь 3 боевых мода в избранное", Target = 3, Category = "combat" },
            new() { Title = "✨ Мастер удобства", Desc = "Добавь 2 QoL-мода в избранное", Target = 2, Category = "qol" },
            new() { Title = "🎨 Художник колонии", Desc = "Добавь 2 графических мода", Target = 2, Category = "graphics" },
            new() { Title = "⚡ Инженер FPS", Desc = "Добавь 1 performance-мод", Target = 1, Category = "performance"},
            new() { Title = "🌿 Ванилла-путь", Desc = "Добавь 3 Vanilla Expanded мода", Target = 3, Category = "vanilla" },
            new() { Title = "🏗️ Большая стройка", Desc = "Набери 5 любых модов в избранное", Target = 5, Category = "any" },
        };

        _currentQuest = quests[rng.Next(quests.Count)];

        int progress = _currentQuest.Category == "any"
            ? favs.Count
            : favs.Count(f =>
            {
                var lower = f.Title.ToLowerInvariant();
                return _currentQuest.Category switch
                {
                    "combat" => Regex.IsMatch(lower, @"combat|weapon|fight|gun"),
                    "qol" => Regex.IsMatch(lower, @"qol|ui|menu|hud"),
                    "graphics" => Regex.IsMatch(lower, @"graphic|texture|visual"),
                    "performance" => Regex.IsMatch(lower, @"performance|fish|thread|fps"),
                    "vanilla" => Regex.IsMatch(lower, @"vanilla expanded|ve-"),
                    _ => false
                };
            });

        int clamped = Math.Min(progress, _currentQuest.Target);
        float pct = (float)clamped / _currentQuest.Target;
        bool complete = clamped >= _currentQuest.Target;

        QuestTitle.Text = _currentQuest.Title;
        QuestDesc.Text = _currentQuest.Desc;
        QuestProgressBar.Progress = pct;
        QuestProgressLabel.Text = $"{clamped} / {_currentQuest.Target}";
        QuestAchieveFrame.IsVisible = complete;

        if (complete)
            QuestAchieveLabel.Text = "🏆 Квест выполнен! Отличная колония!";
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Популярные моды
    // ═════════════════════════════════════════════════════════════════════════
    private async Task LoadPopularAsync(string category)
    {
        PopularLoader.IsVisible = true;
        PopularLoader.IsRunning = true;
        PopularCollection.ItemsSource = null;

        try
        {
            var results = await _steamApiService.SearchModsAsync(category);
            PopularCollection.ItemsSource = results;
        }
        catch { }
        finally
        {
            PopularLoader.IsRunning = false;
            PopularLoader.IsVisible = false;
        }
    }

    private async void OnPopularCategoryChanged(object sender, EventArgs e)
    {
        if (PopularCategoryPicker.SelectedIndex < 0) return;
        await LoadPopularAsync(_popularCategories[PopularCategoryPicker.SelectedIndex]);
    }

    private async void OnRefreshPopularClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) await btn.PlayNeonClickAnimation();
        var cat = PopularCategoryPicker.SelectedIndex >= 0
            ? _popularCategories[PopularCategoryPicker.SelectedIndex]
            : _popularCategories[0];
        await LoadPopularAsync(cat);
    }

    private async void OnPopularSteamClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string url && !string.IsNullOrEmpty(url))
            await Launcher.OpenAsync(new Uri(url));
    }

    private async void OnPopularFavClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string title)
        {
            await btn.PlayNeonClickAnimation();
            var favs = await _storageService.LoadFavoritesAsync();
            if (!favs.Any(f => f.Title == title))
            {
                favs.Add(new FavoriteItem { Title = title, PreviewUrl = "" });
                await _storageService.SaveFavoritesAsync(favs);
            }
        }
    }

    private async void OnBuildSteamClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string query)
        {
            await btn.PlayNeonClickAnimation();
            await Launcher.OpenAsync(new Uri(_linkService.GetSteamUrl(query)));
        }
    }
}

public class DailyQuest
{
    public string Title { get; set; } = string.Empty;
    public string Desc { get; set; } = string.Empty;
    public int Target { get; set; }
    public string Category { get; set; } = "any";
}