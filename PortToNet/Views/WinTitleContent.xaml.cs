using HandyControl.Themes;
using PortToNet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PortToNet.Views
{
    public partial class WinTitleContent
    {
        public WinTitleContent()
        {
            InitializeComponent();
            //this.DataContext = new ViewModels.WinTitleContentViewModel();
        }
        private void MenuAbout_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            new AboutWindow
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
        }

        private void ButtonConfig_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            PopupConfig.IsOpen = true;
        }

        private void ButtonSkins_OnClick(object sender, System.Windows.RoutedEventArgs e)
        {
            PopupConfig.IsOpen = false;
            try
            {
                if (e.OriginalSource is Button button && button.Tag is ApplicationTheme tag)
                {
                    ((App)Application.Current).UpdateSkin(tag);
                    MyAppSetting.Default.UISet.Theme = tag;
                }
                if (e.OriginalSource is Button button2 && button2.Tag is string lang)
                {
                    MyAppSetting.Default.UISet.CurLanguage = lang;
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
