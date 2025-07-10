using Checkin.Models;
using Checkin.ViewModel; // Assuming this is your BaseViewModel's namespace
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel; // Make sure this is included for PropertyChangedEventArgs

namespace Checkin.ViewModels
{
    public class MainPageViewModel : BaseViewModel
    {
        private ContextViewModel _selectedContext;

        public ObservableCollection<ContextViewModel> ContextItems { get; }

        public ContextViewModel SelectedContext
        {
            get => _selectedContext;
            set
            {
                // If SetProperty doesn't provide oldValue, capture it BEFORE the call:
                var oldContext = _selectedContext;
                // SetProperty will handle the null check and PropertyChanged for _selectedContext itself
                if (SetProperty(ref _selectedContext, value))
                {
                    // --- IMPORTANT CHANGE HERE ---
                    // Unsubscribe from the old SelectedContext's PropertyChanged event
                    // to prevent memory leaks and ensure we're only listening to the current one.
                    if (oldContext != null) // oldValue is implicitly available from SetProperty if implemented like the Toolkit's
                                          // If your SetProperty doesn't provide oldValue, you'd need to capture it before the SetProperty call.
                    {
                        if (SetProperty(ref _selectedContext, value))
                        {
                            oldContext.PropertyChanged -= OnSelectedContextPropertyChanged;
                        }
                    }

                    // Subscribe to the new SelectedContext's PropertyChanged event
                    if (_selectedContext != null)
                    {
                        _selectedContext.PropertyChanged += OnSelectedContextPropertyChanged;
                    }

                    // When SelectedContext changes, all its proxied properties also change.
                    // We need to notify the UI for all of them.
                    NotifyCurrentContextPropertiesChanged();
                }
            }
        }
        public void Dispose()
        {
            if (_selectedContext != null)
            {
                _selectedContext.PropertyChanged -= OnSelectedContextPropertyChanged;
            }
        }

        // Properties that the main screen's UI will bind to,
        // which proxy to the currently selected tab's data/commands.
        // These are read-only accessors.
        public int CurrentContextId => SelectedContext?.ContextId ?? 0; // Add null-conditional operator
        public string CurrentContextName => SelectedContext?.ContextName ?? "N/A"; // Add null-conditional operator
        public bool CurrentContextCheckedIn => SelectedContext?.ContextCheckedIn ?? false; // Add null-conditional operator
        public List<CheckModel>? CurrentContextChecks => SelectedContext?.ContextChecks; // Add null-conditional operator
        public string? CurrentContextIcon => SelectedContext?.ContextIcon; // Add null-conditional operator
        public string? CurrentSummaryResults => SelectedContext?.SummaryResults; // Add null-conditional operator
        public bool IsCurrentCheckinButtonEnabled => SelectedContext?.IsCheckinButtonEnabled ?? false; // Add null-conditional operator
        public bool IsCurrentCheckoutButtonEnabled => SelectedContext?.IsCheckoutButtonEnabled ?? false; // Add null-conditional operator
        public string CurrentCheckinButtonColor => SelectedContext?.CheckinButtonColor ?? "Gray"; // Add null-conditional operator
        public string CurrentCheckoutButtonColor => SelectedContext?.CheckoutButtonColor ?? "Gray"; // Add null-conditional operator

        public ICommand CurrentCheckinButtonCommand => SelectedContext?.CheckinButtonCommand; // Add null-conditional operator
        public ICommand CurrentCheckoutButtonCommand => SelectedContext?.CheckoutButtonCommand; // Add null-conditional operator
        public ICommand CurrentSummaryButtonCommand => SelectedContext?.SummaryButtonClicked; // Add null-conditional operator
        public ICommand CurrentContextResetCommand => SelectedContext?.ContextResetCommand; // Add null-conditional operator


        public ICommand AddContextCommand { get; } // No implementation shown, but should be here

        public MainPageViewModel()
        {
            // Initialize ContextItems first, as GetSelectedContext might use it indirectly
            ContextItems = GetAllContexts();

            // Then set SelectedContext. This will trigger its setter logic, including subscription.
            // Ensure GetSelectedContext returns a valid ContextViewModel
            SelectedContext = GetSelectedContext();

            // If ContextItems can be empty or GetSelectedContext fails, handle null SelectedContext
            if (SelectedContext == null && ContextItems.Any())
            {
                SelectedContext = ContextItems.First();
            }
            else if (SelectedContext == null)
            {
                // This scenario means no context was loaded and no default was added by GetAllContexts
                // You might want to create a brand new default one here or show an error state.
                var newDefaultContext = new ContextViewModel(InitializeContext());
                ContextItems.Add(newDefaultContext);
                SelectedContext = newDefaultContext;
            }

            // Initialize commands that belong to MainPageViewModel itself
            AddContextCommand = new Command(async () => await AddNewContext());
        }

        // --- NEW METHOD TO HANDLE CHANGES IN SELECTEDCONTEXT ---
        private void OnSelectedContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // This method is called whenever a property in the *current* SelectedContext changes.
            // Based on which property changed in SelectedContext, we notify the UI for the corresponding
            // proxy property in MainPageViewModel.
            switch (e.PropertyName)
            {
                case nameof(ContextViewModel.ContextId):
                    OnPropertyChanged(nameof(CurrentContextId));
                    break;
                case nameof(ContextViewModel.ContextName):
                    OnPropertyChanged(nameof(CurrentContextName));
                    break;
                case nameof(ContextViewModel.ContextCheckedIn):
                    OnPropertyChanged(nameof(CurrentContextCheckedIn));
                    OnPropertyChanged(nameof(IsCurrentCheckinButtonEnabled)); // Related properties
                    OnPropertyChanged(nameof(IsCurrentCheckoutButtonEnabled)); // Related properties
                    OnPropertyChanged(nameof(CurrentCheckinButtonColor));    // Related properties
                    OnPropertyChanged(nameof(CurrentCheckoutButtonColor));   // Related properties
                    break;
                case nameof(ContextViewModel.ContextChecks):
                    OnPropertyChanged(nameof(CurrentContextChecks));
                    break;
                case nameof(ContextViewModel.ContextIcon):
                    OnPropertyChanged(nameof(CurrentContextIcon));
                    break;
                case nameof(ContextViewModel.SummaryResults):
                    OnPropertyChanged(nameof(CurrentSummaryResults));
                    break;
                case nameof(ContextViewModel.IsCheckinButtonEnabled):
                    OnPropertyChanged(nameof(IsCurrentCheckinButtonEnabled));
                    break;
                case nameof(ContextViewModel.IsCheckoutButtonEnabled):
                    OnPropertyChanged(nameof(IsCurrentCheckoutButtonEnabled));
                    break;
                case nameof(ContextViewModel.CheckinButtonColor):
                    OnPropertyChanged(nameof(CurrentCheckinButtonColor));
                    break;
                case nameof(ContextViewModel.CheckoutButtonColor):
                    OnPropertyChanged(nameof(CurrentCheckoutButtonColor));
                    break;
                    // Commands are read-only ICommand, they generally don't change frequently,
                    // but if their CanExecute changes and you want to reflect it immediately
                    // without the command itself raising CanExecuteChanged, you might list them.
                    // However, Commands generally handle their own CanExecuteChanged.
                    // Case nameof(ContextViewModel.CheckinButtonCommand): OnPropertyChanged(nameof(CurrentCheckinButtonCommand)); break;
                    // Case nameof(ContextViewModel.CheckoutButtonCommand): OnPropertyChanged(nameof(CurrentCheckoutButtonCommand)); break;
            }
        }

        // Helper to notify all current context-related properties when SelectedContext itself changes
        private void NotifyCurrentContextPropertiesChanged()
        {
            OnPropertyChanged(nameof(CurrentContextId));
            OnPropertyChanged(nameof(CurrentContextName));
            OnPropertyChanged(nameof(CurrentContextCheckedIn));
            OnPropertyChanged(nameof(CurrentContextChecks));
            OnPropertyChanged(nameof(CurrentContextIcon));
            OnPropertyChanged(nameof(CurrentSummaryResults));
            OnPropertyChanged(nameof(IsCurrentCheckinButtonEnabled));
            OnPropertyChanged(nameof(IsCurrentCheckoutButtonEnabled));
            OnPropertyChanged(nameof(CurrentCheckinButtonColor));
            OnPropertyChanged(nameof(CurrentCheckoutButtonColor));
            OnPropertyChanged(nameof(CurrentCheckinButtonCommand));
            OnPropertyChanged(nameof(CurrentCheckoutButtonCommand));
            OnPropertyChanged(nameof(CurrentSummaryButtonCommand));
            OnPropertyChanged(nameof(CurrentContextResetCommand));
        }

        // Your existing private methods
        private ContextViewModel GetSelectedContext()
        {
            // 1) use SecureStorage.GetAsync to find the "current" context
            var currentContextRaw = SecureStorage.Default.GetAsync("current");
            if (currentContextRaw.Result != null)
            {
                // 2) use the id from the current context to pull that data from SecureStorage.GetAsync
                var currentContext = SecureStorage.Default.GetAsync(currentContextRaw.Result);
                if (currentContext.Result != null)
                {
                    var currentFormattedContext = FormatStorageData(currentContext.Result.ToString());
                    return new ContextViewModel(currentFormattedContext);
                }
            }
            // 3) if current doesn't exist, there shouldn't be any contexts in storage
            // in this case, we create a new context
            var newContext = InitializeContext();
            var currentContextId = newContext.Id;
            SecureStorage.Default.SetAsync("current", currentContextId.ToString());
            return new ContextViewModel(newContext);
        }

        private ObservableCollection<ContextViewModel> GetAllContexts()
        {
            Console.WriteLine($"CurrentSummaryResults: {CurrentSummaryResults}"); // This might be problematic as SelectedContext might not be set yet.
            var cm = new ObservableCollection<ContextViewModel>();
            try
            {
                var dataCount = 1;
                while (true) // Use a better loop condition
                {
                    var storedDataTask = SecureStorage.Default.GetAsync(dataCount.ToString());
                    storedDataTask.Wait(); // Blocking wait - consider using async/await for UI responsiveness
                    var storedData = storedDataTask.Result;

                    if (storedData != null)
                    {
                        var formattedData = FormatStorageData(storedData);
                        var cvm = new ContextViewModel(formattedData);
                        // Subscribe immediately when adding to ContextItems, if you need to react to changes
                        // on *all* contexts in the collection, not just the selected one.
                        // cvm.PropertyChanged += OnIndividualContextItemPropertyChanged; // Example: if you needed to react to changes on any context in the list
                        cm.Add(cvm);
                        dataCount++;
                    }
                    else if (dataCount == 1 && cm.Count == 0) // No contexts found, create a default
                    {
                        var newContext = new ContextViewModel(InitializeContext());
                        cm.Add(newContext);
                        // No need to set "current" here, GetSelectedContext will handle it
                        break; // Exit loop after creating initial context
                    }
                    else
                    {
                        break; // Exit loop if no more data found
                    }
                }
                return cm;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllContexts: {ex.ToString()}");
                // Ensure at least one context exists if an error occurs
                if (!cm.Any())
                {
                    cm.Add(new ContextViewModel(InitializeContext()));
                }
                return cm;
            }
        }


        // Consider making this async if SecureStorage.Default.SetAsync("current", ...) should be awaited
        private async Task AddNewContext()
        {
            var newContextModel = InitializeContext();
            newContextModel.Id = 2; // Assign unique ID
            newContextModel.Name = $"Context {2}"; // Give it a unique name

            // Save the new context to SecureStorage
            await SecureStorage.Default.SetAsync(newContextModel.Id.ToString(), JsonSerializer.Serialize(newContextModel));

            var newContextViewModel = new ContextViewModel(newContextModel);
            ContextItems.Add(newContextViewModel);

            // Automatically select the newly added context
            SelectedContext = newContextViewModel;

            // Update the "current" context in SecureStorage
            await SecureStorage.Default.SetAsync("current", newContextViewModel.ContextId.ToString());
        }

        private ContextModel FormatStorageData(string storedData)
        {
            if (!string.IsNullOrEmpty(storedData))
            {
                ContextModel? cm = JsonSerializer.Deserialize<ContextModel>(storedData);
                if (cm == null)
                {
                    Console.WriteLine("Warning: Deserialized ContextModel is null. Returning new context.");
                    return InitializeContext();
                }
                return cm;
            }
            else
            {
                Console.WriteLine("Warning: Stored data is empty or null. Returning new context.");
                return InitializeContext();
            }
        }

        private ContextModel InitializeContext()
        {
            return new ContextModel()
            {
                Id = 1,
                Name = "Default", // Make default name unique
                Checks = new List<CheckModel>(), // Initialize to empty list, not null
                Icon = null,
                CheckedIn = false,
                Duration = null,
                CheckinButtonEnabled = true,
                CheckoutButtonEnabled = false,
                CheckinButtonColor = "Green",
                CheckoutButtonColor = "Gray",
            };
        }
    }
}


//using Checkin.Models;
//using Checkin.ViewModel;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace Checkin.ViewModels
//{
//    public class MainPageViewModel : BaseViewModel
//    {
//        private ContextViewModel _selectedContext;

//        public ObservableCollection<ContextViewModel> ContextItems { get; }

//        public ContextViewModel SelectedContext
//        {
//            get => _selectedContext;
//            set
//            {
//                if (SetProperty(ref _selectedContext, value))
//                {
//                    OnPropertyChanged(nameof(CurrentContextId));
//                    OnPropertyChanged(nameof(CurrentContextName));
//                    OnPropertyChanged(nameof(CurrentContextCheckedIn));
//                    OnPropertyChanged(nameof(CurrentContextChecks));
//                    OnPropertyChanged(nameof(CurrentContextIcon));
//                    OnPropertyChanged(nameof(CurrentSummaryResults));
//                    OnPropertyChanged(nameof(IsCurrentCheckinButtonEnabled));
//                    OnPropertyChanged(nameof(IsCurrentCheckoutButtonEnabled));
//                    OnPropertyChanged(nameof(CurrentCheckinButtonColor));
//                    OnPropertyChanged(nameof(CurrentCheckoutButtonColor));
//                    OnPropertyChanged(nameof(CurrentCheckinButtonCommand));
//                    OnPropertyChanged(nameof(CurrentCheckoutButtonCommand));
//                    OnPropertyChanged(nameof(CurrentSummaryButtonCommand));
//                    OnPropertyChanged(nameof(CurrentContextResetCommand));
//                }
//            }
//        }

//        // Properties that the main screen's UI will bind to,
//        // which proxy to the currently selected tab's data/commands.
//        public int CurrentContextId => SelectedContext.ContextId;
//        public string CurrentContextName => SelectedContext.ContextName;
//        public bool CurrentContextCheckedIn => SelectedContext.ContextCheckedIn;
//        public List<CheckModel>? CurrentContextChecks => SelectedContext.ContextChecks;
//        public string? CurrentContextIcon => SelectedContext.ContextIcon;
//        public string? CurrentSummaryResults => SelectedContext.SummaryResults;
//        public bool IsCurrentCheckinButtonEnabled => SelectedContext.IsCheckinButtonEnabled;
//        public bool IsCurrentCheckoutButtonEnabled => SelectedContext.IsCheckoutButtonEnabled;
//        public string CurrentCheckinButtonColor => SelectedContext.CheckinButtonColor;
//        public string CurrentCheckoutButtonColor => SelectedContext.CheckoutButtonColor;

//        public ICommand CurrentCheckinButtonCommand => SelectedContext.CheckinButtonCommand;
//        public ICommand CurrentCheckoutButtonCommand => SelectedContext.CheckoutButtonCommand;
//        public ICommand CurrentSummaryButtonCommand => SelectedContext.SummaryButtonClicked;
//        public ICommand CurrentContextResetCommand => SelectedContext.ContextResetCommand;

//        public ICommand AddContextCommand { get; }

//        public MainPageViewModel()
//        {
//            SelectedContext = GetSelectedContext();
//            ContextItems = GetAllContexts();
//        }


//        private ContextViewModel GetSelectedContext()
//        {
//            // 1) use SecureStorage.GetAsync to find the "current" context
//            var currentContextRaw = SecureStorage.Default.GetAsync("current");
//            if (currentContextRaw.Result != null)
//            {
//                // 2) use the id from the current context to pull that data from SecureStorage.GetAsync
//                var currentContext = SecureStorage.Default.GetAsync(currentContextRaw.Result);
//                if (currentContext.Result != null)
//                {
//                    var currentFormattedContext = FormatStorageData(currentContext.Result.ToString());
//                    return new ContextViewModel(currentFormattedContext);
//                }
//            }
//            // 3) if current doesn't exist, there shouldn't be any contexts in storage
//            // in this case, we create a new context
//            var newContext = InitializeContext();
//            var currentContextId = newContext.Id;
//            SecureStorage.Default.SetAsync("current", currentContextId.ToString());
//            return new ContextViewModel(newContext);
//        }

//        private ObservableCollection<ContextViewModel> GetAllContexts()
//        {
//            Console.WriteLine($"CurrentSummaryResults: {CurrentSummaryResults}");
//            var cm = new ObservableCollection<ContextViewModel>();
//            try
//            {
//                // use FormatStorageData()
//                var dataCount = 1;
//                //var storedData = await SecureStorage.Default.GetAsync("1");
//                while (dataCount > 0)
//                {
//                    var storedData = Task.Run(() => SecureStorage.Default.GetAsync(dataCount.ToString()).Result);
//                    if (storedData.Result != null)
//                    {
//                        var formattedData = FormatStorageData(storedData.Result.ToString());
//                        var cvm = new ContextViewModel(formattedData);
//                        cm.Add(cvm);
//                        dataCount++;
//                    }
//                    else if (storedData == null && cm.Count == 0)
//                    {
//                        dataCount = 0;
//                        cm.Add(new ContextViewModel(InitializeContext()));
//                    }
//                    else
//                    {
//                        dataCount = 0;
//                    }
//                }
//                return cm;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//                return cm;
//            }

//        }

//        private ContextModel FormatStorageData(string storedData)
//        {
//            if (!string.IsNullOrEmpty(storedData))
//            {
//                ContextModel cm = JsonSerializer.Deserialize<ContextModel>(storedData);
//                if (cm == null)
//                {
//                    // possible error. TODO log this if it happens
//                    return InitializeContext();
//                }
//                return cm;
//            }
//            else
//            {
//                return InitializeContext();
//            }
//        }

//        private ContextModel InitializeContext()
//        {
//            return new ContextModel()
//            {
//                Id = 1,
//                Name = "Default",
//                Checks = [],
//                Icon = null,
//                CheckedIn = false,
//                Duration = null,
//                CheckinButtonEnabled = true,
//                CheckoutButtonEnabled = false,
//                CheckinButtonColor = "Green",
//                CheckoutButtonColor = "Gray",
//            };
//        }
//    }
//}
