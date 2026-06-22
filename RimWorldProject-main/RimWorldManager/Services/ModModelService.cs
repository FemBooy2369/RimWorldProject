using System.Text.RegularExpressions;

namespace RimworldModManager.Services;

public class ModInfo
{
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public string TargetVersion => Version;
    public bool IsCompatibleWithCurrentGame => string.IsNullOrEmpty(Version) || Version.Contains("1.5") || Version.Contains("1.6");
}

public class ModFolderService
{
    private readonly string _folderKey = "mods_folder_path";

    public string? GetSavedFolderPath() => Preferences.Get(_folderKey, null);
    public void SaveFolderPath(string path) => Preferences.Set(_folderKey, path);

    public List<ModInfo> ScanFolder(string folderPath)
    {
        var result = new List<ModInfo>();
        if (!Directory.Exists(folderPath)) return result;

        foreach (var dir in Directory.GetDirectories(folderPath))
        {
            var aboutPath = Path.Combine(dir, "About", "About.xml");
            if (!File.Exists(aboutPath)) continue;

            try
            {
                var xml = File.ReadAllText(aboutPath);
                var mod = new ModInfo
                {
                    FolderName = Path.GetFileName(dir),
                    Name = ParseXmlValue(xml, "name"),
                    Author = ParseXmlValue(xml, "author"),
                    Version = ParseXmlValue(xml, "targetVersion"),
                };

                mod.Tags = AutoAssignTags(mod.Name, mod.Author, xml);
                result.Add(mod);
            }
            catch { }
        }

        return result.OrderBy(m => m.Name).ToList();
    }

    private string ParseXmlValue(string xml, string tag)
    {
        var open = $"<{tag}>";
        var close = $"</{tag}>";
        var start = xml.IndexOf(open, StringComparison.OrdinalIgnoreCase);
        if (start < 0) return string.Empty;
        start += open.Length;
        var end = xml.IndexOf(close, start, StringComparison.OrdinalIgnoreCase);
        if (end < 0) return string.Empty;
        return xml[start..end].Trim();
    }

    private List<string> AutoAssignTags(string name, string author, string xml)
    {
        var tags = new List<string>();
        var text = (name + " " + author + " " + xml).ToLower();

        if (Regex.IsMatch(text, @"performance|optimization|fps|lag")) tags.Add("Performance");
        if (Regex.IsMatch(text, @"qol|quality of life|ui|interface|menu")) tags.Add("QoL");
        if (Regex.IsMatch(text, @"graphic|texture|visual|shader")) tags.Add("Graphics");
        if (Regex.IsMatch(text, @"overhaul|rewrite|big update|expansion")) tags.Add("Overhaul");
        if (Regex.IsMatch(text, @"combat|weapon|fight")) tags.Add("Combat");
        if (Regex.IsMatch(text, @"vanilla expanded|ve-")) tags.Add("Vanilla Expanded");
        if (Regex.IsMatch(text, @"storyteller|difficulty|raid")) tags.Add("Gameplay");

        if (tags.Count == 0) tags.Add("Other");
        return tags.Distinct().ToList();
    }
}