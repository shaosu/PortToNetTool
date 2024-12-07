using PortToNet.EventAggregator;
using PortToNet.Model;
using PortToNet.Views;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using System.Windows.Input;


namespace PortToNet.ViewModels
{
    internal abstract class PageViewModelBase : ViewModelBase
    {
        protected WorkMode CurWorkMode;

        #region "  属性  "

        [NotNull]
        protected RecvSettingControlViewModel? _RecvSettingVM;
        public RecvSettingControlViewModel? RecvSettingVM
        {
            get { return _RecvSettingVM; }
            set
            {
                SetProperty(ref _RecvSettingVM, value);
            }
        }

        [NotNull]
        protected SendSettingControlViewModel? _SendSettingVM;
        public SendSettingControlViewModel? SendSettingVM
        {
            get { return _SendSettingVM; }
            set
            {
                SetProperty(ref _SendSettingVM, value);
            }
        }

        protected ObservableCollection<SendSlotControlViewModel> _SlotVMs;
        public ObservableCollection<SendSlotControlViewModel> SlotVMs
        {
            get { return _SlotVMs; }
            set
            {
                SetProperty(ref _SlotVMs, value);
            }
        }

        private string? _GLBMessage;
        public string? GLBMessage
        {
            get { return _GLBMessage; }
            set
            {
                SetProperty(ref _GLBMessage, value);
            }
        }

        protected SendRecvStatisticalInformation _TGInfomation;
        public SendRecvStatisticalInformation TGInfomation
        {
            get { return _TGInfomation; }
            set
            {
                SetProperty(ref _TGInfomation, value);
            }
        }

        private string? _SendData;
        public string? SendData
        {
            get { return _SendData; }
            set
            {
                bool ch = SetProperty(ref _SendData, value);
                if (_SendSettingVM.HexSend)
                {
                    if (string.IsNullOrEmpty(_SendData))
                    {
                        SendHexFormatOk = true;
                    }
                    else
                    {
                        if (ch && _SendSettingVM.HexSend)
                        {
                            SendHexFormatOk = PubMod.CheckStringIsHexFormat(_SendData);
                        }
                    }
                }
                else
                {
                    SendHexFormatOk = true;
                }
            }
        }


        private bool _SendHexFormatOk = true;
        public bool SendHexFormatOk
        {
            get { return _SendHexFormatOk; }
            set
            {
                SetProperty(ref _SendHexFormatOk, value);
            }
        }


        #endregion

        public PageViewModelBase(IEventAggregator ea) : base(ea)
        {
            InitSetting();
            ea.GetEvent<LanguageChangedEvent>().Subscribe(LanguageChanged_Sub);

            _GLBMessage = "";
            _TGInfomation = new SendRecvStatisticalInformation();

            ea?.GetEvent<WorkModeChangedEvent>().Subscribe(WorkModeChangedEvent_Sub);
            CurWorkMode = MainWindowViewModel.GLBWorkMode;
           
        }
        protected void InitSetting(PortSettingBase sett)
        {
            RecvSettingVM = sett.RecvSettingVM;
            SendSettingVM = sett.SendSettingVM;
            SlotVMs = sett.SlotVMs;

            foreach (var item in SlotVMs)
            {
                item.SendCommand = SlotSendCommand;
            }
            SendData = sett.SendData;
        }
        protected void Port_ErrorEvent(string obj)
        {
            try
            {
                _RecvSettingVM.AppendErrorToFlowDocument(obj);
            }
            catch (Exception)
            {
            }
        }
        protected virtual void Port_RecvData(byte[] obj)
        {
            string data = _RecvSettingVM.GetFormatString(obj);
            _RecvSettingVM.AppendToFlowDocument(data, obj.Length, true);
            _TGInfomation.RecvBytes += obj.Length;
            _TGInfomation.RecvCount += 1;
        }

        protected abstract void InitSetting();

        protected abstract bool PortSendData(PortSendDataMsg data);

        protected void WorkModeChangedEvent_Sub(WorkMode mode)
        {
            CurWorkMode = mode;
        }

        protected abstract void LanguageChanged_Sub();

        public DelegateCommand SendCommand => new DelegateCommand(SendCommand_Sub);
        private void SendCommand_Sub()
        {
            SendCommand_Sub(_SendData);
        }
        protected abstract void SendCommand_Sub(string sendData);
        public DelegateCommand<SlotSendCommandArg>? SlotSendCommand
            => new DelegateCommand<SlotSendCommandArg>(SlotSendCommand_Sub);

        private void SlotSendCommand_Sub(SlotSendCommandArg arg)
        {
            SendCommand_Sub(arg.Data);
        }

        public DelegateCommand ClearRecvMessageCommand => new DelegateCommand(ClearRecvMessageCommand_Sub);
        private void ClearRecvMessageCommand_Sub()
        {
            _TGInfomation?.Clear();
        }

        public DelegateCommand<MouseButtonEventArgs> LogStringMouseLeftButtonDownCommand => new DelegateCommand<MouseButtonEventArgs>(LogStringMouseLeftButtonDownCommand_Sub);
        private void LogStringMouseLeftButtonDownCommand_Sub(MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2) // 双击
            {
                _RecvSettingVM.FlowDocumentClear();
            }
        }
        public DelegateCommand<MouseButtonEventArgs> EditStringMouseLeftButtonDownCommand => new DelegateCommand<MouseButtonEventArgs>(EditStringMouseLeftButtonDownCommand_Sub);
        private void EditStringMouseLeftButtonDownCommand_Sub(MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2) // 双击
            {
                SendData = string.Empty;
            }
        }

        public DelegateCommand<RichTextBox> RichTextBoxDocLoadedCommand => new DelegateCommand<RichTextBox>(RichTextBoxDocLoadedCommand_Sub);
        private void RichTextBoxDocLoadedCommand_Sub(RichTextBox r)
        {
            _RecvSettingVM.BindRichTextBox(r);
        }

    }
}

