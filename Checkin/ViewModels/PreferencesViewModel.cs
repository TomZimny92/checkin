using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkin.ViewModels
{
    public class PreferencesViewModel : BaseViewModel
    {
        private const string HourlyRateKey = "HourlyRateKey";

        private double _hourlyRateInput;
        public double HourlyRateInput
        { 
            get => _hourlyRateInput; 
            set => SetProperty(ref _hourlyRateInput, value); 
        }

        public ICommand SavePreferencesCommand { get; }
        public ICommand CancelPreferencesCommand { get; }

        public PreferencesViewModel()
        {
            SavePreferencesCommand = new Command(async () => await ExecuteSavePreferences());
            CancelPreferencesCommand = new Command(async () => await ExecuteCancelPreferences());
            _ = InitializePreferences();
        }

        private async Task ExecuteSavePreferences()
        {
            try
            {
                if (HourlyRateInput != null && HourlyRateInput > 0)
                {
                    await SecureStorage.Default.SetAsync(HourlyRateKey, HourlyRateInput.ToString());
                }

                if (Application.Current != null)
                {
                    await Application.Current.Windows[0].Navigation.PopModalAsync();
                }
            }
            catch (Exception ex) { Console.WriteLine("test"); }
        }

        private async Task ExecuteCancelPreferences()
        {
            if (Application.Current != null)
            {
                await Application.Current.Windows[0].Navigation.PopModalAsync();
            }
        }

        private async Task InitializePreferences()
        {
            try
            {
                var existingPreference = await SecureStorage.Default.GetAsync(HourlyRateKey);
                if (double.TryParse(existingPreference, out double loadedPref))
                {
                    HourlyRateInput = loadedPref;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }
}
