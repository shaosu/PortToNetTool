using Nett;

        /*
        public const string CfgPath = "Config/frpc/frpc.toml";
            try
                    vm.Type = p0.Get<string>(FirstToLower(nameof(Type)));
                    vm.LocalIP = p0.Get<string>(FirstToLower(nameof(LocalIP)));
                    vm.LocalPort = p0.Get<int>(FirstToLower(nameof(LocalPort)));
                    vm.RemotePort = p0.Get<int>(FirstToLower(nameof(RemotePort)));
        {
            if (s.Length >= 1)
                return s.Substring(0, 1).ToLower() + s.Substring(1);
            else return s;
        }
                HandyControl.Controls.Growl.Success($"保存OK:{FrpcSettingVM.CfgPath}");
            //proc.StartInfo.Arguments = string.Format("10");//this is argument
            //proc.StartInfo.UseShellExecute = true;//运行时隐藏dos窗口
            //proc.StartInfo.CreateNoWindow = false;//运行时隐藏dos窗口
            //proc.StartInfo.Verb = "runas";//设置该启动动作，会以管理员权限运行进程
            proc.Start();