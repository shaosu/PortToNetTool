using PortToNet.Model;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Text.Json.Serialization;

namespace PortToNet.ViewModels
{
    public class ComPortSettingControlViewModel : PortSettingControlViewModelBase
    {
        public override event Action<string> ErrorEvent;

        public override event Action<byte[]> RecvDataEvent;

        private SerialPort? Com;

        #region "  串口相关设置  "

        private ObservableCollection<string> _ComPortList;
        [JsonIgnore]
        public ObservableCollection<string> ComPortList
        {
            get { return _ComPortList; }
            set
            {
                SetProperty(ref _ComPortList, value);
            }
        }

        private ObservableCollection<int> _BaudList;
        [JsonIgnore]
        public ObservableCollection<int> BaudList
        {
            get { return _BaudList; }
            set
            {
                SetProperty(ref _BaudList, value);
            }
        }

        private ObservableCollection<int> _DataBitList;
        [JsonIgnore]
        public ObservableCollection<int> DataBitList
        {
            get { return _DataBitList; }
            set
            {
                SetProperty(ref _DataBitList, value);
            }
        }

        private ObservableCollection<ParityDescription> _ParityList;
        [JsonIgnore]
        public ObservableCollection<ParityDescription> ParityList
        {
            get { return _ParityList; }
            set
            {
                SetProperty(ref _ParityList, value);
            }
        }

        private ObservableCollection<StopBitsDescription> _StopBitsList;
        [JsonIgnore]
        public ObservableCollection<StopBitsDescription> StopBitsList
        {
            get { return _StopBitsList; }
            set
            {
                SetProperty(ref _StopBitsList, value);
            }
        }

        private string? _ComPortName;
        public string? ComPortName
        {
            get { return _ComPortName; }
            set
            {
                SetProperty(ref _ComPortName, value);
            }
        }

        private int _Baud;
        public int Baud
        {
            get { return _Baud; }
            set
            {
                SetProperty(ref _Baud, value);
            }
        }

        private int _DataBit;
        public int DataBit
        {
            get { return _DataBit; }
            set
            {
                SetProperty(ref _DataBit, value);
            }
        }

        private ParityDescription? _Parity;
        public ParityDescription? Parity
        {
            get { return _Parity; }
            set
            {
                SetProperty(ref _Parity, value);
            }
        }

        private StopBitsDescription? _StopBits;
        public StopBitsDescription? StopBits
        {
            get { return _StopBits; }
            set
            {
                SetProperty(ref _StopBits, value);
            }
        }

        #endregion

        public ComPortSettingControlViewModel() : base()
        {
            _ComPortList = new ObservableCollection<string>();
            _ComPortList.AddRange(SerialPort.GetPortNames());
            _BaudList = new ObservableCollection<int>();
            _BaudList.Add(1200);
            _BaudList.Add(2400);
            _BaudList.Add(4800);
            _BaudList.Add(9600);
            _BaudList.Add(14400);
            _BaudList.Add(19200);
            _BaudList.Add(38400);
            _BaudList.Add(56000);
            _BaudList.Add(57600);
            _BaudList.Add(115200);
            _DataBitList = new ObservableCollection<int>();
            _DataBitList.Add(8);
            _DataBitList.Add(7);
            _ParityList = new ObservableCollection<ParityDescription>();
            _ParityList.AddRange(ParityDescription.BuildWorkMode(App.UIL10N));
            _StopBitsList = new ObservableCollection<StopBitsDescription>();
            _StopBitsList.AddRange(StopBitsDescription.BuildWorkMode(App.UIL10N));

            _DataBit = 8;
            _Parity = _ParityList.Where(a => a.Mode == System.IO.Ports.Parity.None).FirstOrDefault();
            _StopBits = _StopBitsList.Where(a => a.Mode == System.IO.Ports.StopBits.One).FirstOrDefault();
        }

        public void TryUpPortList_OnLoad()
        {
            try
            {
                var curs = System.IO.Ports.SerialPort.GetPortNames();
                List<string> dels = new List<string>();

                foreach (var item in ComPortList)
                {
                    if (curs.Contains(item) == false) // 不在当前列表中 =>删除
                    {
                        dels.Add(item);
                    }
                }
                foreach (var cur in curs)
                {
                    if (ComPortList.Contains(cur) == false) // 当前列表中不存在=>添加
                    {
                        ComPortList.Add(cur);
                    }
                }
                foreach (var cur in dels)
                {
                    ComPortList.Remove(cur);
                }

                if (_ComPortList.Contains(_ComPortName) == false)
                {
                    ComPortName = string.Empty;
                }
            }
            catch (Exception)
            {
            }
        }
        public void LanguageChanged_Sub()
        {
            Parity = ParityDescription.OnLanguageChanged(Parity, ParityList, App.UIL10N);
        }
        protected override Task<bool> Open()
        {
            OpenedCB?.Invoke(false);
            ErrorEvent?.Invoke(App.GetLanguage("Connecting..."));
            if (string.IsNullOrWhiteSpace(_ComPortName))
            {
                ErrorEvent?.Invoke(App.GetLanguage("SerialPortNameException"));
                return Task.FromResult(false);
            }

            bool ok = false;
            try
            {
                if (Com != null)
                {
                    Com.Close();
                }
                Com = new SerialPort();
                Com.PortName = _ComPortName;
                Com.BaudRate = _Baud;
                Com.DataBits = _DataBit;
                Com.Parity = _Parity.Mode.Value;
                Com.StopBits = _StopBits.Mode.Value;

                SendQueue.Clear();
                Com.Open();
                Com.DataReceived -= Com_DataReceived;
                Com.DataReceived += Com_DataReceived;
                ok = true;
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

        public override Task<bool> Close()
        {
            bool oping = false;

            if (SendQueue.Count > 0)
            {
                ErrorEvent?.Invoke($"{App.GetLanguage("ClearUnsentData")}{SendQueue.Count}");
                SendQueue.Clear();
            }
            try
            {
                if (Com != null)
                {
                    Com.Close();
                }
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex.Message);
            }
            RaisePropertyChanged(nameof(IsOpened));
            return Task.FromResult(oping);
        }
        private void Com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int c = Com.BytesToRead;
            if (c > 0)
            {
                byte[] data = new byte[c];
                try
                {
                    int rc = Com.Read(data, 0, c);
                    if (rc > 0)
                    {
                        Array.Resize(ref data, rc);
                        RecvDataEvent.Invoke(data);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        protected override bool PortWrite(PortSendDataMsg msg)
        {
            if (msg == null) return false;
            if (msg.IsCan) return false;
            var data = msg.Data;
            try
            {
                Com.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke($"{App.GetLanguage("SendFailed:")}{PubMod.ArrayToHexString(data)}{Environment.NewLine}{ex.Message}");
            }
            return true;
        }
    }

}
