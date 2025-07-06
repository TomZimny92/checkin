using Checkin.ViewModel;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Checkin.Models;
using Checkin.ViewModels;
using System.Security.Cryptography.X509Certificates;

namespace Checkin.ViewModels
{
    public class CheckinViewModel : MainViewModel
    {
    //    //IDispatcherTimer timer;
    //    //public CheckinViewModel(IDispatcherTimer timer)
    //    //{
    //    //    Properties = new ContextPropertiesModel();


    //    //}
    //    private bool _checkedOut;
    //    private bool _checkedIn;
    //    public bool CheckedOut { get => _checkedOut; set => SetProperty(ref _checkedOut, value); }
    //    public bool CheckedIn { get => _checkedIn; set => SetProperty(ref _checkedIn, value); }
    //    public ContextPropertiesModel Properties { get; set; }

    //    public required List<ContextModel> ContextModels { get => GetContextData(); set; }

    //    private List<ContextModel> GetContextData()
    //    {

    //        var cm = new List<ContextModel>();
    //        if (false)
    //        { // if we find data in the local storage
    //            return cm; 
    //        }
    //        else
    //        {
    //            var defaultContext = new ContextModel()
    //            {
    //                Id = 1,
    //                Name = "Default",
    //                Checks = [],
    //            };
    //            cm.Add(defaultContext);
    //            return cm;
    //        }
    //    }
    //    public void Checkin(object? sender, EventArgs e)
    //    {
    //        CheckedIn = true;
    //        var timeStamp = new CheckModel()
    //        {
    //            CheckedIn = CheckedIn,
    //            CheckedTime = DateTime.Now,
    //        };
    //        ContextModels[0].Checks?.Add(timeStamp);
    //        // save timestamp to ContextCheckLog
    //        // when holding down, provide option to select custom Checkin time
    //    }

    //    public void Checkout(object? sender, EventArgs e)
    //    {
    //        CheckedIn = false;
    //        var timeStamp = new CheckModel()
    //        {
    //            CheckedIn = CheckedIn,
    //            CheckedTime = DateTime.Now,
    //        };
    //        ContextModels[0].Checks?.Add(timeStamp);
    //        // save timestamp to ContextCheckLog
    //        // when holding down, provide option to select custom Checkout time
    //    }

    //    public void GetSummary(object? sender, EventArgs e)
    //    {
    //        var dateTime1 = ContextModels[0]?.Checks[1]?.CheckedTime;
    //        var dateTime2 = ContextModels[0]?.Checks[0]?.CheckedTime;
    //        var result = dateTime1 - dateTime2;
    //        TestLabel.Text = result.ToString();
    //        // if still checked in, show the current live time
    //        // Nice to Have: also update the live preferences (money, rate, etc)
    //        // otherwise, show calculated time and the calculated preference
    //    }

    //    public bool IsCheckedIn(object? sender, EventArgs e)
    //    {
    //        return CheckedIn;
    //    }


    }
}
