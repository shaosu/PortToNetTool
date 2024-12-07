using PortToNet.EventAggregator;
using PortToNet.Model;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json.Serialization;

namespace PortToNet.ViewModels
{
    public class PortSettingBase
    {
        public RecvSettingControlViewModel? RecvSettingVM { get; set; }

        public SendSettingControlViewModel? SendSettingVM { get; set; }

        public ObservableCollection<SendSlotControlViewModel> SlotVMs { get; set; }

        public string? SendData { get; set; }

        public PortSettingBase()
        {
            RecvSettingVM = new RecvSettingControlViewModel();
            SendSettingVM = new SendSettingControlViewModel();
            SlotVMs = new ObservableCollection<SendSlotControlViewModel>();
            for (int i = 1; i <= 16; i++)
            {
                SlotVMs.Add(new SendSlotControlViewModel() { SlotIndex = i, Data = "AA BB CC" });
            }
        }
    }


    public class NetPortSetting : PortSettingBase
    {
        public NetPortSettingControlViewModel NetPortSettingVM { get; set; }

        public NetPortSetting() : base()
        {
            NetPortSettingVM = new NetPortSettingControlViewModel();
        }
    }

    public class SerialPortSetting : PortSettingBase
    {
        public ComPortSettingControlViewModel ComPortSettingVM { get; set; }
        public CanPortSettingControlViewModel CanPortSettingVM { get; set; }
        public CanSendSettingControlViewModel CanSendSettingVM { get; set; }
        
        public SerialPortSetting() : base()
        {
            ComPortSettingVM = new ComPortSettingControlViewModel();
            CanPortSettingVM = new CanPortSettingControlViewModel();
            CanSendSettingVM = new CanSendSettingControlViewModel();
        }
    }


    public class MainSetting : BindableBase
    {
        [NotNull]
        private WorkModeDescription? _WorkMode;
        public WorkModeDescription? WorkMode
        {
            get { return _WorkMode; }
            set
            {
                SetProperty(ref _WorkMode, value);
            }
        }
        public MainSetting()
        {
            _WorkMode = new WorkModeDescription() { Mode = Model.WorkMode.SingleWorkingMode };
        }
    }

    public class UISetting : BindableBase
    {
        private HandyControl.Themes.ApplicationTheme _Theme;
        public HandyControl.Themes.ApplicationTheme Theme
        {
            get { return _Theme; }
            set
            {
                SetProperty(ref _Theme, value);
            }
        }

        private string _CurLanguage;
        /// <summary>
        /// zh-CN
        /// en-US
        /// </summary>
        public string CurLanguage
        {
            get { return _CurLanguage; }
            set
            {
                SetProperty(ref _CurLanguage, value);
            }
        }
        public UISetting()
        {
            _Theme = HandyControl.Themes.ApplicationTheme.Light;
            _CurLanguage = "zh-CN";
        }
    }

    public class MyAppSetting
    {
        public NetPortSetting NetPort { get; set; }
        public SerialPortSetting SerialPort { get; set; }
        public MainSetting MainSet { get; set; }
        public UISetting UISet { get; set; }

        public GeneralSettingWindowViewModel GeneralSetting { get; set; }

        [JsonIgnore]
        public const string FilePath = "AppSetting.json";
        private static MyAppSetting instance;
        private static readonly object lockObject = new object();

        [JsonIgnore]
        [NotNull]
        public static MyAppSetting? Default
        {
            get
            {
                // 延迟初始化，在需要时才创建实例
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new MyAppSetting();
                        }
                    }
                }
                return instance;
            }
        }

        public MyAppSetting()
        {
            NetPort = new NetPortSetting();
            SerialPort = new SerialPortSetting();
            MainSet = new MainSetting();
            UISet = new UISetting();
            GeneralSetting = new GeneralSettingWindowViewModel();
        }
        private void Check()
        {
            if (NetPort == null)
                NetPort = new NetPortSetting();
            if (SerialPort == null)
                SerialPort = new SerialPortSetting();
            if (MainSet == null)
                MainSet = new MainSetting();
            if (UISet == null)
                UISet = new UISetting();
            if (UISet == null)
                GeneralSetting = new GeneralSettingWindowViewModel();
            
        }

        public static MyAppSetting Load(out string Warn)
        {
            Warn = string.Empty;
            string Path = MyAppSetting.FilePath;
            if (File.Exists(Path) == false)
            {
                return MyAppSetting.Default;
            }

            try
            {
                string js = System.IO.File.ReadAllText(Path);
                instance = System.Text.Json.JsonSerializer.Deserialize<MyAppSetting>(js);
                if (instance == null) instance = new MyAppSetting();
                instance.Check();
                return instance;
            }
            catch (Exception ex)
            {
                // Title += "配置文件加载错误:" + ex.Message;
                Warn = "配置文件加载错误";
                return new MyAppSetting();
            }
        }

        private static int FileHashCode;
        public static void Save()
        {
            string js = System.Text.Json.JsonSerializer.Serialize(instance);
            if (FileHashCode != js.GetHashCode())
            {
                System.IO.File.WriteAllText(FilePath, js);
            }
        }

    }
}
