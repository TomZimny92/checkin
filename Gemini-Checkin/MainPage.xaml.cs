using System.Collections.ObjectModel;
using System.Text.Json;

namespace Gemini_Checkin
{
    public partial class MainPage : ContentPage
    {
        private bool _isCheckedIn = false;
        private DateTime _lastCheckinTime;
        private ObservableCollection<(DateTime Checkin, DateTime? Checkout)> _timeEntries;
        private IDispatcherTimer _clockTimer;

        public MainPage()
        {
            InitializeComponent();
            _timeEntries = new ObservableCollection<(DateTime Checkin, DateTime? Checkout)>();
            SetupClock();
            LoadData();
            UpdateButtonsState();
        }

        private void SetupClock()
        {
            _clockTimer = Dispatcher.CreateTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1);
            _clockTimer.Tick += (s, e) =>
            {
                ClockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            _clockTimer.Start();
        }

        private void LoadData()
        {
            try
            {
                var timeLog = SecureStorage.Default.GetAsync("timeEntries");
                var isCheckedIn = SecureStorage.Default.GetAsync("isCheckedin");

                if (timeLog.Result != null)
                {
                    var formattedTimeLog = JsonSerializer.Deserialize<ObservableCollection<(DateTime Checkin, DateTime? Checkout)>>(timeLog.Result);
                    _timeEntries = formattedTimeLog;
                }
                if (isCheckedIn.Result != null)
                {
                    var formattedIsCheckedIn = JsonSerializer.Deserialize<bool>(isCheckedIn.Result);
                    _isCheckedIn = formattedIsCheckedIn;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void UpdateButtonsState()
        {
            if (_isCheckedIn)
            {
                CheckinButton.IsEnabled = false;
                CheckinButton.BackgroundColor = Colors.Gray;

                CheckoutButton.IsEnabled = true;
                CheckoutButton.BackgroundColor = Colors.Red;
            }
            else
            {
                CheckinButton.IsEnabled = true;
                CheckinButton.BackgroundColor = Colors.Green;

                CheckoutButton.IsEnabled = false;
                CheckoutButton.BackgroundColor = Colors.Gray;
            }
        }

        private void CheckinButton_Clicked(object sender, EventArgs e)
        {
            // timeEntries
            // isCheckedin
            
            _isCheckedIn = true;
            _lastCheckinTime = DateTime.Now;
            _timeEntries.Add((_lastCheckinTime, null));
            SecureStorage.Default.SetAsync("timeEntries", JsonSerializer.Serialize(_timeEntries));
            SecureStorage.Default.SetAsync("isCheckedin", JsonSerializer.Serialize(_isCheckedIn));
            UpdateButtonsState();
            DisplayAlert("Check-in", $"Checked in at {_lastCheckinTime:HH:mm:ss}", "OK");
        }

        private void CheckoutButton_Clicked(object sender, EventArgs e)
        {
            _isCheckedIn = false;
            DateTime checkoutTime = DateTime.Now;

            // Find the last open check-in entry and update it
            if (_timeEntries.Any() && !_timeEntries.Last().Checkout.HasValue)
            {
                var lastEntry = _timeEntries.Last();
                _timeEntries[_timeEntries.Count - 1] = (lastEntry.Checkin, checkoutTime);
            }
            else
            {
                // This case should ideally not happen if logic is followed,
                // but as a fallback, add a new entry if somehow checkout is clicked
                // without a corresponding check-in.
                _timeEntries.Add((DateTime.MinValue, checkoutTime)); // Use MinValue for a placeholder check-in
            }

            SecureStorage.Default.SetAsync("timeEntries", JsonSerializer.Serialize(_timeEntries));
            SecureStorage.Default.SetAsync("isCheckedin", JsonSerializer.Serialize(_isCheckedIn));
            UpdateButtonsState();
            DisplayAlert("Check-out", $"Checked out at {checkoutTime:HH:mm:ss}", "OK");
        }

        private void SummaryButton_Clicked(object sender, EventArgs e)
        {
            TimeSpan totalElapsedTime = TimeSpan.Zero;
            foreach (var entry in _timeEntries)
            {
                if (entry.Checkout.HasValue)
                {
                    totalElapsedTime += (entry.Checkout.Value - entry.Checkin);
                }
                else if (_isCheckedIn && entry.Checkin == _lastCheckinTime)
                {
                    // If currently checked in, include the duration of the current session
                    totalElapsedTime += (DateTime.Now - entry.Checkin);
                }
            }

            SummaryLabel.Text = $"Total Elapsed Time: {totalElapsedTime:hh\\:mm\\:ss}";
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            _timeEntries.Clear();
            _isCheckedIn = false;
            _lastCheckinTime = DateTime.MinValue; // Reset last check-in time
            UpdateButtonsState();
            SummaryLabel.Text = "Total Elapsed Time: 00:00:00";
            DisplayAlert("Reset", "All time entries have been cleared.", "OK");
        }
    }
}
