using HandyControl.Tools.Extension;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Security.Policy;
using System.Windows;

namespace PortToNet.Model
{

    public abstract class EnumDescription<T> where T : struct, Enum
    {
        [NotNull]
        public T? Mode { get; set; }

        [NotNull]
        public string? Display { get; set; }
        public override string ToString()
        {
            return Display;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            if (obj is EnumDescription<T> eobj)
            {
                return Mode.Equals(eobj.Mode);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Mode.GetHashCode();
        }

        public int CompareTo(T? other)
        {
            return Mode.Value.CompareTo(other);
        }

    }

    public class WorkModeDescription : EnumDescription<WorkMode>
    {
        public static List<WorkModeDescription> BuildWorkMode(ResourceDictionary? UIL10N)
        {
            List<WorkModeDescription> rt = new List<WorkModeDescription>();
            if (UIL10N == null)
            {
                rt.Add(new WorkModeDescription { Mode = WorkMode.SingleWorkingMode, Display = "SingleWorkingMode" });
                rt.Add(new WorkModeDescription { Mode = WorkMode.DTUMode, Display = "DTUMode" });
            }
            else
            {
                string a0 = (string)UIL10N["SingleWorkingMode"];
                string a1 = (string)UIL10N["DTUMode"];
                string a2 = (string)UIL10N["PortToNetMode"];
                string a3 = (string)UIL10N["NetToPortMode"];
                rt.Add(new WorkModeDescription { Mode = WorkMode.SingleWorkingMode, Display = a0 });
                rt.Add(new WorkModeDescription { Mode = WorkMode.DTUMode, Display = a1 });
                rt.Add(new WorkModeDescription { Mode = WorkMode.PortToNetMode, Display = a2 });
                rt.Add(new WorkModeDescription { Mode = WorkMode.NetToPortMode, Display = a3 });
            }
            return rt;
        }

        public static WorkModeDescription? OnLanguageChanged(WorkModeDescription? Old, ObservableCollection<WorkModeDescription> Coll, ResourceDictionary? UIL10N)
        {
            var tmp = Old?.Mode;
            Old = null;
            Coll.Clear();
            Coll.AddRange(BuildWorkMode(UIL10N));
            var cur = Coll.Where(a => a.Mode == tmp).FirstOrDefault();
            return cur;
        }
    }

    public class ParityDescription : EnumDescription<System.IO.Ports.Parity>
    {
        public static List<ParityDescription> BuildWorkMode(ResourceDictionary? UIL10N)
        {
            List<ParityDescription> rt = new List<ParityDescription>();
            if (UIL10N == null)
            {
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.None, Display = "None" });
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Odd, Display = "Odd" });
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Even, Display = "Even" }); // 偶校验
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Mark, Display = "Mark" });
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Space, Display = "Space" });
            }
            else
            {
                string a0 = (string)UIL10N["Parity.None"] ?? "无";
                string a1 = (string)UIL10N["Parity.Odd"] ?? "奇校验";
                string a2 = (string)UIL10N["Parity.Even"] ?? "偶校验";
                string a3 = (string)UIL10N["Parity.Mark"] ?? "Mark";
                string a4 = (string)UIL10N["Parity.Space"] ?? "空格";

                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.None, Display = a0 });
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Odd, Display = a1 });
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Even, Display = a2 }); // 偶校验
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Mark, Display = a3 });
                rt.Add(new ParityDescription { Mode = System.IO.Ports.Parity.Space, Display = a4 });
            }
            return rt;
        }
        public static ParityDescription? OnLanguageChanged(ParityDescription? Old, ObservableCollection<ParityDescription> Coll, ResourceDictionary? UIL10N)
        {
            var tmp = Old?.Mode;
            Old = null;
            Coll.Clear();
            Coll.AddRange(BuildWorkMode(UIL10N));
            var cur = Coll.Where(a => a.Mode == tmp).FirstOrDefault();
            return cur;
        }
    }

    public class StopBitsDescription : EnumDescription<System.IO.Ports.StopBits>
    {
        public static List<StopBitsDescription> BuildWorkMode(ResourceDictionary? UIL10N)
        {
            List<StopBitsDescription> rt = new List<StopBitsDescription>();
            rt.Add(new StopBitsDescription { Mode = System.IO.Ports.StopBits.None, Display = "0" });
            rt.Add(new StopBitsDescription { Mode = System.IO.Ports.StopBits.One, Display = "1" });
            rt.Add(new StopBitsDescription { Mode = System.IO.Ports.StopBits.Two, Display = "2" });
            rt.Add(new StopBitsDescription { Mode = System.IO.Ports.StopBits.OnePointFive, Display = "1.5" });
            return rt;
        }
    }

    public class NetProtocolTypeDescription : EnumDescription<NetProtocolType>
    {
        public static List<NetProtocolTypeDescription> BuildWorkMode(ResourceDictionary? UIL10N)
        {
            List<NetProtocolTypeDescription> rt = new List<NetProtocolTypeDescription>();
            rt.Add(new NetProtocolTypeDescription { Mode = NetProtocolType.TCP_Client, Display = "TCP Client" });
            rt.Add(new NetProtocolTypeDescription { Mode = NetProtocolType.TCP_Server, Display = "TCP Server" });
            rt.Add(new NetProtocolTypeDescription { Mode = NetProtocolType.UDP, Display = "UDP" });
            rt.Add(new NetProtocolTypeDescription { Mode = NetProtocolType.UDP_Server, Display = "UDP_Server" });
            return rt;
        }
    }
    
    public class ShowTimeFormatDescription:EnumDescription<ShowTimeFormat>
    {
        System.DateTime ComperOldTime = System.DateTime.Now;
        System.DateTime CompareRelativeTime = System.DateTime.Now;
        public string GetTimeString(ShowTimeFormat format, string append = "")
        {
            string resualt;
            TimeSpan timesp = new TimeSpan();
            if (format != ShowTimeFormat.CpRelativeSecondAndMs)
                CompareRelativeTime = System.DateTime.Now;

            if (format == ShowTimeFormat.None) return string.Empty;
            switch (format)
            {
                case ShowTimeFormat.Default:
                    resualt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + append;
                    break;
                case ShowTimeFormat.Second:
                    resualt = System.DateTime.Now.ToString("HH:mm:ss") + append;
                    break;
                case ShowTimeFormat.SecondAndMs:
                    resualt = System.DateTime.Now.ToString("ss.fff") + append;
                    break;
                case ShowTimeFormat.Compare:
                    timesp = System.DateTime.Now - ComperOldTime;
                    ComperOldTime = System.DateTime.Now;
                    resualt = $"{timesp.Hours}:{timesp.Minutes}:{timesp.Seconds}.{timesp.Milliseconds,-3}" + append;
                    break;
                case ShowTimeFormat.CpRelativeSecondAndMs:
                    timesp = System.DateTime.Now - CompareRelativeTime;
                    resualt = $"{(long)timesp.TotalSeconds}.{timesp.Milliseconds,-3}" + append;
                    break;
                case ShowTimeFormat.CpSecondAndMs:
                    timesp = System.DateTime.Now - ComperOldTime;
                    ComperOldTime = System.DateTime.Now;
                    resualt = $"{timesp.Seconds}.{timesp.Milliseconds}" + append;
                    break;
                case ShowTimeFormat.CompareSecond:
                    timesp = System.DateTime.Now - ComperOldTime;
                    ComperOldTime = System.DateTime.Now;
                    resualt = $"{timesp.Seconds}" + append;
                    break;
                case ShowTimeFormat.UnixSecond:
                    System.DateTime startTime1 = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                    var sec = (long)(System.DateTime.Now - startTime1).TotalSeconds;
                    resualt = $"{sec}" + append;
                    break;
                case ShowTimeFormat.Tick:
                    resualt = $"{System.DateTime.Now.Ticks}" + append;
                    break;
                case ShowTimeFormat.UnixSecondAndMs:
                    // System.DateTime startTime2 = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                    long secms2 = (System.DateTime.Now.Ticks - 621355968000000000) / 10000;  //相对毫秒 
                    var sec2 = (long)(secms2 / 1000);
                    long misec = (long)(secms2 - sec2 * 1000);
                    resualt = $"{sec2}.{misec,-3}" + append;
                    break;
                default:
                    resualt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + append;
                    break;
            }
            return resualt;
        }


        public static List<ShowTimeFormatDescription> BuildWorkMode(ResourceDictionary? UIL10N)
        {
            List<ShowTimeFormatDescription> rt = new List<ShowTimeFormatDescription>();
            if (UIL10N == null)
            {
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.None, Display = "None" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Default, Display = "Default" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Second, Display = "Second" });  
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.SecondAndMs, Display = "SecondAndMs" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Compare, Display = "Compare" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.CompareSecond, Display = "CompareSecond" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.CpSecondAndMs, Display = "CpSecAndMs" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.CpRelativeSecondAndMs, Display = "CpRelative" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.UnixSecond, Display = "UnixSecond" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Tick, Display = "Tick" });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.UnixSecondAndMs, Display = "UnixSecAndMs" });
            }
            else
            {
                string a0 = (string)UIL10N["ShowTimeFormat.None"] ?? "不显示";
                string a1 = (string)UIL10N["ShowTimeFormat.Default"] ?? "默认";
                string a2 = (string)UIL10N["ShowTimeFormat.Second"] ?? "秒";
                string a3 = (string)UIL10N["ShowTimeFormat.SecondAndMs"] ?? "秒和毫秒";
                string a4 = (string)UIL10N["ShowTimeFormat.Compare"] ?? "比较";
                string a7 = (string)UIL10N["ShowTimeFormat.CompareSecond"] ?? "比较秒";
                string a5 = (string)UIL10N["ShowTimeFormat.CpSecondAndMs"] ?? "比较秒和毫秒";
                string a6 = (string)UIL10N["ShowTimeFormat.CpRelativeSecondAndMs"] ?? "比较相对";
                string a8 = (string)UIL10N["ShowTimeFormat.UnixSecond"] ?? "Uxix秒";
                string a9 = (string)UIL10N["ShowTimeFormat.Tick"] ?? "Tick";
                string a10 = (string)UIL10N["ShowTimeFormat.UnixSecondAndMs"] ?? "Unix秒和毫秒";

                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.None, Display = a0 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Default, Display = a1 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Second, Display = a2 });  
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.SecondAndMs, Display = a3 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Compare, Display = a4 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.CompareSecond, Display = a5 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.CpSecondAndMs, Display = a6 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.CpRelativeSecondAndMs, Display = a7 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.UnixSecond, Display = a8 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.Tick, Display = a9 });
                rt.Add(new ShowTimeFormatDescription { Mode = ShowTimeFormat.UnixSecondAndMs, Display = a10 });
            }
            return rt;
        }
        public static ShowTimeFormatDescription? OnLanguageChanged(ShowTimeFormatDescription? Old, ObservableCollection<ShowTimeFormatDescription> Coll, ResourceDictionary? UIL10N)
        {
            var tmp = Old?.Mode;
            Old = null;
            Coll.Clear();
            Coll.AddRange(BuildWorkMode(UIL10N));
            var cur = Coll.Where(a => a.Mode == tmp).FirstOrDefault();
            return cur;
        }
    }

}
