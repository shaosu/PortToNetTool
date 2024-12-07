using PortToNet.Ecan;
using PortToNet.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PortToNet.EventAggregator.CanPortRecvToNetEvent;

namespace PortToNet.EventAggregator
{
    public class RequestChangeLanguageEvent : PubSubEvent<string>
    {
        public static string? CurLanguage { get; set; }
    }

    public class LanguageChangedEvent : PubSubEvent
    {
    }
    public class GeneralSettingChangedEvent : PubSubEvent
    {
    }

    public class WorkModeChangedEvent : PubSubEvent<WorkMode>
    {
    }
    public class PortRecvToNetEvent : PubSubEvent<byte[]>
    {
    }
    public class CanPortRecvToNetEvent : PubSubEvent<byte[]>
    {
       
    }
    public class NetRecvToPortEvent : PubSubEvent<byte[]>
    {
    }
    public class NetRecvToCanPortEvent : PubSubEvent<List<CAN_OBJ>>
    {
    }
    public class MainWindowClosingEvent : PubSubEvent<CancelEventArgs>
    {
    }

}
