using PortToNet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PortToNet.Views
{
    /// <summary>
    /// FrpcSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FrpcSettingWindow : Window
    {
        FrpcSettingWindowViewModel VM;
        public FrpcSettingWindow()
        {
            InitializeComponent();
            this.Loaded += FrpcSettingWindow_Loaded;

        }

        private void ActualPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            VM.Frpc.AuthToken = pwd_AuthToken.Password;
        }

        bool first = true;
        private void FrpcSettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (first)
            {
                first = false;
                VM = (FrpcSettingWindowViewModel)this.DataContext;
                pwd_AuthToken.ActualPasswordBox.PasswordChanged += ActualPasswordBox_PasswordChanged;
            }

            pwd_AuthToken.UnsafePassword = VM.Frpc.AuthToken;
            pwd_AuthToken.Password = VM.Frpc.AuthToken;
        }
    }
}

