using RealEstateApp.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RealEstateApp.ViewModels
{
    public class HeightCalculatorPageViewModel : BaseViewModel
    {
        #region Properties

        public ObservableCollection<BarometerMeasurement> Measurements { get; set; } = new();

        private double _currentPressure;
        public double CurrentPressure
        {
            get => _currentPressure;
            set
            {
                SetProperty(ref _currentPressure, value);
            }
        }

        private double _currentAltitude;
        public double CurrentAltitude
        {
            get => _currentAltitude;
            set
            {
                SetProperty(ref _currentAltitude, value);
            }
        }

        private string _measurementLabel;
        public string MeasurementLabel
        {
            get => _measurementLabel;
            set
            {
                SetProperty(ref _measurementLabel, value);
            }
        }
        #endregion

        private const double seaLevelPressure = 1013.25;

        public HeightCalculatorPageViewModel()
        {
            if (!Barometer.Default.IsMonitoring)
            {
                // Turn on barometer
                Barometer.Default.ReadingChanged += Barometer_ReadingChanged;
                Barometer.Default.Start(SensorSpeed.UI);
            }
        }

        internal void UnsubscribeFromBarometer()
        {
            if (Barometer.Default.IsMonitoring)
            {
                Barometer.Default.ReadingChanged -= Barometer_ReadingChanged;
                Barometer.Default.Stop();
            }
        }

        private void Barometer_ReadingChanged(object sender, BarometerChangedEventArgs e)
        {
            CurrentPressure = e.Reading.PressureInHectopascals;
            CurrentAltitude = 44307.69396 * (1.0 - Math.Pow(CurrentPressure / seaLevelPressure, 0.190284));
        }

        private Command _saveMeasurementCommand;
        public ICommand SaveMeasurementCommand => _saveMeasurementCommand ??= new Command(() => SaveMeasurement());

        private void SaveMeasurement()
        {
            double heightChange = 0;
            double currentAltitude = CurrentAltitude;

            if (Measurements.Count > 0)
            {
                var lastMeasurement = Measurements.Last();
                heightChange = lastMeasurement.Altitude - currentAltitude;
            }


            var baroMeasurement = new BarometerMeasurement
            {
                Altitude = currentAltitude,
                Pressure = CurrentPressure,
                Label = MeasurementLabel,
                HeightChange = heightChange,
            };

            Measurements.Add(baroMeasurement);
        }
    }
}
