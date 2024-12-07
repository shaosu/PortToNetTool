using PortToNet.Model;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Concurrent;
using System.Text.Json.Serialization;

namespace PortToNet.ViewModels
{
    public abstract class PortSettingControlViewModelBase : BindableBase
    {
        public abstract event Action<string> ErrorEvent;

        public abstract event Action<byte[]> RecvDataEvent;
        /// <summary>
        /// 是否为CAN
        /// </summary>
        [JsonIgnore]
        public Action<bool>? OpenedCB;

        protected bool toListion;
        protected bool _IsOpened;
        [JsonIgnore]
        public bool IsOpened
        {
            get { return _IsOpened; }
            set
            {
                SetProperty(ref _IsOpened, value);
            }
        }
        private bool _OpenOrCloseing;
        public bool OpenOrCloseing
        {
            get { return _OpenOrCloseing; }
            set
            {
                SetProperty(ref _OpenOrCloseing, value);
            }
        }
        public DelegateCommand OpenOrCloseCommand => new DelegateCommand(OpenOrCloseCommand_Sub);
        protected async void OpenOrCloseCommand_Sub()
        {
            OpenOrCloseing = true;
            if (_IsOpened)
            {
                IsOpened = await Close();
            }
            else
            {
                IsOpened = await Open();
            }

            if (_IsOpened)
            {
                SendTimer = new System.Threading.Timer(SendTimer_CB, null, 100, Timeout.Infinite);
            }

            UpdateString();
            RaisePropertyChanged(nameof(IsOpened));
            OpenOrCloseing = false;
        }

        protected virtual void UpdateString()
        {

        }

        /// <summary>
        /// 成功:返回true
        /// </summary>
        /// <returns></returns>
        protected abstract Task<bool> Open();
        /// <summary>
        /// 成功:返回false
        /// </summary>
        /// <returns></returns>
        public abstract Task<bool> Close();


        protected ConcurrentQueue<PortSendDataMsg> SendQueue = new ConcurrentQueue<PortSendDataMsg>();

        public bool SendData(PortSendDataMsg data)
        {
            if (_IsOpened == false) return false;
            if (data == null) return false;
            SendQueue.Enqueue(data);
            return true;
        }

        protected abstract bool PortWrite(PortSendDataMsg data);

        private int SendTimerIntever = 10;
        protected System.Threading.Timer SendTimer;
        protected void SendTimer_CB(object? state)
        {
            if (_IsOpened == false) return;

            try
            {
                if (SendQueue.Count > 0)
                {
                    PortSendDataMsg data;
                    if (SendQueue.TryDequeue(out data))
                    {
                        if (data.Data.Length > 0)
                        {
                            PortWrite(data);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            SendTimer.Change(SendTimerIntever, Timeout.Infinite);
        }

    }
}
