using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkin.ViewModels
{
    public class SummaryViewModel : BaseViewModel
    {
        private const string CheckSummaryKey = "CheckSummaryKey";
        private const string CalculatedResultKey = "CalculatedResult";
        private const string TimeEntriesKey = "TimeEntriesKey";

        private ObservableCollection<DateOnly> _checkinDates;
        public ObservableCollection<DateOnly> CheckinDates
        {
            get => _checkinDates;
            set => SetProperty(ref _checkinDates, value);
        }

        private ObservableCollection<DateOnly> _checkoutDates;
        public ObservableCollection<DateOnly> CheckoutDates
        {
            get => _checkoutDates;
            set => SetProperty(ref _checkoutDates, value);
        }

        private ObservableCollection<TimeOnly> _checkinTimes;
        public ObservableCollection<TimeOnly> CheckinTimes
        {
            get => _checkinTimes;
            set => SetProperty(ref _checkinTimes, value);
        }

        private ObservableCollection<TimeOnly> _checkoutTimes;
        public ObservableCollection<TimeOnly> CheckoutTimes
        {
            get => _checkoutTimes;
            set => SetProperty(ref _checkoutTimes, value);
        }

        public SummaryViewModel()
        {
            _ = PopulateData();
        }

        private async Task PopulateData()
        {
            try
            {
                var timeEntries = await SecureStorage.Default.GetAsync(TimeEntriesKey);
                var calculatedResult = await SecureStorage.Default.GetAsync(CalculatedResultKey);

                // get values for checkin/checkout dates
            }
            catch (Exception ex)
            {

            }
        }

        private void DoTheMath()
        {
            // pass this data throught the ctor
            TimeSpan timeSpanElapsed = TimeSpan.Parse(TotalElapsedTime);
            var totalTimeInMinutes = timeSpanElapsed.TotalMinutes;
            var minutesRate = HourlyRate / 60;
            CalculatedResult = Math.Round(totalTimeInMinutes * minutesRate, 2).ToString("F2");

            // we shouldn't need to save this data every second. Save it when the Result button is clicked
            //SecureStorage.Default.SetAsync(CalculatedResultKey, CalculatedResult);
        }
    }
}
