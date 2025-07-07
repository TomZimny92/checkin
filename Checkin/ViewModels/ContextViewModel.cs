using Checkin.Models;
using Checkin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkin.ViewModels
{
    public class ContextViewModel : MainViewModel
    {
        private ContextModel _contextItem;

        public ContextViewModel(ContextModel contextModel)
        {
            _contextItem = contextModel;
            AddContextButtonCommand = new Command(OnAddContextClicked);
        }

        public string ContextName => _contextItem.Name;
        
        public ICommand AddContextButtonCommand { get; }
    }
}
