using Microsoft.Maui.Dispatching;
using RimworldModManager.Extensions;
using RimworldModManager.Models;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;

namespace RimworldModManager.Pages;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesViewModel _viewModel;
    private readonly ThemeService _themeService;

    public FavoritesPage(FavoritesViewModel viewModel, ThemeService themeService, ThemeViewModel themeViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _themeService = themeService;

        BindingContext = new { Theme = themeViewModel, Favorites = viewModel };

        _themeService.ThemeChanged += () => MainThread.BeginInvokeOnMainThread(ApplyTheme);
        ApplyTheme();
    }

    public FavoritesPage() : this(
        MauiProgram.Current!.Services.GetService<FavoritesViewModel>()!,
        MauiProgram.Current!.Services.GetService<ThemeService>()!,
        MauiProgram.Current!.Services.GetService<ThemeViewModel>()!)
    { }

    private void ApplyTheme() { }

    private async void OnSteamClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FavoriteItem item)
        {
            await btn.PlayNeonClickAnimation();
            _viewModel.OpenSteamCommand.Execute(item);
        }
    }

    private async void OnNexusClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FavoriteItem item)
        {
            await btn.PlayNeonClickAnimation();
            _viewModel.OpenNexusCommand.Execute(item);
        }
    }

    private async void OnRemoveFavoriteClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is FavoriteItem item)
        {
            await btn.PlayNeonClickAnimation();
            _viewModel.RemoveCommand.Execute(item);
        }
    }
}
