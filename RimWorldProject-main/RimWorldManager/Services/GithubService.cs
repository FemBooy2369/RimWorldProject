using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RimworldModManager.Services;

public class VanillaMod
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = "Нет описания";
    public string HtmlUrl { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = "no_image.jpg";
    public int Stars { get; set; }
    public bool IsEnriched { get; set; } = false;
    public string Compatibility { get; set; } = "1.5 / 1.6"; // ← Добавлено
}

public class GithubService
{
    private readonly HttpClient _http = new();
    private readonly SteamApiService _steamApiService;

    public GithubService(SteamApiService steamApiService)
    {
        _steamApiService = steamApiService;
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RimworldModManager", "1.0"));
    }

    public async Task<List<VanillaMod>> GetVanillaModsAsync()
    {
        var result = new List<VanillaMod>();
        int page = 1;

        try
        {
            while (true)
            {
                var url = $"https://api.github.com/orgs/Vanilla-Expanded/repos?per_page=30&page={page}";
                var response = await _http.GetStringAsync(url);
                var doc = JsonDocument.Parse(response);
                var repos = doc.RootElement.EnumerateArray().ToList();

                if (repos.Count == 0) break;

                foreach (var repo in repos)
                {
                    var name = repo.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                    var mod = new VanillaMod
                    {
                        Name = name,
                        Description = repo.TryGetProperty("description", out var d) ? d.GetString() ?? "" : "",
                        HtmlUrl = repo.TryGetProperty("html_url", out var h) ? h.GetString() ?? "" : "",
                        Stars = repo.TryGetProperty("stargazers_count", out var s) ? s.GetInt32() : 0,
                        Compatibility = "1.5 / 1.6"
                    };
                    result.Add(mod);
                }

                if (repos.Count < 30) break;
                page++;
            }
        }
        catch { }

        return result.OrderByDescending(r => r.Stars).ToList();
    }

    public async Task EnrichWithSteamAsync(List<VanillaMod> mods, Action<VanillaMod> onUpdated)
    {
        for (int i = 0; i < Math.Min(mods.Count, 40); i += 5)
        {
            var batch = mods.Skip(i).Take(5).ToList();

            foreach (var mod in batch)
            {
                if (mod.IsEnriched) continue;

                try
                {
                    var steamResults = await _steamApiService.SearchModsAsync(mod.Name);
                    var best = steamResults.FirstOrDefault(s =>
                        s.Title.ToLowerInvariant().Contains(mod.Name.ToLowerInvariant()) ||
                        mod.Name.ToLowerInvariant().Contains(s.Title.ToLowerInvariant()));

                    if (best != null)
                    {
                        mod.PreviewUrl = best.PreviewUrl;
                        if (string.IsNullOrEmpty(mod.Description) || mod.Description.Length < 40)
                            mod.Description = best.Description;
                    }

                    mod.IsEnriched = true;
                    onUpdated?.Invoke(mod);
                }
                catch { }
            }

            await Task.Delay(600);
        }
    }
}