namespace RimworldModManager.Services;

public class LinkService
{
    public string GetPlaygroundUrl(string modName)
        => $"https://www.google.com/search?q=site:playground.ru/rimworld+{Uri.EscapeDataString(modName)}";

    public string GetTopModsUrl(string modName)
        => $"https://top-mods.ru/mods/rimworld?ref=dtf.ru&search={Uri.EscapeDataString(modName)}";

    public string GetSteamUrl(string modName)
        => $"https://steamcommunity.com/workshop/browse/?appid=294100&searchtext={modName.Replace(" ", "+")}";
}