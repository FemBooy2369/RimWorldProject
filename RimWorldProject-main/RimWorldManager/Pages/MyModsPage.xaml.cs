using Microsoft.Maui.Dispatching;
using RimworldModManager.Extensions;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;

namespace RimworldModManager.Pages;

public partial class MyModsPage : ContentPage
{
    private readonly MyModsViewModel _viewModel;
    private readonly ThemeService _themeService;

    public MyModsPage(MyModsViewModel viewModel, ThemeService themeService, ThemeViewModel themeViewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _themeService = themeService;

        BindingContext = new { Theme = themeViewModel, Mods = viewModel };

        //MyModsCollection.ItemsSource = _viewModel.Mods;

        _themeService.ThemeChanged += () => MainThread.BeginInvokeOnMainThread(ApplyTheme);
        ApplyTheme();
    }

    // Пустой конструктор для Shell
    public MyModsPage() : this(
        MauiProgram.Current!.Services.GetService<MyModsViewModel>()!,
        MauiProgram.Current!.Services.GetService<ThemeService>()!,
        MauiProgram.Current!.Services.GetService<ThemeViewModel>()!)
    { }

    private void ApplyTheme() { }

    private async void OnSelectFolderClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
            await btn.PlayNeonClickAnimation();

        _viewModel.PickFolderCommand.Execute(null);
    }
}