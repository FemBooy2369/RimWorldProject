using System.Net.Http;
using System.Text.Json;

namespace RimworldModManager.Services;

public class SteamMod
{
    public string PublishedFileId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PreviewUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url => $"https://steamcommunity.com/sharedfiles/filedetails/?id={PublishedFileId}";
}

public class SteamApiService
{
    // ВСТАВЬ СВОЙ КЛЮЧ СЮДА:
    private const string ApiKey = "845FC575FB3287C4A997BE1392094BE5";
    private const string AppId = "294100";
    private readonly HttpClient _http = new();

    public async Task<List<SteamMod>> SearchModsAsync(string query)
    {
        var url = $"https://api.steampowered.com/IPublishedFileService/QueryFiles/v1/" +
                  $"?key={ApiKey}&appid={AppId}&search_text={Uri.EscapeDataString(query)}" +
                  $"&numperpage=15&return_previews=true&return_metadata=true&return_short_description=true&query_type=0";
        try
        {
            var response = await _http.GetStringAsync(url);
            var doc = JsonDocument.Parse(response);
            var files = doc.RootElement
                .GetProperty("response")
                .GetProperty("publishedfiledetails");

            var result = new List<SteamMod>();
            foreach (var file in files.EnumerateArray())
            {
                var preview = file.TryGetProperty("preview_url", out var p) ? p.GetString() ?? "" : "";
                var desc = file.TryGetProperty("short_description", out var d) ? d.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(desc))
                    desc = file.TryGetProperty("description", out var d2) ? d2.GetString() ?? "" : "";

                // Чистим HTML теги из описания
                desc = System.Text.RegularExpressions.Regex.Replace(desc, "<.*?>", "").Trim();
                if (desc.Length > 120) desc = desc[..120] + "...";

                result.Add(new SteamMod
                {
                    PublishedFileId = file.TryGetProperty("publishedfileid", out var id) ? id.GetString() ?? "" : "",
                    Title = file.TryGetProperty("title", out var title) ? title.GetString() ?? "Без названия" : "Без названия",
                    PreviewUrl = string.IsNullOrEmpty(preview) ? "no_image.jpg" : preview,
                    Description = string.IsNullOrEmpty(desc) ? "Описание недоступно" : desc,
                });
            }
            return result;
        }
        catch
        {
            return new();
        }
    }
}