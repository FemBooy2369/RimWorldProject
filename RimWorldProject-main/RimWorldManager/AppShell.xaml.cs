using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;

namespace RimworldModManager;

public partial class AppShell : Shell
{
    private readonly ThemeService _themeService;

    public AppShell()
    {
        InitializeComponent();

        _themeService = MauiProgram.Current!.Services.GetService<ThemeService>()!;
        BindingContext = MauiProgram.Current!.Services.GetService<ThemeViewModel>();

        _themeService.ThemeChanged += () =>
            MainThread.BeginInvokeOnMainThread(ApplyTabBarColors);

        ApplyTabBarColors();
    }

    private void ApplyTabBarColors()
    {
        var accent = _themeService.GetAccent();
        var bg = _themeService.GetMenuBg();
        var unselected = _themeService.GetTextMuted();

        Shell.SetTabBarBackgroundColor(this, bg);
        Shell.SetTabBarForegroundColor(this, accent);
        Shell.SetTabBarTitleColor(this, accent);
        Shell.SetTabBarUnselectedColor(this, unselected);
    }
}