namespace RimworldModManager.Services;

public class LinkService
{
    public string GetSteamUrl(string modName)
        => $"https://steamcommunity.com/workshop/browse/?appid=294100&searchtext={Uri.EscapeDataString(modName)}";

    public string GetNexusUrl(string modName)
        => $"https://www.nexusmods.com/rimworld/search/?gsearch={Uri.EscapeDataString(modName)}";
}