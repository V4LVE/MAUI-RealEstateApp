using RealEstateApp.Models;
using RealEstateApp.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(Mode), "mode")]
[QueryProperty(nameof(Property), "MyProperty")]
public class AddEditPropertyPageViewModel : BaseViewModel
{
    readonly IPropertyService service;

    public AddEditPropertyPageViewModel(IPropertyService service)
    {
        this.service = service;
        Agents = new ObservableCollection<Agent>(service.GetAgents());

        NetworkAccess accessType = Connectivity.Current.NetworkAccess;

        Connectivity.ConnectivityChanged += OnConnectivityChanged;


        if (accessType != NetworkAccess.Internet)
        {
            Shell.Current.DisplayAlert("No Internet", "You are not connected to the internet. Please check your connection and try again.", "OK");
            InternetAvailable = false;
        }
    }

    public string Mode { get; set; }

    #region PROPERTIES
    public ObservableCollection<Agent> Agents { get; }

    private Property _property;
    public Property Property
    {
        get => _property;
        set
        {
            SetProperty(ref _property, value);
            Title = Mode == "newproperty" ? "Add Property" : "Edit Property";

            if (_property.AgentId != null)
            {
                SelectedAgent = Agents.FirstOrDefault(x => x.Id == _property?.AgentId);
            }
        }
    }

    private Agent _selectedAgent;
    public Agent SelectedAgent
    {
        get => _selectedAgent;
        set
        {


            if (Property != null)
            {
                SetProperty(ref _selectedAgent, value);
                Property.AgentId = _selectedAgent?.Id;
            }
        }
    }

    string statusMessage;
    public string StatusMessage
    {
        get { return statusMessage; }
        set { SetProperty(ref statusMessage, value); }
    }

    Color statusColor;
    public Color StatusColor
    {
        get { return statusColor; }
        set { SetProperty(ref statusColor, value); }
    }

    private bool _internetAvailable = true;

    public bool InternetAvailable
    {
        get { return _internetAvailable; }
        set { SetProperty(ref _internetAvailable, value); }
    }
    #endregion


    private Command savePropertyCommand;
    public ICommand SavePropertyCommand => savePropertyCommand ??= new Command(async () => await SaveProperty());
    private async Task SaveProperty()
    {
        if (IsValid() == false)
        {
            StatusMessage = "Please fill in all required fields";
            StatusColor = Colors.Red;
        }
        else
        {
            service.SaveProperty(Property);
            await Shell.Current.GoToAsync("///propertylist");
        }
    }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(Property.Address)
            || Property.Beds == null
            || Property.Price == null
            || Property.AgentId == null)
            return false;
        return true;
    }

    private Command cancelSaveCommand;
    public ICommand CancelSaveCommand => cancelSaveCommand ??= new Command(async () => await Shell.Current.GoToAsync(".."));


    private Command getLocationCommand;
    public ICommand GetLocationCommand => getLocationCommand ??= new Command(async () => await GetCurrentLocation());

    private bool _isCheckingLocation;
    public bool IsCheckingLocation
    {
        get { return _isCheckingLocation; }
        set { SetProperty(ref _isCheckingLocation, value); }
    }

    private async Task GetCurrentLocation()
    {
        try
        {
            _isCheckingLocation = true;

            GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

            Location location = await Geolocation.Default.GetLocationAsync(request);

            if (location != null)
            {
                IEnumerable<Placemark> placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);

                Placemark placemark = placemarks?.FirstOrDefault();

                Property.Address = $"{placemark.Thoroughfare} {placemark.SubThoroughfare}, {placemark.Locality} {placemark.PostalCode} {placemark.CountryName}";

                Property.Latitude = location.Latitude;
                Property.Longitude = location.Longitude;
                OnPropertyChanged(nameof(Property));
            }

        }
        catch (FeatureNotSupportedException fnsEx)
        {
            // Handle not supported on device exception
        }
        catch (FeatureNotEnabledException fneEx)
        {
            // Handle not enabled on device exception
        }
        catch (PermissionException pEx)
        {
            // Handle permission exception
        }
        catch (Exception ex)
        {
            // Unable to get location
        }
        finally
        {
            _isCheckingLocation = false;
        }
    }

    private Command getAddressLocationCommand;
    public ICommand GetAddressLocationCommand => getAddressLocationCommand ??= new Command(async () => await GetAddressLocation());
    private async Task GetAddressLocation()
    {
        if (_isCheckingLocation)
        {
            await Shell.Current.DisplayAlert("Please wait", "Already checking address please wait", "OK");
            return;
        }

        try
        {
            _isCheckingLocation = true;
            StatusMessage = "Checking address...";
            StatusColor = Colors.Yellow;

            if (String.IsNullOrWhiteSpace(Property.Address))
            {
                await Shell.Current.DisplayAlert("Error", "Missing address please enter a valid one", "OK");
                return;
            }

            IEnumerable<Location> locations = await Geocoding.Default.GetLocationsAsync(Property.Address);

            Location location = locations?.FirstOrDefault();

            if (location is null)
            {
                await Shell.Current.DisplayAlert("Error", "No valid address was found. Please check your input and try again", "OK");
            }

            if (location != null)
            {
                Property.Latitude = location.Latitude;
                Property.Longitude = location.Longitude;
                OnPropertyChanged(nameof(Property));
            }

        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "Failed to fetch address. Please double check input", "OK");
        }
        finally
        {
            _isCheckingLocation = false;
            StatusMessage = string.Empty;
        }
    }

    private async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        if (e.NetworkAccess == NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("Internet Restored", "You are now connected to the internet.", "OK");
            InternetAvailable = true;
        }

        if (e.NetworkAccess != NetworkAccess.Internet)
        {
            await Shell.Current.DisplayAlert("No Internet", "You are not connected to the internet. Please check your connection and try again.", "OK");
            InternetAvailable = false;
        }
    }
}
