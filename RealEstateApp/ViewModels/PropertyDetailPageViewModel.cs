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

    private Command openMailCommand;
    public ICommand OpenMailCommand => openMailCommand ??= new Command<string>(async (email) => await OpenMail(email));

    async Task OpenMail(string email)
    {
        if (Email.Default.IsComposeSupported)
        {

            string subject = "Mail to Vendor!";
            string body = "It was great to see you last weekend.";
            string[] recipients = new[] { email };

            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                BodyFormat = EmailBodyFormat.PlainText,
                To = new List<string>(recipients)
            };

            await Email.Default.ComposeAsync(message);
        }
    }

    private Command phoneDialogCommand;
    public ICommand PhoneDialogCommand => phoneDialogCommand ??= new Command<string>(async (phonenumber) => await OpenPhoneDialog(phonenumber));

    async Task OpenPhoneDialog(string phonenumber)
    {
        string action = await Shell.Current.DisplayActionSheet(phonenumber, "Cancel", null, "Call", "SMS");

        switch (action)
        {
            case "Call":
                if (PhoneDialer.Default.IsSupported)
                    PhoneDialer.Default.Open(phonenumber);
                break;
            case "SMS":
                if (Sms.Default.IsComposeSupported)
                {
                    string[] recipients = new[] { phonenumber };
                    string text = "Hello, I'm interested in buying your propery";

                    var message = new SmsMessage(text, recipients);

                    await Sms.Default.ComposeAsync(message);
                }
                break;
            default:
                break;
        }
    }

}
