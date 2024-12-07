using PortToNet.EventAggregator;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortToNet.ViewModels
{
    public class GeneralSettingWindowViewModel : Prism.Mvvm.BindableBase
    {
        private IEventAggregator ea;
        public GeneralSettingWindowViewModel()
        {
            _CanPackInterval = 200;
            ea = App.CurrentApp.Container.Resolve<IEventAggregator>();
        }


        private int _CanPackInterval;
        public int CanPackInterval
        {
            get { return _CanPackInterval; }
            set
            {
                int val = Math.Clamp(value, 50, 1000);
                SetProperty(ref _CanPackInterval, val);
            }
        }

        [JsonIgnore]
        public DelegateCommand SaveGeneralSettingCommand => new DelegateCommand(SaveGeneralSettingCommand_Sub);
        private void SaveGeneralSettingCommand_Sub()
        {
            ea.GetEvent<GeneralSettingChangedEvent>().Publish();
            HandyControl.Controls.Growl.Success($"保存OK!");
        }

    }
}
