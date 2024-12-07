using Prism.Events;
using Prism.Mvvm;
using System.Text;

namespace PortToNet.ViewModels
{
    public class SendRecvStatisticalInformation : BindableBase
    {
        private long _RecvBytes;
        public long RecvBytes
        {
            get { return _RecvBytes; }
            set
            {
                SetProperty(ref _RecvBytes, value);
            }
        }

        private long _SendBytes;
        public long SendBytes
        {
            get { return _SendBytes; }
            set
            {
                SetProperty(ref _SendBytes, value);
            }
        }

        private long _RecvCount;
        public long RecvCount
        {
            get { return _RecvCount; }
            set
            {
                SetProperty(ref _RecvCount, value);
            }
        }

        private long _SendCount;
        public long SendCount
        {
            get { return _SendCount; }
            set
            {
                SetProperty(ref _SendCount, value);
            }
        }

        public SendRecvStatisticalInformation()
        {
        }
        public override string ToString()
        {
            StringBuilder sb= new StringBuilder();
            sb.Append(App.GetLanguage("SendCount:"));
            sb.Append(_SendCount);
            sb.Append(" ");
            sb.Append(App.GetLanguage("RecvCount:"));
            sb.Append(_SendCount);
            sb.Append(" ");
            sb.Append(App.GetLanguage("SendBytes:"));
            sb.Append(_SendCount);
            sb.Append(" ");
            sb.Append(App.GetLanguage("RecvBytes:"));
            sb.Append(_SendCount);
            sb.Append(" ");
            return sb.ToString();
        }
        
        public void Clear()
        {
            RecvCount = SendCount = 0;
            RecvBytes = SendBytes = 0;
        }
    
    }

    public abstract class ViewModelBase : BindableBase
    {
        protected IEventAggregator? ea;
        public ViewModelBase(IEventAggregator tea)
        {
            ea = tea;
        }
    }
}
