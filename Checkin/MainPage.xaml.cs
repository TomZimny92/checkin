

using Checkin.Models;
using Checkin.ViewModel;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checkin;

public partial class MainPage : ContentPage
{
    private IDispatcherTimer _timer;
    //public ContextModel CurrentContext {  get; set; }

    public MainPage(BaseViewModel viewModel, IDispatcherTimer timer)
    {
        InitializeComponent();
        //BindingContext = viewModel;
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

    private void Checkin(object sender, EventArgs e)
    {
        CheckinButton.IsEnabled = false;
        CheckoutButton.IsEnabled = true;
        CheckinButton.BackgroundColor = Colors.Gray;
        CheckoutButton.BackgroundColor = Colors.Red;
    }

    private void Checkout(object sender, EventArgs e)
    {
        CheckinButton.IsEnabled = true;
        CheckoutButton.IsEnabled = false;
        CheckinButton.BackgroundColor = Colors.Green;
        CheckoutButton.BackgroundColor = Colors.Gray;
    }
}