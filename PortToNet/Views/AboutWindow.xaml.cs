using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace PortToNet.Views
{
    public partial class AboutWindow : HandyControl.Controls.Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            DataContext = this;

            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            Version = GetVersionString(true);
            CopyRight = versionInfo.LegalCopyright;
            CompanyName = versionInfo.CompanyName;
            ProductName = versionInfo.ProductName;
            RuntimeVersion = $"Runtime:{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
        }

        public static string GetVersionString(bool showDebug = false)
        {
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            var str = string.Empty;
            string[] v = versionInfo.FileVersion.Split(new char[] { '.' });
            if (v.Length >= 3)
                str = $"V{v[0]}.{v[1]}.{v[2]}";
            else if (v.Length >= 2)
                str = $"V{v[0]}.{v[1]}.0";
            else if (v.Length >= 1)
            {
                str = $"V{v[0]}.0.0";
            }
            else
            {
                str = $"V1.0.0";
            }
            if (showDebug == true && RunningModeIsDebug)
            {
                return str + "-Debug";
            }
            else
                return str;
        }
        public static bool RunningModeIsDebug
        {
            get
            {
                var assebly = Assembly.GetEntryAssembly();
                if (assebly == null)
                {
                    assebly = new StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
                }

                var debugableAttribute = assebly.GetCustomAttribute<DebuggableAttribute>();
                var isdebug = debugableAttribute.DebuggingFlags.HasFlag(DebuggableAttribute.DebuggingModes.EnableEditAndContinue);
                return isdebug;
            }
        }
        public static string VersionString
        {
            get
            {
                return GetVersionString();
            }
        }
        public static readonly DependencyProperty CopyRightProperty = DependencyProperty.Register(
            "CopyRight", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));

        public string CopyRight
        {
            get => (string)GetValue(CopyRightProperty);
            set => SetValue(CopyRightProperty, value);
        }

        public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
            "Version", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));

        public string Version
        {
            get => (string)GetValue(VersionProperty);
            set => SetValue(VersionProperty, value);
        }

        public static readonly DependencyProperty ProductNameProperty = DependencyProperty.Register(
         "ProductName", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));

        public string ProductName
        {
            get => (string)GetValue(ProductNameProperty);
            set => SetValue(ProductNameProperty, value);
        }

        public static readonly DependencyProperty CompanyNameProperty = DependencyProperty.Register(
          "CompanyName", typeof(string), typeof(AboutWindow), new PropertyMetadata(default(string)));

        public string CompanyName
        {
            get => (string)GetValue(CompanyNameProperty);
            set => SetValue(CompanyNameProperty, value);
        }

        public string RuntimeVersion
        {
            get { return (string)GetValue(RuntimeVersionProperty); }
            set { SetValue(RuntimeVersionProperty, value); }
        }

        public static readonly DependencyProperty RuntimeVersionProperty =
            DependencyProperty.Register(nameof(RuntimeVersion), typeof(string), typeof(AboutWindow), new PropertyMetadata(string.Empty));
    }
}
