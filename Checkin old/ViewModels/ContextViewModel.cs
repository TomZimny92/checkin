using Checkin.Models;
using Checkin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Checkin.ViewModels
{
    public class ContextViewModel : BaseViewModel
    {
        private ContextModel _contextItem;
        public ContextModel ContextItem
        {
            get => _contextItem;
            set
            {
                if (_contextItem != value)
                {
                    _contextItem = value;
                    OnPropertyChanged();
                }
            }
                //SetProperty(ref _contextItem, value);
        }
        private bool _isButtonEnabled = true;

        public ContextViewModel(ContextModel contextModel)
        {
            _contextItem = contextModel;
            CheckinButtonCommand = new Command(execute: () => { OnCheckinClicked(); });
            CheckoutButtonCommand = new Command(execute: () => { OnCheckoutClicked(); });
            SummaryButtonClicked = new Command(execute: () => { OnSummaryClicked(); });
            ContextResetCommand = new Command(execute: () => { OnResetClicked(); });
        }

        public int ContextId => _contextItem.Id;
        public string ContextName => _contextItem.Name;
        public Boolean ContextCheckedIn => _contextItem.CheckedIn;
        public List<CheckModel>? ContextChecks => _contextItem.Checks;
        public string? ContextIcon => _contextItem.Icon;
        public string? SummaryResults => _contextItem.Duration;
        public bool IsCheckinButtonEnabled => _contextItem.CheckinButtonEnabled;
        public bool IsCheckoutButtonEnabled => _contextItem.CheckoutButtonEnabled;
        public string CheckinButtonColor => _contextItem.CheckinButtonColor;
        public string CheckoutButtonColor => _contextItem.CheckoutButtonColor;

        public ICommand CheckinButtonCommand { get; private set; }
        public ICommand CheckoutButtonCommand { get; private set; }
        public ICommand SummaryButtonClicked { get; private set; }
        public ICommand ContextResetCommand { get; private set; }

        public ICommand ToggleButtonAbility { get; private set; }

        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set
            {
                if (_isButtonEnabled != value)
                {
                    _isButtonEnabled = value;
                    OnPropertyChanged();
                }
            }
        }


        private void OnCheckinClicked()
        {
            _contextItem.CheckedIn = true;
            var timeStamp = new CheckModel()
            {
                CheckedIn = _contextItem.CheckedIn,
                CheckedTime = DateTime.Now,
            };
            _contextItem.Checks?.Add(timeStamp);
            _contextItem.CheckinButtonEnabled = false;
            _contextItem.CheckoutButtonEnabled = true;
            _contextItem.CheckinButtonColor = "Gray";
            _contextItem.CheckoutButtonColor = "Red";
            CalculateDuration();
            SecureStorage.Default.SetAsync(_contextItem.Id.ToString(), JsonSerializer.Serialize(_contextItem));
            //DisableButton("Checkin");

        }

        private void OnCheckoutClicked()
        {
            _contextItem.CheckedIn = false;
            var timeStamp = new CheckModel()
            {
                CheckedIn = _contextItem.CheckedIn,
                CheckedTime = DateTime.Now,
            };
            _contextItem.Checks?.Add(timeStamp);
            _contextItem.CheckinButtonEnabled = true;
            _contextItem.CheckoutButtonEnabled = false;
            _contextItem.CheckinButtonColor = "Green";
            _contextItem.CheckoutButtonColor = "Gray";
            CalculateDuration();
            SecureStorage.Default.SetAsync(_contextItem.Id.ToString(), JsonSerializer.Serialize(_contextItem));
            //DisableButton("Checkout");

        }

        private void OnSummaryClicked()
        {
            // check local storage to see if this value exists

            // if exists, see if it's up to date (tbd on how to do that) (hash?)
            // save ID and Summary to the "current" key
            // if not, do the math

            // save the value to local storage
            CalculateDuration();
        }

        private void CalculateDuration()
        {
            // check to see if context is currently checked in
            // if so, do the math up to the latest checkin, then add DateTime.Now
            // have the results display live
            if (_contextItem.CheckedIn)
            {

            }
            else // if context is checked out, do the regular math
            {
                TimeSpan totalTime = new(0, 0, 0);
                var checks = _contextItem.Checks;
                if (checks.Count > 1 && checks != null)
                {
                    for (var i = 0; i < checks?.Count - 1; i += 2)
                    {
                        totalTime = totalTime.Add(checks[i + 1].CheckedTime - checks[i].CheckedTime);
                    }
                }
                _contextItem.Duration = totalTime.ToString();
            }
        }

        private void DisableButton(string buttonName)
        {
            if (_contextItem.CheckedIn && buttonName == "Checkin")
            {
                IsButtonEnabled = false;

                return;
            }
            if (_contextItem.CheckedIn && buttonName == "Checkout")
            {
                IsButtonEnabled = true;
                return;
            }
            if (!_contextItem.CheckedIn && buttonName == "Checkin")
            {
                IsButtonEnabled = true;
                return;
            }
            if (!_contextItem.CheckedIn && buttonName == "Checkout")
            {
                IsButtonEnabled = false;
                return;
            }
            else
            {
                IsButtonEnabled = true;
            }
        }

        private void OnResetClicked()
        {
            _contextItem.Checks = [];
            _contextItem.Duration = "";
            _contextItem.CheckedIn = false;
            _contextItem.CheckinButtonEnabled = true;
            _contextItem.CheckoutButtonEnabled = false;
            _contextItem.CheckinButtonColor = "Green";
            _contextItem.CheckoutButtonColor = "Gray";
            // dont' remove the whole thing. just the checks and duration
            // instead, update the Storage with an updated object for that id
            //SecureStorage.Default.Remove(_contextItem.Id.ToString());

            SecureStorage.Default.SetAsync(_contextItem.Id.ToString(), JsonSerializer.Serialize(_contextItem));

            // need to reset button settings
        }
    }
}