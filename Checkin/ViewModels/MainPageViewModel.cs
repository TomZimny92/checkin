using Checkin.Models;
using Checkin.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkin.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private ContextViewModel _selectedContext;
        private int _tabCounter = 0;

        public ObservableCollection<ContextViewModel> ContextItems { get; }

        public ContextViewModel SelectedContext
        {
            get => _selectedContext;
            set
            {
                if (SetProperty(ref _selectedContext, value))
                {
                    OnPropertyChanged(nameof(SelectedContext));
                    OnPropertyChanged(nameof(CurrentButton1Text));
                    OnPropertyChanged(nameof(CurrentButton2Text));
                    OnPropertyChanged(nameof(CurrentButton3Text));
                    OnPropertyChanged(nameof(CurrentButton1Command));
                    OnPropertyChanged(nameof(CurrentButton2Command));
                    OnPropertyChanged(nameof(CurrentButton3Command));
                }
            }
        }

        // Properties that the main screen's UI will bind to,
        // which proxy to the currently selected tab's data/commands.
        public int CurrentContextId => SelectedContext.ContextId;
        public string CurrentContextName => SelectedContext.ContextName;
        public bool CurrentContextCheckedIn => SelectedContext.ContextCheckedIn;
        public List<CheckModel>? CurrentContextChecks => SelectedContext.ContextChecks;
        public string? CurrentContextIcon => SelectedContext.ContextIcon;

        public ICommand CurrentCheckinButtonCommand => SelectedContext.CheckinButtonCommand;
        public ICommand CurrentCheckoutButtonCommand => SelectedContext.CheckoutButtonCommand;
        public ICommand CurrentSummaryButtonClicked => SelectedContext.SummaryButtonClicked;

        public ICommand AddContextCommand { get; }

        public MainPageViewModel()
        {
            SelectedContext = GetSelectedContext();
            ContextItems = GetAllContexts();
        }

        private ContextViewModel GetSelectedContext()
        {

            // 1) use SecureStorage.GetAsync to find the "current" context
            var currentContext = SecureStorage.Default.GetAsync("current");
            if (currentContext != null)
            {
                // 2) use the id from the current context to pull that data from SecureStorage.GetAsync
                var currentFormattedContext = FormatStorageData(currentContext.ToString());
                return new ContextViewModel(currentFormattedContext);
            }
            else
            {
                // 3) if current doesn't exist, there shouldn't be any contexts in storage
                // in this case, we create a new context
                var newContext = InitializeContext();
                var currentContextId = newContext.Id;
                SecureStorage.Default.SetAsync("current", currentContextId.ToString());
                return new ContextViewModel(newContext);
            }
        }

        private ObservableCollection<ContextViewModel> GetAllContexts()
        {
            // use FormatStorageData()
            var cm = new ObservableCollection<ContextViewModel>();
            var dataCount = 1;
            //var storedData = await SecureStorage.Default.GetAsync("1");
            while (dataCount > 0)
            {
                var storedData = SecureStorage.Default.GetAsync(dataCount.ToString());
                if (storedData != null)
                {
                    var formattedData = FormatStorageData(storedData.ToString());
                    var cvm = new ContextViewModel(formattedData);
                    cm.Add(cvm);
                    dataCount++;
                }
                else if (storedData == null && cm.Count == 0)
                {
                    dataCount = 0;
                    cm.Add(InitializeContext());
                }
                else
                {
                    dataCount = 0;
                }
            }
            return cm;
        }

        private ContextModel FormatStorageData(string storedData)
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

        private ContextModel InitializeContext()
        {
            return new ContextModel()
            {
                Id = 1,
                Name = "Default",
                Checks = [],
                Icon = null,
                CheckedIn = false,
            };
        }
    }
}
