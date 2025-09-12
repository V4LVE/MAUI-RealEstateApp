using RealEstateApp.Models;
using RealEstateApp.Services;
using System.Windows.Input;

namespace RealEstateApp.ViewModels
{
    public class LoginPageViewModel(IPropertyService service) : BaseViewModel
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }

        private Command loginCommand;
        public ICommand LoginCommand => loginCommand ??= new Command(async () => await Login());
        async Task Login()
        {
            LoginResult result = service.LoginAsync(Username, Password);

            if (!result.Succeded)
            {
                await Shell.Current.DisplayAlert("Login Failed", "Invalid username or password", "OK");
                return;
            }

            await SecureStorage.Default.SetAsync("access_token", result.AccessToken);
            await SecureStorage.Default.SetAsync("refresh_token", result.RefreshToken);

            var secureToken = await SecureStorage.GetAsync("access_token");

            await Shell.Current.DisplayAlert("Login Success", $"Login was a success. Access Token {secureToken}", "OK");
        }
    }
}
