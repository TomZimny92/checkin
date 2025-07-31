using Checkin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkin.ViewModels
{
    public class MainViewModel : BaseViewModel
    {

        // SecureStorage Keys
        private const string IsCheckedInKey = "IsCheckedInKey";
        private const string TimeEntriesKey = "TimeEntriesKey";
        private const string TotalElapsedTimeKey = "TotalElapsedTimeKey";
        private const string HourlyRateKey = "HourlyRateKey";

        private bool _isCheckedIn;
        public bool IsCheckedIn
        {
            get => _isCheckedIn;
            set
            {
                if (SetProperty(ref _isCheckedIn, value))
                {
                    UpdateCommandStates();

                }
            }
        }

        private ObservableCollection<TimeEntry> _timeEntries;
        public ObservableCollection<TimeEntry> TimeEntries
        {
            get => _timeEntries;
            set => SetProperty(ref _timeEntries, value);
        }

        private string _currentTime;
        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        private string _totalElapsedTime;
        public string TotalElapsedTime
        {
            get => _totalElapsedTime;
            set => SetProperty(ref _totalElapsedTime, value);
        }

        private double _hourlyRate;
        public double HourlyRate
        {
            get => _hourlyRate;
            set => SetProperty(ref _hourlyRate, value);
        }

        public ICommand CheckinCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand ShowSummaryCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand PreferencesCommand { get; }

        private IDispatcherTimer _clockTimer;

        public MainViewModel()
        {
            //TimeEntries = new ObservableCollection<TimeEntry>();
            //TotalElapsedTime = "Total Elapsed Time: 00:00:00"; // Initial state

            CheckinCommand = new Command(ExecuteCheckin, CanExecuteCheckin);
            CheckoutCommand = new Command(ExecuteCheckout, CanExecuteCheckout);
            //ShowSummaryCommand = new Command(ExecuteShowSummary);
            ShowSummaryCommand = new Command(ExecuteShowResult);
            ResetCommand = new Command(ExecuteReset);
            PreferencesCommand = new Command(async () => await ExecutePreferences());

            _ = InitializeData();
            SetupClock();
            //UpdateCommandStates(); // Initial command states
        }

        private async Task InitializeData()
        {
            try
            {
                var timeEntries = await SecureStorage.Default.GetAsync(TimeEntriesKey);
                if (!string.IsNullOrEmpty(timeEntries))
                {
                    var formattedTimeEntries = FormatStorageData(timeEntries);
                    if (formattedTimeEntries != null)
                    {
                        //TimeEntries.Clear();
                        TimeEntries = formattedTimeEntries;
                    }

                }
                else
                {
                    TimeEntries = new ObservableCollection<TimeEntry>();
                }

                var totalElapsedTime = await SecureStorage.Default.GetAsync(TotalElapsedTimeKey);
                if (!string.IsNullOrEmpty(totalElapsedTime))
                {
                    TotalElapsedTime = totalElapsedTime;

                }
                else
                {
                    TotalElapsedTime = "Total Elapsed Time: 00:00:00";
                }

                var isCheckedIn = await SecureStorage.Default.GetAsync(IsCheckedInKey);
                if (bool.TryParse(isCheckedIn, out bool loadedIsCheckedIn))
                {
                    IsCheckedIn = loadedIsCheckedIn;
                }
                else
                {
                    IsCheckedIn = false;
                }

                var hourlyRate = await SecureStorage.Default.GetAsync(HourlyRateKey);
                if (double.TryParse(hourlyRate, out double loadedRate))
                {
                    HourlyRate = loadedRate;
                }
                else
                {
                    HourlyRate = 0.0; // Default if not found or invalid
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"some of the data failed to load: {ex}");
            }
            finally
            {
                UpdateCommandStates();
            }
        }

        private static ObservableCollection<TimeEntry> FormatStorageData(string storedData)
        {
            var cm = new ObservableCollection<TimeEntry>();
            if (!string.IsNullOrEmpty(storedData))
            {
                cm = JsonSerializer.Deserialize<ObservableCollection<TimeEntry>>(storedData);
                if (cm == null)
                {
                    Console.WriteLine("Warning: Deserialized TimeEntry is null. Returning new context.");
                }
                return cm;
            }
            else
            {
                Console.WriteLine("Warning: Stored data is empty or null. Returning new context.");
                return cm;
            }
        }

        private void SetupClock()
        {
            // probably need to have Timer dependently injected
            _clockTimer = Application.Current.Dispatcher.CreateTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) =>
            {
                CurrentTime = DateTime.Now.ToString("HH:mm:ss");
                // Update summary continuously if checked in
                if (IsCheckedIn)
                {
                    ExecuteShowSummary();
                }
            };
            _clockTimer.Start();
        }

        private void ExecuteCheckin()
        {
            IsCheckedIn = true;
            TimeEntries.Add(new TimeEntry { CheckinTime = DateTime.Now, CheckoutTime = null });
            UpdateCommandStates();
            SecureStorage.Default.SetAsync(TimeEntriesKey, JsonSerializer.Serialize(TimeEntries));
            SecureStorage.Default.SetAsync(IsCheckedInKey, IsCheckedIn.ToString());
            //App.Current.MainPage.DisplayAlert("Check-in", $"Checked in at {DateTime.Now:HH:mm:ss}", "OK");
        }

        private bool CanExecuteCheckin()
        {
            return !IsCheckedIn;
        }

        private void ExecuteCheckout()
        {
            IsCheckedIn = false;
            DateTime checkoutTime = DateTime.Now;

            // Find the last open check-in entry and update it
            var lastEntry = TimeEntries.LastOrDefault(e => !e.CheckoutTime.HasValue);
            if (lastEntry != null)
            {
                lastEntry.CheckoutTime = checkoutTime;
            }
            else
            {
                // Fallback: If somehow checkout is clicked without a check-in,
                // add a placeholder entry.
                // I don't like this. Change it.
                TimeEntries.Add(new TimeEntry { CheckinTime = DateTime.MinValue, CheckoutTime = checkoutTime });
            }

            UpdateCommandStates();
            ExecuteShowSummary(); // Update summary immediately after checkout
            SecureStorage.Default.SetAsync(TimeEntriesKey, JsonSerializer.Serialize(TimeEntries));
            SecureStorage.Default.SetAsync(IsCheckedInKey, IsCheckedIn.ToString());
            //App.Current.MainPage.DisplayAlert("Check-out", $"Checked out at {checkoutTime:HH:mm:ss}", "OK");
        }

        private bool CanExecuteCheckout()
        {
            return IsCheckedIn;
        }

        private void ExecuteShowSummary()
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (var entry in TimeEntries)
            {
                if (entry.CheckoutTime.HasValue)
                {
                    total += entry.Duration;
                }
                else if (IsCheckedIn && entry == TimeEntries.LastOrDefault(e => !e.CheckoutTime.HasValue))
                {
                    // If currently checked in, add duration from check-in to now for the active session
                    total += (DateTime.Now - entry.CheckinTime);
                }
            }
            TotalElapsedTime = $"Total Elapsed Time: {total:hh\\:mm\\:ss}";
            SecureStorage.Default.SetAsync(TotalElapsedTimeKey, TotalElapsedTime);
        }

        private void ExecuteShowResult()
        {
            // HourlyRate * ElapsedTime
            // TotalElapsedTime needs to be parsed to remove the leading string
            TimeSpan timeSpanElapsed = TimeSpan.Parse(TotalElapsedTime);
            var result = HourlyRate * timeSpanElapsed;
            App.Current.MainPage.DisplayAlert("Result", $"You owe: ${result}", "Ugh");
        }

        private void ExecuteReset()
        {
            TimeEntries.Clear();
            IsCheckedIn = false;
            TotalElapsedTime = "Total Elapsed Time: 00:00:00";
            UpdateCommandStates();
            SecureStorage.Default.SetAsync(TimeEntriesKey, JsonSerializer.Serialize(TimeEntries));
            SecureStorage.Default.SetAsync(IsCheckedInKey, IsCheckedIn.ToString());
            SecureStorage.Default.SetAsync(TotalElapsedTimeKey, TotalElapsedTime);
            //App.Current.MainPage.DisplayAlert("Reset", "All time entries have been cleared.", "OK");
        }

        private async Task ExecutePreferences()
        {
            // show the modal
            try
            {
                var preferencesPage = new PreferencesPage();
                var preferencesViewModel = new PreferencesViewModel();
                preferencesPage.BindingContext = preferencesViewModel;
                if (Application.Current != null)
                {
                    await Application.Current.Windows[0].Navigation.PushModalAsync(preferencesPage);
                }
                
                //await Application.Current.MainPage.Navigation.PushModalAsync(preferencesPage); 
                var rate = await SecureStorage.Default.GetAsync(HourlyRateKey);
                if (double.TryParse(rate, out double loadedRate))
                {
                    HourlyRate = loadedRate;
                }
                else
                {
                    HourlyRate = 0.0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hourly rate: {ex.Message}");
                HourlyRate = 0.0;
            }


        }

        private async Task SaveDataAsync()
        {
            try
            {
                await SecureStorage.Default.SetAsync(TimeEntriesKey, JsonSerializer.Serialize(TimeEntries));
                await SecureStorage.Default.SetAsync(IsCheckedInKey, IsCheckedIn.ToString());
                await SecureStorage.Default.SetAsync(TotalElapsedTimeKey, TotalElapsedTime);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void UpdateCommandStates()
        {
            ((Command)CheckinCommand).ChangeCanExecute();
            ((Command)CheckoutCommand).ChangeCanExecute();
        }
    }
}
