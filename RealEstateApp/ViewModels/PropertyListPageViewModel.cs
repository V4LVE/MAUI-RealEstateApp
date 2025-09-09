using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace RealEstateApp.ViewModels;
public class PropertyListPageViewModel : BaseViewModel
{
    public ObservableCollection<PropertyListItem> PropertiesCollection { get; set; } = new();

    private readonly IPropertyService service;

    public PropertyListPageViewModel(IPropertyService service)
    {
        Title = "Property List";
        this.service = service;
    }

    bool isRefreshing;
    public bool IsRefreshing
    {
        get => isRefreshing;
        set => SetProperty(ref isRefreshing, value);
    }

    Location _lastKnownLocation;

    private Command getPropertiesCommand;
    public ICommand GetPropertiesCommand => getPropertiesCommand ??= new Command(async () => await GetPropertiesAsync());

    async Task GetPropertiesAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;

            List<Property> properties = service.GetProperties();

            if (PropertiesCollection.Count != 0)
                PropertiesCollection.Clear();

            List<PropertyListItem> propertiesListSorted = new();

            foreach (Property property in properties)
            {
                double distance = 0.0;

                if (_lastKnownLocation != null)
                {
                    Location addressLocation = new(property.Latitude ?? 0, property.Longitude ?? 0);

                    distance = Location.CalculateDistance(addressLocation, _lastKnownLocation, DistanceUnits.Kilometers);
                }

                propertiesListSorted.Add(new PropertyListItem(property, distance));
            }

            propertiesListSorted = propertiesListSorted.OrderBy(p => p.Distance).ToList();

            foreach (var propertyListItem in propertiesListSorted)
            {
                PropertiesCollection.Add(propertyListItem);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to get monkeys: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    private Command goToDetailsCommand;
    public ICommand GoToDetailsCommand => goToDetailsCommand ??= new Command<PropertyListItem>(async (propertyListItem) => await GoToDetails(propertyListItem));

    async Task GoToDetails(PropertyListItem propertyListItem)
    {
        if (propertyListItem == null)
            return;

        await Shell.Current.GoToAsync(nameof(PropertyDetailPage), true, new Dictionary<string, object>
        {
            {"MyPropertyListItem", propertyListItem }
        });
    }

    private Command goToAddPropertyCommand;
    public ICommand GoToAddPropertyCommand => goToAddPropertyCommand ??= new Command(async () => await GotoAddProperty());
    async Task GotoAddProperty()
    {
        await Shell.Current.GoToAsync($"{nameof(AddEditPropertyPage)}?mode=newproperty", true, new Dictionary<string, object>
        {
            {"MyProperty", new Property() }
        });
    }

    #region Dsitance sorting
    private Command sortByDistanceCommand;
    public ICommand SortByDistanceCommand => sortByDistanceCommand ??= new Command(async () => await SortByDistance());

    async Task SortByDistance()
    {
        try
        {
            if (_lastKnownLocation == null)
            {
                _lastKnownLocation = await Geolocation.Default.GetLocationAsync();
            }

            await GetPropertiesAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to sort by distance: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

}
