using Checkin.Models;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace Checkin.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const string IsCheckedInKey = "IsCheckedInKey";
        private const string TimeEntriesKey = "TimeEntriesKey";
        private const string TotalElapsedTimeKey = "TotalElapsedTimeKey";
        private const string DoBucketKey = "DoBucketKey";

        private DateTime _minDatePickerValue;
        public DateTime MinDatePickerValue
        {
            get => _minDatePickerValue;
            set => SetProperty(ref _minDatePickerValue, value);
        }

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

        private bool _showManualEntry;
        public bool ShowManualEntry
        {
            get => _showManualEntry;
            set
            {
                if (SetProperty(ref _showManualEntry, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        private DateTime _manualDate;
        public DateTime ManualDate
        {
            get => _manualDate;
            set => SetProperty(ref _manualDate, value);
        }

        private TimeSpan _manualTime;
        public TimeSpan ManualTime
        {
            get => _manualTime;
            set => SetProperty(ref _manualTime, value);
                
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

        private Stack<DateTime> _doBucket;
        public Stack<DateTime> DoBucket
        {
            get => _doBucket;
            set => SetProperty(ref _doBucket, value);
        }

        private bool _canUndo;
        public bool CanUndo
        {
            get => _canUndo;
            set => SetProperty(ref _canUndo, value);
        }

        private bool _canRedo;
        public bool CanRedo
        {
            get => _canRedo;
            set => SetProperty(ref _canRedo, value);
        }

        public ICommand CheckinCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand ShowSummaryCommand { get; }
        public ICommand ShowManualEntryCommand { get; }
        public ICommand SaveManualEntryCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand PreferencesCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        private IDispatcherTimer? _clockTimer;

        public MainViewModel()
        {
            CheckoutCommand = new Command(async () => await ExecuteCheckout(), CanExecuteCheckout);
            CheckinCommand = new Command(async () => await ExecuteCheckin(), CanExecuteCheckin);
            SaveManualEntryCommand = new Command(async () => await ExecuteSaveManualEntry());
            UndoCommand = new Command(async () => await ExecuteUndo());
            RedoCommand = new Command(async () => await ExecuteRedo());
            PreferencesCommand = new Command(async () => await ExecutePreferences());
            ShowSummaryCommand = new Command(async () => await ExecuteShowResult());
            ResetCommand = new Command(async () => await ExecuteReset());
            ShowManualEntryCommand = new Command(ExecuteShowManualEntry);

            _ = InitializeData();
            SetupClock();
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
                        TimeEntries = formattedTimeEntries;
                    }
                }
                else
                {
                    TimeEntries = [];
                }

                var totalElapsedTime = await SecureStorage.Default.GetAsync(TotalElapsedTimeKey);
                if (!string.IsNullOrEmpty(totalElapsedTime))
                {
                    TotalElapsedTime = totalElapsedTime;
                }
                else
                {
                    TotalElapsedTime = "00:00:00";
                }

                var doBucket = await SecureStorage.Default.GetAsync(DoBucketKey);
                if (!string.IsNullOrEmpty(doBucket))
                {
                    var formattedDoBucket = JsonSerializer.Deserialize<Stack<DateTime>>(doBucket);
                    if (formattedDoBucket != null)
                    {
                        DoBucket = formattedDoBucket;
                    }
                }
                else
                {
                    DoBucket = new Stack<DateTime>();
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

            }
            catch (Exception ex)
            {
                Console.WriteLine($"some of the data failed to load: {ex}");
            }
            finally
            {
                MinDatePickerValue = DateTime.Now;
                ManualDate = MinDatePickerValue;
                UpdateCommandStates();
            }
        }

        private static ObservableCollection<TimeEntry> FormatStorageData(string storedData)
        {
            var cm = new ObservableCollection<TimeEntry>();
            if (!string.IsNullOrEmpty(storedData))
            {
                cm = JsonSerializer.Deserialize<ObservableCollection<TimeEntry>>(storedData);
                if (cm != null)
                {
                    return cm;
                }
                else
                {
                    Console.WriteLine("Warning: Deserialized TimeEntry is null. Returning new context.");
                    return [];
                }
            }
            else
            {
                Console.WriteLine("Warning: Stored data is empty or null. Returning new context.");
                return cm;
            }
        }

        private void SetupClock()
        {
            _clockTimer = App.Current?.Dispatcher.CreateTimer();
            if (_clockTimer != null)
            {
                _clockTimer.Interval = TimeSpan.FromSeconds(1);
                _clockTimer.Tick += (s, e) =>
                {
                    CurrentTime = DateTime.Now.ToString("HH:mm:ss");
                    // Update summary continuously if checked in
                    if (IsCheckedIn)
                    {
                        CalculateElapsedTime();
                    }
                };
                _clockTimer.Start();
            }
        }

        private async Task ExecuteCheckin()
        {
            IsCheckedIn = true;
            if (TimeEntries != null)
            {
                TimeEntries.Add(new TimeEntry { CheckinTime = DateTime.Now, CheckoutTime = null });
                DoBucket.Clear();
                UpdateCommandStates();
                await SaveDataAsync();
            }
            else
            {
                Console.WriteLine("ExecuteCheckin blew up");
            }
        }

        private bool CanExecuteCheckin()
        {
            return !IsCheckedIn;
        }

        private async Task ExecuteCheckout()
        {
            IsCheckedIn = false;
            DateTime checkoutTime = DateTime.Now;

            if (TimeEntries != null)
            {
                var lastEntry = TimeEntries.LastOrDefault(e => !e.CheckoutTime.HasValue);
                if (lastEntry != null)
                {
                    lastEntry.CheckoutTime = checkoutTime;
                }
                else
                {
                    // Fallback: If somehow checkout is clicked without a check-in,
                    // add a placeholder entry.
                    // I don't like this. You're probably using it wrong. Here, let me show you.
                    TimeEntries.Add(new TimeEntry { CheckinTime = DateTime.MinValue, CheckoutTime = checkoutTime });
                }

                DoBucket.Clear();
                UpdateCommandStates();
                CalculateElapsedTime();
                await SaveDataAsync();
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        private bool CanExecuteCheckout()
        {
            return IsCheckedIn;
        }

        private void CalculateElapsedTime()
        {
            TimeSpan total = TimeSpan.Zero;
            if (TimeEntries != null)
            {
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

                TotalElapsedTime = FormatElapsedTime(total);
                SecureStorage.Default.SetAsync(TotalElapsedTimeKey, TotalElapsedTime);
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        private static string FormatElapsedTime(TimeSpan time)
        {
            var hoursActual = (time.Days * 24) + time.Hours;
            var sbHours = new StringBuilder(hoursActual.ToString());
            var sbMinutes = new StringBuilder(time.Minutes.ToString());
            var sbSeconds = new StringBuilder(time.Seconds.ToString());

            if (hoursActual < 10)
            {
                sbHours.Insert(0, "0");
            }                 
            if (time.Minutes < 10)
            {
                sbMinutes.Insert(0, "0");
            }
            if (time.Seconds < 10)
            {
                sbSeconds.Insert(0, "0");
            }
            var totalTime = $"{sbHours}:{sbMinutes}:{sbSeconds}";
            return totalTime;
        }

        private async Task ExecuteShowResult()
        {
            try
            {
                if (TotalElapsedTime != null && TimeEntries != null)
                {
                    var summaryPage = new SummaryPage();
                    var summaryViewModel = new SummaryViewModel(TotalElapsedTime, TimeEntries);
                    summaryPage.BindingContext = summaryViewModel;

                    if (Application.Current != null)
                    {
                        await Application.Current.Windows[0].Navigation.PushModalAsync(summaryPage);
                    }
                }
                else
                {
                    App.Current?.Windows[0]?.Page?.DisplayAlert("Summary", "Elapsed Time or Time Entries not found. There's nothing to report.", "OK");                                        
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void ExecuteShowManualEntry()
        {
            ShowManualEntry = !ShowManualEntry;
        }

        private async Task ExecuteSaveManualEntry()
        {
            DateOnly date = DateOnly.FromDateTime(ManualDate);
            TimeOnly time = TimeOnly.FromTimeSpan(ManualTime);

            var te = await SecureStorage.Default.GetAsync(TimeEntriesKey);

            if (te is null)
            {
                var newTimeEntries = new ObservableCollection<TimeEntry>();
                var newTimeEntry = new TimeEntry
                {
                    CheckinTime = new DateTime(date, time),
                    CheckoutTime = null,
                };
                newTimeEntries.Add(newTimeEntry);
                IsCheckedIn = true;
            }
            else
            {
                if (IsCheckedIn)
                {
                    var timeEntries = JsonSerializer.Deserialize<ObservableCollection<TimeEntry>>(te);
                    var lastEntry = timeEntries?[^1]; // fancy way to get the last index
                    if (lastEntry != null && timeEntries != null)
                    {
                        if (lastEntry.CheckoutTime is null)
                        {
                            lastEntry.CheckoutTime = new DateTime(date, time);
                            timeEntries[^1] = lastEntry;
                            TimeEntries = timeEntries;
                            IsCheckedIn = false;
                        }                        
                    }
                }
                else
                {
                    var newEntry = new TimeEntry
                    {
                        CheckinTime = new DateTime(date, time),
                        CheckoutTime = null
                    };
                    TimeEntries?.Add(newEntry);
                    IsCheckedIn = true;
                }
            }
            CalculateElapsedTime();
            UpdateCommandStates();
            await SaveDataAsync();
        }

        private async Task ExecuteReset()
        {
            if (TimeEntries != null)
            {
                TimeEntries.Clear();
                IsCheckedIn = false;
                TotalElapsedTime = "00:00:00";
                DoBucket.Clear();

                UpdateCommandStates();
                await SaveDataAsync();

                App.Current?.Windows[0]?.Page?.DisplayAlert("Reset", "All time entries have been cleared.", "OK");
            }
            else
            {
                throw new NullReferenceException();
            }
        }

        private async Task ExecutePreferences()
        {
            try
            {
                var preferencesPage = new PreferencesPage();
                var preferencesViewModel = new PreferencesViewModel();
                preferencesPage.BindingContext = preferencesViewModel;
                if (Application.Current != null)
                {
                    await Application.Current.Windows[0].Navigation.PushModalAsync(preferencesPage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading hourly rate: {ex.Message}");
            }
        }

        private async Task ExecuteUndo()
        {
            var te = await SecureStorage.Default.GetAsync(TimeEntriesKey);
            if (te != null)
            {
                var timeEntries = JsonSerializer.Deserialize<ObservableCollection<TimeEntry>>(te);
                var lastEntry = timeEntries?.Count > 0 ? timeEntries[^1] : null;
                
                if (lastEntry != null)
                {
                    if (lastEntry.CheckoutTime is null)
                    {
                        DoBucket.Push(lastEntry.CheckinTime);
                        TimeEntries?.RemoveAt(TimeEntries.Count - 1);
                    }
                    else
                    {
                        DoBucket.Push((DateTime)lastEntry.CheckoutTime);
                        TimeEntries[^1].CheckoutTime = null;
                    }
                }
                else
                {
                    await App.Current?.Windows[0]?.Page?.DisplayAlert("Undo", "Nothing to undo.", "Ok");
                    return;
                }
            }

            IsCheckedIn = !IsCheckedIn;
            CalculateElapsedTime();
            UpdateCommandStates();
            await SaveDataAsync();
        }

        private async Task ExecuteRedo()
        {
            if (DoBucket.Count > 0)
            {
                var lastDo = DoBucket.Pop();
                var teCount = TimeEntries?.Count > 0 ? TimeEntries.Count : 0;
                var lastEntry = teCount > 0 ? TimeEntries[^1] : new TimeEntry();
                if (IsCheckedIn)
                {
                    lastEntry.CheckoutTime = lastDo;
                    TimeEntries?.RemoveAt(teCount - 1);
                    TimeEntries?.Add(lastEntry);
                }
                else
                {
                    var reEntry = new TimeEntry 
                    {
                        CheckinTime = lastDo,
                        CheckoutTime = null,
                    };
                    TimeEntries?.Add(reEntry);
                }
            }
            else
            {
                await App.Current?.Windows[0]?.Page?.DisplayAlert("Redo", "Nothing to redo", "Ok");
                return;
            }

            IsCheckedIn = !IsCheckedIn;
            CalculateElapsedTime();
            UpdateCommandStates();
            await SaveDataAsync();
        }

        private void SetDoState()
        {
            CanUndo = TimeEntries.Count > 0;
            CanRedo = DoBucket.Count > 0;
        }

        private async Task SaveDataAsync()
        {
            try
            {
                await SecureStorage.Default.SetAsync(TimeEntriesKey, JsonSerializer.Serialize(TimeEntries));
                await SecureStorage.Default.SetAsync(IsCheckedInKey, IsCheckedIn.ToString());
                await SecureStorage.Default.SetAsync(TotalElapsedTimeKey, TotalElapsedTime);
                await SecureStorage.Default.SetAsync(DoBucketKey, JsonSerializer.Serialize(DoBucket));
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
            ((Command)ShowManualEntryCommand).ChangeCanExecute();
            ((Command)UndoCommand).ChangeCanExecute();
            ((Command)RedoCommand).ChangeCanExecute();
            SetDoState();
        }
    }
}
