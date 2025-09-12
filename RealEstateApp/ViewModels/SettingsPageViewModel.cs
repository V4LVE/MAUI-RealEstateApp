namespace RealEstateApp.ViewModels
{
    public class SettingsPageViewModel : BaseViewModel
    {
        #region Properties
        public double Volume
        {
            get => Preferences.Default.Get("volume", 1.0);
            set
            {
                Preferences.Default.Set("volume", value);
                OnPropertyChanged(nameof(Volume));
            }
        }

        public double Pitch
        {
            get => Preferences.Default.Get("pitch", 1.0);
            set
            {
                Preferences.Default.Set("pitch", value);
                OnPropertyChanged(nameof(Pitch));
            }
        }
        #endregion
    }
}
