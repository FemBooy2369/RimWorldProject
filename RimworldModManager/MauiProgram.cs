using Microsoft.Extensions.Logging;
using RimworldModManager.Pages;
using RimworldModManager.Services;
using RimworldModManager.ViewModels;

namespace RimworldModManager;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<LinkService>();
        builder.Services.AddSingleton<StorageService>();
        builder.Services.AddSingleton<ModFolderService>();
        builder.Services.AddSingleton<SteamApiService>();
        builder.Services.AddSingleton<ThemeService>();
        builder.Services.AddSingleton<ThemeViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<FavoritesViewModel>();
        builder.Services.AddTransient<MyModsViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<MyModsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}