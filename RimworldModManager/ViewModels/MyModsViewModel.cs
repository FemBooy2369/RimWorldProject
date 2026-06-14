using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using RimworldModManager.Services;

namespace RimworldModManager.ViewModels;

public class MyModsViewModel : INotifyPropertyChanged
{
    private readonly ModFolderService _modFolderService;

    private string? _folderPath;
    private bool _hasFolder;

    public string? FolderPath
    {
        get => _folderPath;
        set { _folderPath = value; OnPropertyChanged(); HasFolder = !string.IsNullOrEmpty(value); }
    }

    public bool HasFolder
    {
        get => _hasFolder;
        set { _hasFolder = value; OnPropertyChanged(); }
    }

    public ObservableCollection<ModInfo> Mods { get; } = new();

    public ICommand PickFolderCommand { get; }
    public ICommand RefreshCommand { get; }

    public MyModsViewModel(ModFolderService modFolderService)
    {
        _modFolderService = modFolderService;

        FolderPath = _modFolderService.GetSavedFolderPath();
        if (!string.IsNullOrEmpty(FolderPath)) LoadMods(FolderPath);

        PickFolderCommand = new Command(async () =>
        {
#if WINDOWS
            var result = await PickFolderWindowsAsync();
            if (result != null)
            {
                FolderPath = result;
                _modFolderService.SaveFolderPath(result);
                LoadMods(result);
            }
#else
            // На мобильных — ввод пути вручную (заглушка)
            await Task.CompletedTask;
#endif
        });

        RefreshCommand = new Command(() =>
        {
            if (!string.IsNullOrEmpty(FolderPath)) LoadMods(FolderPath);
        });
    }

    private void LoadMods(string path)
    {
        var mods = _modFolderService.ScanFolder(path);
        Mods.Clear();
        foreach (var mod in mods) Mods.Add(mod);
    }

#if WINDOWS
    private async Task<string?> PickFolderWindowsAsync()
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
        picker.FileTypeFilter.Add("*");

        var hwnd = ((MauiWinUIWindow)Application.Current!.Windows[0].Handler.PlatformView!).WindowHandle;
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
#endif

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}