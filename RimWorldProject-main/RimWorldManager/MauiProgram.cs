using Microsoft.Extensions.Logging;
using RimworldModManager.Pages;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace RimworldModManager;

public static class MauiProgram
{
    public static MauiApp? Current { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Services
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<ThemeViewModel>();
        builder.Services.AddSingleton<SteamApiService>();
        builder.Services.AddSingleton<GithubService>();
        builder.Services.AddSingleton<LinkService>();
        builder.Services.AddSingleton<ModFolderService>();
        builder.Services.AddSingleton<StorageService>();

        // ViewModels
        builder.Services.AddSingleton<MainViewModel>();        
        builder.Services.AddSingleton<FavoritesViewModel>();
        builder.Services.AddSingleton<MyModsViewModel>();

        // Pages
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<VanillaPage>();
        builder.Services.AddTransient<MyModsPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<RadarPage>();

        Current = builder.Build();
        return Current;
    }
}