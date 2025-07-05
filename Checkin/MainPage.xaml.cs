using Checkin.Models;
using Checkin.ViewModel;

namespace Checkin;

public partial class MainPage : ContentPage
{
    private IDispatcherTimer _timer;
    //public List<ContextModel> ContextModels = [];

    public MainPage(MainViewModel viewModel, IDispatcherTimer timer)
    {
        InitializeComponent();
        //LaunchSetup();
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

    //private void LaunchSetup()
    //{
    //    // if we find items in the local storage, populate the necessary objects
    //        // 
    //    var defaultContext = new ContextModel() { 
    //        Id = 1,
    //        Name = "Default",
    //        Checks = [],
    //    }; 
    //    ContextModels.Add(defaultContext);
    //}

}