using Gemini_Checkin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Gemini_Checkin.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private bool _isCheckedIn;
        public bool IsCheckedIn
        {
            get => _isCheckedIn;
            set => SetProperty(ref _isCheckedIn, value);
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

        public ICommand CheckinCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand ShowSummaryCommand { get; }
        public ICommand ResetCommand { get; }

        private IDispatcherTimer _clockTimer;

        public MainViewModel()
        {
            //TimeEntries = new ObservableCollection<TimeEntry>();
            //TotalElapsedTime = "Total Elapsed Time: 00:00:00"; // Initial state
            InitializeData();

            CheckinCommand = new Command(ExecuteCheckin, CanExecuteCheckin);
            CheckoutCommand = new Command(ExecuteCheckout, CanExecuteCheckout);
            ShowSummaryCommand = new Command(ExecuteShowSummary);
            ResetCommand = new Command(ExecuteReset);

            SetupClock();
            UpdateCommandStates(); // Initial command states
        }

        private async void InitializeData()
        {
            try
            {
                var timeEntries = await SecureStorage.Default.GetAsync("timeEntries");
                if (timeEntries != null) 
                { 
                    var formattedTimeEntries = FormatStorageData(timeEntries);
                    TimeEntries = formattedTimeEntries;
                }

                var totalElapsedTime = await SecureStorage.Default.GetAsync("totalElapsedTime");
                if (totalElapsedTime != null)
                {
                    TotalElapsedTime = totalElapsedTime;

                }

                var isCheckedIn = await SecureStorage.Default.GetAsync("isCheckedIn");
                if (isCheckedIn != null)
                { 
                    IsCheckedIn = Convert.ToBoolean(isCheckedIn);
                }
            }
            catch { }
        }

        private ObservableCollection<TimeEntry> FormatStorageData(string storedData)
        {
            var cm = new ObservableCollection<TimeEntry>();
            if (!string.IsNullOrEmpty(storedData))
            {
                cm = JsonSerializer.Deserialize<ObservableCollection<TimeEntry>>(storedData);
                if (cm == null)
                {
                    Console.WriteLine("Warning: Deserialized ContextModel is null. Returning new context.");
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
            SecureStorage.Default.SetAsync("timeEntries", JsonSerializer.Serialize(TimeEntries));
            SecureStorage.Default.SetAsync("isCheckedIn", IsCheckedIn.ToString());
            App.Current.MainPage.DisplayAlert("Check-in", $"Checked in at {DateTime.Now:HH:mm:ss}", "OK");
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
            SecureStorage.Default.SetAsync("timeEntries", JsonSerializer.Serialize(TimeEntries));
            SecureStorage.Default.SetAsync("isCheckedIn", IsCheckedIn.ToString());
            App.Current.MainPage.DisplayAlert("Check-out", $"Checked out at {checkoutTime:HH:mm:ss}", "OK");
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
            SecureStorage.Default.SetAsync("totalElapsedTime", TotalElapsedTime);
        }

        private void ExecuteReset()
        {
            TimeEntries.Clear();
            IsCheckedIn = false;
            TotalElapsedTime = "Total Elapsed Time: 00:00:00";
            UpdateCommandStates();
            SecureStorage.Default.SetAsync("timeEntries", JsonSerializer.Serialize(TimeEntries));
            SecureStorage.Default.SetAsync("isCheckedIn", IsCheckedIn.ToString());
            SecureStorage.Default.SetAsync("totalElapsedTime", TotalElapsedTime);
            App.Current.MainPage.DisplayAlert("Reset", "All time entries have been cleared.", "OK");
        }

        private void UpdateCommandStates()
        {
            ((Command)CheckinCommand).ChangeCanExecute();
            ((Command)CheckoutCommand).ChangeCanExecute();
        }
    }
}
