using RealEstateApp.ViewModels;

namespace RealEstateApp.Views;

public partial class HeightCalculatorPage : ContentPage
{
    public HeightCalculatorPage(HeightCalculatorPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnDisappearing()
    {
        if (BindingContext is HeightCalculatorPageViewModel vm)
            vm.UnsubscribeFromBarometer();
    }
}