using Microsoft.Extensions.Logging;
using RealEstateApp.Repositories;
using RealEstateApp.Services;
using RealEstateApp.ViewModels;
using RealEstateApp.Views;
using RealEstateApp.Views.Modals;

namespace RealEstateApp;

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
                fonts.AddFont("fa-solid-900.ttf", "FA-solid");
            });

        builder.Services.AddSingleton<IPropertyService, MockRepository>();
        builder.Services.AddSingleton<PropertyListPage>();
        builder.Services.AddSingleton<PropertyListPageViewModel>();

        builder.Services.AddTransient<PropertyDetailPage>();
        builder.Services.AddTransient<PropertyDetailPageViewModel>();

        builder.Services.AddTransient<CompassPage>();
        builder.Services.AddTransient<CompassViewModel>();

        builder.Services.AddTransient<HeightCalculatorPage>();
        builder.Services.AddTransient<HeightCalculatorPageViewModel>();

        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SettingsPageViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPageViewModel>();

        builder.Services.AddTransient<AddEditPropertyPage>();
        builder.Services.AddTransient<AddEditPropertyPageViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.ConfigureEssentials(essentials =>
         {
             essentials.UseMapServiceToken("YOUR-API-TOKEN");
         });

        return builder.Build();
    }
}
