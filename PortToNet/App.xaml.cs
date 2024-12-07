using HandyControl.Properties.Langs;
using HandyControl.Themes;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using PortToNet.EventAggregator;
using PortToNet.ViewModels;
using PortToNet.Views;
using Prism.Events;
using Prism.Ioc;
using System.Configuration;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading;
using System.Windows;


namespace PortToNet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Prism.DryIoc.PrismApplication
    {
        public const string AppName = "PortToNet";
        public static string AppCopy = string.Empty;
        private static System.Threading.Mutex mutex;
        public static App CurrentApp
        {
            get { return (App)App.Current; }
        }

        [NotNull]
        private IEventAggregator? ea;
        internal static ResourceDictionary? UIL10N;
        internal static string GetLanguage(string key)
        {
            if (App.UIL10N == null) return key;
            string value = App.UIL10N[key] as string;
            if (value == null) return key;
            return value;
        }

        private int LanguageResourceDictionaryPos = 2;
        protected override Window CreateShell()
        {
            mutex = new System.Threading.Mutex(true, AppName);
            if (mutex.WaitOne(0, false) == false)
            {
                AppCopy = " -2";
            }
            ea = Container.Resolve<IEventAggregator>();

            string msg;
            MyAppSetting.Load(out msg);
       
            ea.GetEvent<RequestChangeLanguageEvent>().Subscribe(ChangeLanguageEventSub);
            UIL10N = this.Resources.MergedDictionaries[LanguageResourceDictionaryPos];
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<MainWindow>();
            containerRegistry.Register<WinTitleContent>();
            containerRegistry.Register<SerialPortPage>();
            containerRegistry.Register<NetPortPage>();
            containerRegistry.Register<FrpcSettingWindow>();
        }

        internal void UpdateSkin(ApplicationTheme theme)
        {
            ThemeAnimationHelper.AnimateTheme(PortToNet.Views.MainWindow.Instance, ThemeAnimationHelper.SlideDirection.Top, 0.3, 1, 0.5);

            ThemeManager.Current.ApplicationTheme = theme;

            /*
            var demoResources = new ResourceDictionary
            {
                Source = ApplicationHelper.GetAbsoluteUri("HandyControlDemo", $"/Resources/Themes/Basic/Colors/{theme.ToString()}.xaml")
            };
            Resources.MergedDictionaries[0].MergedDictionaries.InsertOrReplace(1, demoResources);
            */

            ThemeAnimationHelper.AnimateTheme(PortToNet.Views.MainWindow.Instance, ThemeAnimationHelper.SlideDirection.Bottom, 0.3, 0.5, 1);
        }

        private void ChangeLanguageEventSub(string language)
        {
            string? AsmName = typeof(App).Assembly.GetName().Name;
            if (AsmName != null)
            {
                Uri langUri = new Uri(@"pack://application:,,,/" + AsmName + ";component/Res/Localization/" + language + ".xaml", UriKind.RelativeOrAbsolute);
                ResourceDictionary newDictionary = new ResourceDictionary();
                newDictionary.Source = langUri;
                UIL10N = newDictionary;
                this.Resources.MergedDictionaries[LanguageResourceDictionaryPos] = newDictionary;
                ea.GetEvent<LanguageChangedEvent>().Publish();
            }
        }

        /// <summary>
        /// 有界面主线程执行， 同步执行
        /// </summary>
        /// <param name="action"></param>
        public static void UIDispatcherDoAction(Action action)
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(new Action(() =>
                {
                    action();
                }));
            }
        }

    }
}
