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
        private int _hourlyRateInput;
        public int HourlyRateInput
        { 
            get => _hourlyRateInput; 
            set => SetProperty(ref _hourlyRateInput, value); 
        }

        public ICommand SavePreferencesCommand { get; }
        public ICommand CancelPreferencesCommand { get; }

        public PreferencesViewModel()
        {
            SavePreferencesCommand = new Command(ExecuteSavePreferences);
            CancelPreferencesCommand = new Command(ExecuteCancelPreferences);         
            InitializePreferences();
        }

        private void ExecuteSavePreferences()
        {
            Console.WriteLine("test");
        }

        private void ExecuteCancelPreferences()
        {
            Console.WriteLine("test");
        }

        private void InitializePreferences()
        {

        }

    }
}
