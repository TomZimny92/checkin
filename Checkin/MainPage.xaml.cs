namespace Checkin;

public partial class MainPage : ContentPage
{
    int count = 0;
    private IDispatcherTimer _timer;
    private Boolean CheckedIn;
    private DateTime ElapsedTime;

    public MainPage()
    {
        InitializeComponent();
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

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private void Checkin(object? sender, EventArgs e)
    {
        CheckedIn = true;

    }
}