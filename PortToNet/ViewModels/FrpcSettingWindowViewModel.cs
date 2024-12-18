using Nett;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortToNet.ViewModels
{
    public class FrpcSettingVM : Prism.Mvvm.BindableBase
    {
        /*
         * 
serverAddr = "xxx.xxx.xxx.xxx"
serverPort = xxx
auth.token = "xxx"
[[proxies]]
name = "test"
type = "udp"
localIP = "127.0.0.1"
localPort = 15000
remotePort = 15000
         * 
         */
        public const string CfgPath = "Config/frpc/frpc.toml";
        public List<string> IPAddressList { get; set; }
        public List<string> TypeList { get; set; }
        public static FrpcSettingVM Load(string Path)
        {
            try
            {
                FrpcSettingVM vm = new FrpcSettingVM();
                TomlTable toml = Toml.ReadFile(Path);
                TomlTableArray peopleTable = toml.Get<TomlTableArray>("proxies");
                if (peopleTable.Items.Count > 0)
                {
                    var p0 = peopleTable.Items[0];
                    vm.Name = p0.Get<string>(FirstToLower(nameof(Name)));
                    vm.Type = p0.Get<string>(FirstToLower(nameof(Type)));
                    vm.LocalIP = p0.Get<string>(FirstToLower(nameof(LocalIP)));
                    vm.LocalPort = p0.Get<int>(FirstToLower(nameof(LocalPort)));
                    vm.RemotePort = p0.Get<int>(FirstToLower(nameof(RemotePort)));
                }
                vm.ServerAddr = toml.Get<string>("serverAddr");
                vm.ServerPort = toml.Get<int>("serverPort");
                var tmp = toml.Get<TomlTable>("auth");
                vm.AuthToken = tmp.Get<string>("token");
                return vm;
            }
            catch (Exception ex)
            {

            }
            return new FrpcSettingVM();
        }
        public static void Save(FrpcSettingVM frc)
        {
            string Format = $"serverAddr = \"{frc.ServerAddr}\"\nserverPort = {frc.ServerPort}\nauth.token = \"{frc.AuthToken}\"\n" +
                $"[[proxies]]\nname = \"{frc.Name}\"\ntype = \"{frc.Type}\"\n" +
                $"localIP = \"{frc.LocalIP}\"\nlocalPort = {frc.LocalPort}\r\nremotePort = {frc.RemotePort}\r\n";
            var sw = File.CreateText(CfgPath);
            sw.Write(Format);
            sw.Close();
        }

        public static string FirstToLower(string s)
        {
            if (s.Length >= 1)
                return s.Substring(0, 1).ToLower() + s.Substring(1);
            else return s;
        }

        public FrpcSettingVM()
        {
            ServerAddr = "xxx.xxx.xxx.xxx";
            ServerPort = 0000;
            AuthToken = "xxx";
            Name = $"PC_{Environment.MachineName}";

            TypeList = new List<string>();
            TypeList.Add("tcp");
            TypeList.Add("udp");
            Type = "udp";

            LocalIP = "127.0.0.1";
            LocalPort = 15000;
            RemotePort = 15000;

            IPAddressList = new List<string>();
            IPAddressList.Add("127.0.0.1");
            IPAddressList.AddRange(PubMod.GetLoacalIPMaybeVirtualNetwork());
        }

        private string _ServerAddr;
        public string ServerAddr
        {
            get { return _ServerAddr; }
            set
            {
                SetProperty(ref _ServerAddr, value);
            }
        }

        private int _ServerPort;
        public int ServerPort
        {
            get { return _ServerPort; }
            set
            {
                SetProperty(ref _ServerPort, value);
            }
        }

        private string _AuthToken;
        public string AuthToken
        {
            get { return _AuthToken; }
            set
            {
                SetProperty(ref _AuthToken, value);
            }
        }


        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                SetProperty(ref _Name, value);
            }
        }

        private string _Type;
        public string Type
        {
            get { return _Type; }
            set
            {
                SetProperty(ref _Type, value);
            }
        }


        private string _LocalIP;
        public string LocalIP
        {
            get { return _LocalIP; }
            set
            {
                SetProperty(ref _LocalIP, value);
            }
        }

        private int _LocalPort;
        public int LocalPort
        {
            get { return _LocalPort; }
            set
            {
                int val = Math.Clamp(value, 15000, 15400);
                SetProperty(ref _LocalPort, val);
            }
        }

        private int _RemotePort;
        public int RemotePort
        {
            get { return _RemotePort; }
            set
            {
                int val = Math.Clamp(value, 15000, 15400);
                SetProperty(ref _RemotePort, val);
            }
        }
    }

    public class FrpcSettingWindowViewModel : Prism.Mvvm.BindableBase
    {
        public FrpcSettingWindowViewModel()
        {
            _Frpc = FrpcSettingVM.Load(FrpcSettingVM.CfgPath); 
        }

        private FrpcSettingVM _Frpc;
        public FrpcSettingVM Frpc
        {
            get { return _Frpc; }
            set
            {
                SetProperty(ref _Frpc, value);
            }
        }

        public DelegateCommand SaveFrpcSettingCommand => new DelegateCommand(SaveFrpcSettingCommand_Sub);
        private void SaveFrpcSettingCommand_Sub()
        {
            try
            {
                FrpcSettingVM.Save(_Frpc);
                HandyControl.Controls.Growl.Success($"保存OK:{FrpcSettingVM.CfgPath}");
            }
            catch (Exception)
            {
                HandyControl.Controls.Growl.Warning($"保存异常:{FrpcSettingVM.CfgPath}");
            }
        }
        public DelegateCommand RunFrpcCommand => new DelegateCommand(RunFrpcCommand_Sub);
        private void RunFrpcCommand_Sub()
        {
            string BatPath = "Config/frpc/Runfrpc.bat";
            Process proc = new Process();
            string bat = Path.Combine(Environment.CurrentDirectory, "Config", "frpc", "Runfrpc.bat");
            string env = Path.Combine(Environment.CurrentDirectory, "Config", "frpc");
            string frpc = Path.Combine(Environment.CurrentDirectory, "Config", "frpc", "frpc.exe");
            string toml = Path.Combine(Environment.CurrentDirectory, "Config", "frpc", "frpc.toml");

            proc.StartInfo.FileName = frpc;
            proc.StartInfo.Arguments = $" -c {toml}";
            //proc.StartInfo.Arguments = string.Format("10");//this is argument
            //proc.StartInfo.UseShellExecute = true;//运行时隐藏dos窗口
            //proc.StartInfo.CreateNoWindow = false;//运行时隐藏dos窗口
            //proc.StartInfo.Verb = "runas";//设置该启动动作，会以管理员权限运行进程
            proc.Start();

        }

    }
}

