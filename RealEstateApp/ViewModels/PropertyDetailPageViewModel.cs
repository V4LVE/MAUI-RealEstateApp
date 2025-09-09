using RealEstateApp.Models;
using RealEstateApp.Services;
using RealEstateApp.Views;
using System.Diagnostics;
using System.Windows.Input;

namespace RealEstateApp.ViewModels;

[QueryProperty(nameof(PropertyListItem), "MyPropertyListItem")]
public class PropertyDetailPageViewModel : BaseViewModel
{
    private readonly IPropertyService service;
    public PropertyDetailPageViewModel(IPropertyService service)
    {
        this.service = service;
    }

    Property property;
    public Property Property { get => property; set { SetProperty(ref property, value); } }

    bool _isReading;

    public bool IsReading
    {
        get => _isReading;
        set => SetProperty(ref _isReading, value);
    }


    Agent agent;
    public Agent Agent { get => agent; set { SetProperty(ref agent, value); } }


    PropertyListItem propertyListItem;
    public PropertyListItem PropertyListItem
    {
        set
        {
            SetProperty(ref propertyListItem, value);

            Property = propertyListItem.Property;
            Agent = service.GetAgents().FirstOrDefault(x => x.Id == Property.AgentId);
        }
    }

    private Command editPropertyCommand;
    public ICommand EditPropertyCommand => editPropertyCommand ??= new Command<Property>(async (property) => await GotoEditProperty(property));
    async Task GotoEditProperty(Property property)
    {
        await Shell.Current.GoToAsync($"{nameof(AddEditPropertyPage)}?mode=editproperty", true, new Dictionary<string, object>
        {
            {"MyProperty", property }
        });
    }

    private CancellationTokenSource _ttsCts;


    private Command readDescriptionCommand;
    public ICommand ReadDescriptionCommand => readDescriptionCommand ??= new Command(async () => await ReadDescription());

    private Command stopReadingCommand;
    public ICommand StopReadingCommand => stopReadingCommand ??= new Command(StopReading);


    async private Task ReadDescription()
    {
        try
        {
            IsReading = true;
            _ttsCts = new CancellationTokenSource();
            IEnumerable<Locale> locales = await TextToSpeech.Default.GetLocalesAsync();
            var settings = new SpeechOptions()
            {
                Volume = 1.0f,
                Pitch = 1.0f,
                Locale = locales.FirstOrDefault()
            };
            await TextToSpeech.Default.SpeakAsync(Property.Description, settings, _ttsCts.Token);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Unable to read description: {ex.Message}");
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsReading = false;
            _ttsCts?.Dispose();
            _ttsCts = null;
        }
    }

    private void StopReading()
    {
        _ttsCts?.Cancel();
        _isReading = false;
    }
}
