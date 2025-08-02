using Checkin.Models;
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


        private string _calculatedResult;
        public string CalculatedResult
        {
            get => _calculatedResult;
            set => SetProperty(ref _calculatedResult, value);
        }

        private ObservableCollection<TimeEntry> _summaryTimeEntries;
        public ObservableCollection<TimeEntry> SummaryTimeEntries 
        {
            get => _summaryTimeEntries;
            set => SetProperty(ref _summaryTimeEntries, value);
        }

        public SummaryViewModel(string totalElapsedTime, double hourlyRate)
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
