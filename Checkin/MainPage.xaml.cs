
using Checkin.Models;
using Checkin.ViewModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkin;

public partial class MainPage : ContentPage
{
    private IDispatcherTimer _timer;
    public ContextModel CurrentContext {  get; set; }
    //public List<ContextModel> ContextModels = [];

    public MainPage(MainViewModel viewModel, IDispatcherTimer timer)
    {
        InitializeComponent();
        LaunchSetup();
        BindingContext = viewModel;
        _timer = timer;
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        ClockDisplay.Text = DateTime.Now.ToString("HH:mm:ss");

    }
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer.Stop();
    }

    private async void LaunchSetup()
    {
        ContextModels = await GetContextData();
    }

    //IDispatcherTimer timer;
    //public CheckinViewModel(IDispatcherTimer timer)
    //{
    //    Properties = new ContextPropertiesModel();


    //}
    public bool CheckedOut { get; set; }
    public bool CheckedIn { get; set; }
    public ContextPropertiesModel Properties { get; set; }

    public required List<ContextModel> ContextModels { get; set; }

    private async Task<List<ContextModel>> GetContextData()
    {
        var cm = new List<ContextModel>();
        var dataCount = 1;
        //var storedData = await SecureStorage.Default.GetAsync("1");
        while (dataCount > 0)
        {
            var storedData = await SecureStorage.Default.GetAsync(dataCount.ToString());
            if (storedData != null)
            {
                var formattedData = FormatStorageData(storedData);
                cm.Add(formattedData);
                dataCount++;
            }
            else
            {
                dataCount = 0;
                cm.Add(InitializeContext());
                continue;
            }

        }
        return cm;
    }

    public ContextModel InitializeContext()
    {
        return new ContextModel()
        {
             Id = 1,
             Name = "Default",
             Checks = [],
             Icon = null,
        };
    }

    public ContextModel FormatStorageData(string storedData)
    {
        if (!string.IsNullOrEmpty(storedData))
        {
            ContextModel cm = JsonSerializer.Deserialize<ContextModel>(storedData);
            if (cm == null)
            {
                // possible error. TODO log this if it happens
                return InitializeContext();
            }
            return cm;
        }
        else
        {
            return InitializeContext();
        }
    }
    public async void Checkin(object? sender, EventArgs e)
    {
        CheckedIn = true;
        var timeStamp = new CheckModel()
        {
            CheckedIn = CheckedIn,
            CheckedTime = DateTime.Now,
        };
        ContextModels[0].Checks?.Add(timeStamp);
        await SecureStorage.Default.SetAsync(ContextModels[0].Id.ToString(), JsonSerializer.Serialize(ContextModels[0]));
        // save timestamp to ContextCheckLog
        // when holding down, provide option to select custom Checkin time
    }

    public async void Checkout(object? sender, EventArgs e)
    {
        CheckedIn = false;
        var timeStamp = new CheckModel()
        {
            CheckedIn = CheckedIn,
            CheckedTime = DateTime.Now,
        };
        ContextModels[0].Checks?.Add(timeStamp);
        await SecureStorage.Default.SetAsync(ContextModels[0].Id.ToString(), JsonSerializer.Serialize(ContextModels[0]));
        // save timestamp to ContextCheckLog
        // when holding down, provide option to select custom Checkout time
    }

    public void GetSummary(object? sender, EventArgs e)
    {
        List<DateTime> checkinTime = [];
        List<DateTime> checkoutTime = [];
        if (ContextModels[0].Checks != null)
        {
            foreach (var item in ContextModels[0].Checks)
            {
                if (item.CheckedIn)
                {
                    checkinTime.Add(item.CheckedTime);
                }
                else
                {
                    checkoutTime.Add(item.CheckedTime);
                }
            }
        }
        var shiftLog = new List<TimeSpan>();
        for (var i = 0; i < checkinTime.Count; i++)
        {
            var shift = checkoutTime[i] - checkinTime[i];
            shiftLog.Add(shift);
        }
        TimeSpan total = new();
        for (var i = 0; i < shiftLog.Count; i++)
        {
            total.Add(shiftLog[i]); // THIS IS NOT ADDING CORRECTLY
        }
        TestLabel.Text = total.ToString();
        // if still checked in, show the current live time
        // Nice to Have: also update the live preferences (money, rate, etc)
        // otherwise, show calculated time and the calculated preference
    }

    public bool IsCheckedIn(object? sender, EventArgs e)
    {
        var len = ContextModels[0]?.Checks.Count ?? 0;
        if (ContextModels[0]?.Checks[len]?.CheckedIn == true)
        {
            CheckedIn = true;
            CheckedOut = false;
            return CheckedIn;
        }
        else
        {
            CheckedIn = false;
            CheckedOut = true;
            return CheckedIn;
        }
    }



}