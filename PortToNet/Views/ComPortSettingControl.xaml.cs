using PortToNet.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PortToNet.Views
{
    /// <summary>
    /// ComPortSettingControl.xaml 的交互逻辑
    /// </summary>
    public partial class ComPortSettingControl 
    {
        public ComPortSettingControl()
        {
            InitializeComponent();
            this.Loaded += ComPortSettingControl_Loaded;
        }

        private void ComPortSettingControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var VM = (ComPortSettingControlViewModel)this.DataContext;
                VM.TryUpPortList_OnLoad();
            }
            catch (Exception)
            {
            }
        }

        private void ComPort_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var VM = (ComPortSettingControlViewModel)this.DataContext;
            VM.ComPortList.Clear();
            VM.ComPortList.AddRange( System.IO.Ports.SerialPort.GetPortNames());
        }
    }
}
