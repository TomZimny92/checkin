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

        public ContextViewModel(ContextModel contextModel)
        {
            _contextItem = contextModel;
            CheckinButtonCommand = new Command(OnCheckinClicked);
            CheckoutButtonCommand = new Command(OnCheckoutClicked);
            SummaryButtonClicked = new Command(OnSummaryClicked);
        }

        public int ContextId => _contextItem.Id;
        public string ContextName => _contextItem.Name;
        public Boolean ContextCheckedIn => _contextItem.CheckedIn;
        public List<CheckModel>? ContextChecks => _contextItem.Checks;
        public string? ContextIcon => _contextItem.Icon;

        public ICommand CheckinButtonCommand { get; }
        public ICommand CheckoutButtonCommand { get; }
        public ICommand SummaryButtonClicked { get; }

        private async void OnCheckinClicked()
        {
            _contextItem.CheckedIn = true;
            var timeStamp = new CheckModel()
            {
                CheckedIn = _contextItem.CheckedIn,
                CheckedTime = DateTime.Now,
            };
            _contextItem.Checks?.Add(timeStamp);
            await SecureStorage.Default.SetAsync(_contextItem.Id.ToString(), JsonSerializer.Serialize(_contextItem));

        }

        private async void OnCheckoutClicked()
        {
            _contextItem.CheckedIn = false;
            var timeStamp = new CheckModel()
            {
                CheckedIn = _contextItem.CheckedIn,
                CheckedTime = DateTime.Now,
            };
            _contextItem.Checks?.Add(timeStamp);
            await SecureStorage.Default.SetAsync(_contextItem.Id.ToString(), JsonSerializer.Serialize(_contextItem));


        }

        private void OnSummaryClicked()
        {

        }
    }
}
