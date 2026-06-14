namespace RimworldModManager.Services;

public class ModInfo
{
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FolderName { get; set; } = string.Empty;
}

public class ModFolderService
{
    private readonly string _folderKey = "mods_folder_path";

    public string? GetSavedFolderPath()
        => Preferences.Get(_folderKey, null);

    public void SaveFolderPath(string path)
        => Preferences.Set(_folderKey, path);

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
                result.Add(new ModInfo
                {
                    FolderName = Path.GetFileName(dir),
                    Name = ParseXmlValue(xml, "name"),
                    Author = ParseXmlValue(xml, "author"),
                    Version = ParseXmlValue(xml, "targetVersion"),
                });
            }
            catch { /* пропускаем битые моды */ }
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
}