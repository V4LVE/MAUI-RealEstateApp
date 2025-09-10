using RealEstateApp.ViewModels;

namespace RealEstateApp.Views.Modals;


public partial class CompassPage : ContentPage
{
    public CompassPage(CompassViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is CompassViewModel vm)
            vm.UnSubscribeOnCompass();
    }
}