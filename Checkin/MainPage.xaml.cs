using Checkin.Models;

namespace Checkin;

public partial class MainPage : ContentPage
{
    private IDispatcherTimer _timer;
    private Boolean CheckedIn;
    private DateTime ElapsedTime;
    private List<DateTime> ContextCheckLog;
    private List<ContextModel> ContextModels;
    //private string TestLabel;

    public MainPage()
    {
        InitializeComponent();
        LaunchSetup();
        _timer = Application.Current.Dispatcher.CreateTimer();
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

    private void LaunchSetup()
    {
        // if we find items in the local storage, populate the necessary objects
            // 
        var defaultContext = new ContextModel() { 
            Id = 1,
            Name = "Default",
            Checks = [],
        }; 
        ContextModels.Add(defaultContext);
    }

    private void Checkin(object? sender, EventArgs e)
    {
        CheckedIn = true;
        var timeStamp = new CheckModel()
        {
            CheckedIn = CheckedIn,
            CheckedTime = DateTime.Now,
        };
        ContextModels[0].Checks?.Add(timeStamp);
        // save timestamp to ContextCheckLog
        // when holding down, provide option to select custom Checkin time
    }

    private void Checkout(object? sender, EventArgs e)
    {
        CheckedIn = false;
        var timeStamp = new CheckModel()
        {
            CheckedIn = CheckedIn,
            CheckedTime = DateTime.Now,
        };
        ContextModels[0].Checks?.Add(timeStamp);
        // save timestamp to ContextCheckLog
        // when holding down, provide option to select custom Checkout time
    }

    private void GetSummary(object? sender, EventArgs e)
    {
        var dateTime1 = ContextModels[0]?.Checks[1].CheckedTime;
        var dateTime2 = ContextModels[0]?.Checks[0].CheckedTime;
        var result = dateTime2 - dateTime1;
        TestLabel.Text = result.ToString();
        // if still checked in, show the current live time
        // Nice to Have: also update the live preferences (money, rate, etc)
        // otherwise, show calculated time and the calculated preference
    }
}