using Checkin.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace Checkin.ViewModels
{
    public class SummaryViewModel : BaseViewModel
    {
        private const string HourlyRateKey = "HourlyRateKey";
        //private const string CalculatedResultKey = "CalculatedResult"; pass through param
        //private const string TimeEntriesKey = "TimeEntriesKey"; pass through param


        private string? _calculatedResult;
        public string? CalculatedResult
        {
            get => (_calculatedResult ??= "");
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

        public ICommand CloseSummaryCommand { get; }

        public SummaryViewModel(string totalElapsedTime, ObservableCollection<TimeEntry> timeEntries)
        {
            _summaryElapsedTime = totalElapsedTime;
            _summaryTimeEntries = timeEntries;
            CloseSummaryCommand = new Command(async () => await ExecuteCloseSummary());
            _ = PopulateData(); // pulls data from SecureStorage
        }

        private async Task PopulateData()
        {
            try
            {
                var storedHourlyRate = await SecureStorage.Default.GetAsync(HourlyRateKey);

                if (double.TryParse(storedHourlyRate, out double rate))
                {
                    SummaryHourlyRate = rate;
                }
                // get values for checkin/checkout dates
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                DoTheMath(); // calculates the result
                FormatTimeEntryData();
            }
        }

        private void DoTheMath()
        {
            // pass this data throught the ctor
            var parsableElapsedTime = UnformatElapsedTime(_summaryElapsedTime);
            var timeSpanElapsed = TimeSpan.Parse(parsableElapsedTime);
            // TimeSpan is parsing incorrectly if hours > 23
            // will probably need to break the ElapsedTime apart and parse it then
            var totalTimeInMinutes = timeSpanElapsed.TotalMinutes;
            var minutesRate = _summaryHourlyRate / 60;
            CalculatedResult = Math.Round(totalTimeInMinutes * minutesRate, 2).ToString("F2");

            // we shouldn't need to save this data every second. Save it when the Result button is clicked
            //SecureStorage.Default.SetAsync(CalculatedResultKey, CalculatedResult);
        }

        private string UnformatElapsedTime(string elapsedTime)
        {
            var result = new string[4];
            var etSplit = elapsedTime.Split(':');
            
            if (int.TryParse(etSplit[0], out int hours))
            {
                result[0] = Math.Floor((double)hours / 24).ToString();
                result[1] = (hours % 24).ToString();
                result[2] = etSplit[1];
                result[3] = etSplit[2];
                return string.Join(':', result);
            }
            return elapsedTime;
        }

        private async Task ExecuteCloseSummary()
        {
            try
            {
                if (App.Current != null)
                {
                    await App.Current.Windows[0].Navigation.PopModalAsync();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void FormatTimeEntryData()
        {

        }

    }
}
