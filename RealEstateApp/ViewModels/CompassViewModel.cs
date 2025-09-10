using RealEstateApp.Models;
using RealEstateApp.Services;

namespace RealEstateApp.ViewModels
{
    [QueryProperty(nameof(Property), "MyProperty")]
    public class CompassViewModel : BaseViewModel
    {
        #region properties
        private Property _property;
        public Property Property
        {
            get => _property;
            set
            {
                SetProperty(ref _property, value);
            }
        }

        private double _currentHeading;
        public double CurrentHeading
        {
            get => _currentHeading;
            set
            {
                SetProperty(ref _currentHeading, value);
            }
        }

        private string _currentAspect;
        public string CurrentAspect
        {
            get => _currentAspect;
            set
            {
                SetProperty(ref _currentAspect, value);
            }
        }

        private double _rotationAngle;
        public double RotationAngle
        {
            get => _rotationAngle;
            set
            {
                SetProperty(ref _rotationAngle, value);
            }
        }

        #endregion

        readonly IPropertyService service;

        public CompassViewModel(IPropertyService propertyService)
        {
            service = propertyService;

            if (!Compass.IsMonitoring)
            {
                Compass.Default.ReadingChanged += Compass_ReadingChanged;
                Compass.Default.Start(SensorSpeed.UI);
            }
        }

        public void UnSubscribeOnCompass()
        {
            if (Compass.IsMonitoring)
            {
                Compass.Default.Stop();
                Compass.Default.ReadingChanged -= Compass_ReadingChanged;
            }

            Property.Aspect = CurrentAspect;
            service.SaveProperty(Property);
            OnPropertyChanged(nameof(Property));
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            CurrentHeading = e.Reading.HeadingMagneticNorth;
            RotationAngle = 360 - e.Reading.HeadingMagneticNorth;
            SetPropertyAspectToClosestDirection();
        }

        public void SetPropertyAspectToClosestDirection()
        {
            if (Property == null)
                return;

            double heading = CurrentHeading % 360;
            if (heading < 0) heading += 360;

            string aspect = heading switch
            {
                >= 315 or < 45 => "North",
                >= 45 and < 135 => "East",
                >= 135 and < 225 => "South",
                _ => "West"
            };

            CurrentAspect = aspect;
            OnPropertyChanged(nameof(Property));
        }
    }
}
