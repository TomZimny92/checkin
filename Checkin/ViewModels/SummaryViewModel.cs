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
        private const string HourlyRateKey = "HourlyRateKey";
        //private const string CalculatedResultKey = "CalculatedResult"; pass through param
        //private const string TimeEntriesKey = "TimeEntriesKey"; pass through param


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

        private string _summaryElapsedTime;
        public string SummaryElapsedTime
        {
            get => _summaryElapsedTime;
            set => SetProperty(ref _summaryElapsedTime, value);
        }

        private double _summaryHourlyRate;
        public double SummaryHourlyRate
        {
            get => _summaryHourlyRate;
            set => SetProperty(ref _summaryHourlyRate, value);
        }



        public SummaryViewModel(string totalElapsedTime, ObservableCollection<TimeEntry> timeEntries)
        {
            _summaryElapsedTime = totalElapsedTime;
            _summaryTimeEntries = timeEntries;
            _ = PopulateData(); // pulls data from SecureStorage
            DoTheMath(); // calculates the result
            FormatTimeEntryData();
        }

        private async Task PopulateData()
        {
            try
            {
                var storedHourlyRate = await SecureStorage.Default.GetAsync(HourlyRateKey);
                if (double.TryParse(storedHourlyRate, out double rate))
                {
                    _summaryHourlyRate = rate;
                }
                // get values for checkin/checkout dates
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void DoTheMath()
        {
            // pass this data throught the ctor
            TimeSpan timeSpanElapsed = TimeSpan.Parse(_summaryElapsedTime);
            var totalTimeInMinutes = timeSpanElapsed.TotalMinutes;
            var minutesRate = _summaryHourlyRate / 60;
            CalculatedResult = Math.Round(totalTimeInMinutes * minutesRate, 2).ToString("F2");

            // we shouldn't need to save this data every second. Save it when the Result button is clicked
            //SecureStorage.Default.SetAsync(CalculatedResultKey, CalculatedResult);
        }

        private void FormatTimeEntryData()
        {

        }

    }
}
