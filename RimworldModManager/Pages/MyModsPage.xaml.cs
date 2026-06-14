using RimworldModManager.Services;
using RimworldModManager.ViewModels;

namespace RimworldModManager.Pages;

public partial class MyModsPage : ContentPage
{
    private readonly ThemeService _themeService;

    public MyModsPage(MyModsViewModel vm, ThemeService themeService)
    {
        InitializeComponent();
        BindingContext = vm;
        _themeService = themeService;
        _themeService.ThemeChanged += () => MainThread.BeginInvokeOnMainThread(ApplyTheme);
        ApplyTheme();
    }

    private void ApplyTheme()
    {
        var accent = _themeService.GetAccent();
        var card = _themeService.GetCard();
        var border = _themeService.GetBorder();
        var muted = _themeService.GetTextMuted();

        BackgroundColor = _themeService.GetBackground();
        MyModsTitleLabel.TextColor = accent;
        FolderFrame.BackgroundColor = card;
        FolderFrame.BorderColor = border;
        FolderPathLabel.TextColor = muted;
        PickFolderBtn.BackgroundColor = card;
        PickFolderBtn.TextColor = accent;
        PickFolderBtn.BorderColor = accent;
        RefreshBtn.BackgroundColor = card;
        RefreshBtn.BorderColor = border;
        RefreshBtn.TextColor = muted;
        EmptyModsLabel.TextColor = muted;
    }
}