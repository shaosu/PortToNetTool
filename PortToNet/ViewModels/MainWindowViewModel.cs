using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HandyControl.Properties.Langs;
using Microsoft.Win32;
using PortToNet.EventAggregator;
using PortToNet.Model;
using PortToNet.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace PortToNet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                SetProperty(ref _Title, value);
            }
        }

        public static WorkMode GLBWorkMode;
        [NotNull]
        private WorkModeDescription? _WorkMode;
        public WorkModeDescription? WorkMode
        {
            get { return _WorkMode; }
            set
            {
                bool ch = SetProperty(ref _WorkMode, value);
                if (ch && _WorkMode != null)
                {
                    ea?.GetEvent<WorkModeChangedEvent>().Publish(_WorkMode.Mode.Value);
                }
                if (_WorkMode != null)
                    GLBWorkMode = _WorkMode.Mode.Value;
            }
        }

        private ObservableCollection<WorkModeDescription> _WorkModelList;
        public ObservableCollection<WorkModeDescription> WorkModelList
        {
            get { return _WorkModelList; }
            set
            {
                SetProperty(ref _WorkModelList, value);
            }
        }

        public MainWindowViewModel(IEventAggregator ea) : base(ea)
        {
            _Title = $"{App.AppName} {AboutWindow.GetVersionString()} {App.AppCopy}";
            _WorkModelList = new ObservableCollection<WorkModeDescription>();
            _WorkModelList.AddRange(WorkModeDescription.BuildWorkMode(App.UIL10N).ToArray());
            WorkMode = _WorkModelList.Where(x => x.Mode == MyAppSetting.Default.MainSet.WorkMode.Mode).FirstOrDefault();
            ea.GetEvent<LanguageChangedEvent>().Subscribe(LanguageChanged_Sub);
        }

        private void LanguageChanged_Sub()
        {
            WorkMode = WorkModeDescription.OnLanguageChanged(WorkMode, WorkModelList, App.UIL10N);
        }


        public DelegateCommand MainWinLoadCommand => new DelegateCommand(MainWinLoadCommand_Sub);
        private void MainWinLoadCommand_Sub()
        {
            try
            {
                if (string.IsNullOrEmpty(MyAppSetting.Default.UISet.CurLanguage) == false)
                    ea?.GetEvent<RequestChangeLanguageEvent>().Publish(MyAppSetting.Default.UISet.CurLanguage);

                ((App)Application.Current).UpdateSkin(MyAppSetting.Default.UISet.Theme);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Warning(ex.Message);
            }
        }


        public DelegateCommand<CancelEventArgs> MainWindowClosingCommand => new DelegateCommand<CancelEventArgs>(MainWindowClosingCommand_Sub);
        private void MainWindowClosingCommand_Sub(CancelEventArgs e)
        {
            MyAppSetting.Default.MainSet.WorkMode = WorkMode;
            ea.GetEvent<MainWindowClosingEvent>().Publish(e);
            MyAppSetting.Save();

        }
        public void RaiseWorkModeUpdate()
        {
            ea?.GetEvent<WorkModeChangedEvent>().Publish(_WorkMode.Mode.Value);
        }
    }
}
