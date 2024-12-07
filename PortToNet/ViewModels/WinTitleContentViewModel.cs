using PortToNet.EventAggregator;
using PortToNet.Views;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PortToNet.ViewModels
{

    public class WinTitleContentViewModel : ViewModelBase
    {
        private string? _CurFileName;
        public string? CurFileName
        {
            get { return _CurFileName; }
            set
            {
                SetProperty(ref _CurFileName, value);
            }
        }
        protected IEventAggregator? _ea;

        public WinTitleContentViewModel(IEventAggregator ea) : base(ea)
        {
            _CurFileName = "";
            _ea = ea;
        }
        public DelegateCommand ExitCommand => new DelegateCommand(ExitCommand_Sub);
        private void ExitCommand_Sub()
        {

        }


        public DelegateCommand OpenGeneralSettingCommand => new DelegateCommand(OpenGeneralSettingCommand_Sub);
        private void OpenGeneralSettingCommand_Sub()
        {
            GeneralSettingWindow win = new GeneralSettingWindow();
            win.DataContext =  MyAppSetting.Default.GeneralSetting;
            win.ShowDialog();
        }


        public DelegateCommand OpenFrpcSettingCommand => new DelegateCommand(OpenFrpcSettingCommand_Sub);
        private void OpenFrpcSettingCommand_Sub()
        {
            FrpcSettingWindow win = new FrpcSettingWindow();
            win.DataContext = new FrpcSettingWindowViewModel();
            win.ShowDialog();
        }

        public DelegateCommand HelpCommand => new DelegateCommand(HelpCommand_Sub);
        private void HelpCommand_Sub()
        {

        }

        private string? CurLanguage;
        public DelegateCommand<string> ChangeLanguageCommand => new DelegateCommand<string>(ChangeLanguageCommand_Sub);
        private void ChangeLanguageCommand_Sub(string lang)
        {
            if (CurLanguage != lang)
            {
                CurLanguage = lang;
                _ea?.GetEvent<RequestChangeLanguageEvent>().Publish(lang);
            }
        }



    }
}
