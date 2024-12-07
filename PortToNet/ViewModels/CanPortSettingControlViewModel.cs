using PortToNet.Ecan;
using PortToNet.EventAggregator;
using PortToNet.Model;
using Prism.Events;
using Prism.Ioc;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace PortToNet.ViewModels
{
    public class CanPortSettingControlViewModel : PortSettingControlViewModelBase
    {
        public override event Action<string> ErrorEvent;
        public override event Action<byte[]> RecvDataEvent;
        public event Action<List<CAN_OBJ>> CanRecvDataEvent;

        private ComProc MCan = null;
        private IEventAggregator ea;

        #region "  CAN 相关设置 "

        private CanTypeInfos _CanDevType;
        public CanTypeInfos CanDevType
        {
            get { return _CanDevType; }
            set
            {
                SetProperty(ref _CanDevType, value);
                MCan.CanTypeInfo = _CanDevType;
            }
        }

        private int _CANNumber;
        public int CANNumber
        {
            get { return _CANNumber; }
            set
            {
                SetProperty(ref _CANNumber, value);
                MCan.DevIndex = (uint)_CANNumber;
            }
        }

        private int _CANChannel;
        public int CANChannel
        {
            get { return _CANChannel; }
            set
            {
                SetProperty(ref _CANChannel, value);
                MCan.CanIndex = (uint)_CANChannel - 1;
            }
        }

        private CanBaudrate _CanBaud;
        public CanBaudrate CanBaud
        {
            get { return _CanBaud; }
            set
            {
                SetProperty(ref _CanBaud, value);
                MCan.Can1_BaudrateIndex = _CanBaud.Index;
            }
        }

        private ObservableCollection<CanTypeInfos> _CanDevTypeList;
        [JsonIgnore]
        public ObservableCollection<CanTypeInfos> CanDevTypeList
        {
            get { return _CanDevTypeList; }
            set
            {
                SetProperty(ref _CanDevTypeList, value);
            }
        }


        private ObservableCollection<int> _CANNumberList;
        [JsonIgnore]
        public ObservableCollection<int> CANNumberList
        {
            get { return _CANNumberList; }
            set
            {
                SetProperty(ref _CANNumberList, value);
            }
        }

        private ObservableCollection<int> _CANChannelList;
        [JsonIgnore]
        public ObservableCollection<int> CANChannelList
        {
            get { return _CANChannelList; }
            set
            {
                SetProperty(ref _CANChannelList, value);
            }
        }


        private ObservableCollection<CanBaudrate> _CanBaudList;
        [JsonIgnore]
        public ObservableCollection<CanBaudrate> CanBaudList
        {
            get { return _CanBaudList; }
            set
            {
                SetProperty(ref _CanBaudList, value);
            }
        }

        #endregion
        public CanPortSettingControlViewModel()
        {
            MCan = new ComProc();
            MCan.CanRecvDataEvent += MCan_CanRecvDataEvent;
            _CanDevTypeList = new ObservableCollection<CanTypeInfos>();
            _CanDevTypeList.AddRange(CanTypeInfos.CanList);
            _CANNumberList = new ObservableCollection<int>();
            _CANNumberList.AddRange(Enumerable.Range(0, 10));
            _CANChannelList = new ObservableCollection<int>();
            _CANChannelList.AddRange(Enumerable.Range(1, 2));
            _CanBaudList = new ObservableCollection<CanBaudrate>();
            _CanBaudList.AddRange(CanBaudrate.CanBaudrateList);

            CanDevType = _CanDevTypeList[0];
            CANNumber = _CANNumberList[0];
            CANChannel = _CANChannelList[0];
            CanBaud = _CanBaudList[5];

            ea = App.CurrentApp.Container.Resolve<IEventAggregator>();
            ea.GetEvent<GeneralSettingChangedEvent>().Subscribe(GeneralSettingChangedEvent_Sub);
        }

        private void GeneralSettingChangedEvent_Sub()
        {
            MCan.RecTimer_Sw = MyAppSetting.Default.GeneralSetting.CanPackInterval;
        }

        private void MCan_CanRecvDataEvent(List<CAN_OBJ> obj)
        {
            CanRecvDataEvent?.Invoke(obj);
        }

        public override Task<bool> Close()
        {
            {
                bool oping = false;

                if (SendQueue.Count > 0)
                {
                    ErrorEvent?.Invoke($"{App.GetLanguage("ClearUnsentData")}{SendQueue.Count}");
                    SendQueue.Clear();
                }
                try
                {
                    if (MCan != null)
                    {
                        MCan.CloseDev();
                    }
                }
                catch (Exception ex)
                {
                    ErrorEvent?.Invoke(ex.Message);
                }
                RaisePropertyChanged(nameof(IsOpened));
                return Task.FromResult(oping);
            }
        }
        protected override Task<bool> Open()
        {
            OpenedCB?.Invoke(true);
            ErrorEvent?.Invoke(App.GetLanguage("Connecting..."));
            if (_CanDevType == null)
            {
                ErrorEvent?.Invoke(App.GetLanguage("PleaseSelectCanDevice"));
                return Task.FromResult(false);
            }

            bool ok = false;
            try
            {
                if (MCan != null)
                {
                    MCan.CloseDev();
                }
                SendQueue.Clear();
                MCan.OpenDev();
                ok = MCan.IsOpened;

            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex.Message);
                ok = false;
            }
            if (ok)
                ErrorEvent?.Invoke(App.GetLanguage("ConnectionSuccessful"));
            else
                ErrorEvent?.Invoke(App.GetLanguage("ConnectionException"));
            return Task.FromResult(ok);
        }


        protected override bool PortWrite(PortSendDataMsg data)
        {
            if (data == null) return false;
            try
            {
                if (data.IsCan == false) return false;
                return MCan.Can_Send(data.BuildCanData());
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke($"{App.GetLanguage("SendFailed:")} CanID:0x{data.CanID:X} {PubMod.ArrayToHexString(data.Data)}{Environment.NewLine}{ex.Message}");
            }
            return true;
        }
    }
}


