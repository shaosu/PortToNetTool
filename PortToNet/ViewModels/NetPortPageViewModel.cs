using PortToNet.Ecan;
using PortToNet.EventAggregator;
using PortToNet.Model;
using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PortToNet.ViewModels
{
    internal class NetPortPageViewModel : PageViewModelBase
    {
        public NetPortPageViewModel(IEventAggregator ea) : base(ea)
        {
            _NetPortSettingVM = MyAppSetting.Default.NetPort.NetPortSettingVM;
            _NetPortSettingVM.ErrorEvent += Port_ErrorEvent;
            _NetPortSettingVM.RecvDataEvent += Port_RecvData;
            LanguageChanged_Sub();
            _NetPortSettingVM.RecvDataEvent += Port_RecvData_ForDTU;
            ea.GetEvent<PortRecvToNetEvent>().Subscribe(PortRecvToNetEvent_Sub);
            ea.GetEvent<CanPortRecvToNetEvent>().Subscribe(PortRecvToNetEvent_Sub);

            ea.GetEvent<MainWindowClosingEvent>().Subscribe(MainWindowClosingEvent_Sub);

            _NetPortSettingVM.OpenedCB = OpenedCB_Sub;

            RecvSettingVM = MyAppSetting.Default.NetPort.RecvSettingVM;
        }

        protected override void Port_RecvData(byte[] obj)
        {
            PackType type = PackCanFrame.GetPackType(obj);
            switch (type)
            {
                case PackType.Heart:
                    _RecvSettingVM.AppendToFlowDocument("Heart", 3, true);
                    _TGInfomation.RecvBytes += 3;
                    _TGInfomation.RecvCount += 1;
                    break;
                case PackType.CanData:
                    byte[] msg1 = PackCanFrame.UnPackCan_Data(obj);
                    string data1 = "CanData:" + _RecvSettingVM.GetFormatString(msg1);
                    _RecvSettingVM.AppendToFlowDocument(data1, msg1.Length, true);
                    _TGInfomation.RecvBytes += msg1.Length;
                    _TGInfomation.RecvCount += 1;
                    break;
                case PackType.Data:
                    byte[] msg = PackCanFrame.UnPackData(obj);
                    string data = _RecvSettingVM.GetFormatString(msg);
                    _RecvSettingVM.AppendToFlowDocument(data, msg.Length, true);
                    _TGInfomation.RecvBytes += msg.Length;
                    _TGInfomation.RecvCount += 1;
                    break;
                default:
                    break;
            }
        }
        public bool IsCan { get; set; }
        private void OpenedCB_Sub(bool isCan)
        {
            IsCan = isCan;
        }
        private void MainWindowClosingEvent_Sub(CancelEventArgs args)
        {
            MyAppSetting.Default.NetPort.SendData = SendData;
            _NetPortSettingVM.Close();
        }
        protected override void InitSetting()
        {
            InitSetting(MyAppSetting.Default.NetPort);
        }

        private NetPortSettingControlViewModel _NetPortSettingVM;
        public NetPortSettingControlViewModel NetPortSettingVM
        {
            get { return _NetPortSettingVM; }
            set
            {
                SetProperty(ref _NetPortSettingVM, value);
            }
        }
        protected override void LanguageChanged_Sub()
        {
            _NetPortSettingVM.LanguageChanged_Sub();
            RecvSettingVM.LanguageChanged_Sub();
        }

        protected override bool PortSendData(PortSendDataMsg data)
        {
            bool ok = _NetPortSettingVM.SendData(data);
            return ok;
        }

        private void Port_RecvData_ForDTU(byte[] obj)
        {
            PackType type = PackCanFrame.GetPackType(obj);
            if (type == PackType.Heart) return;
            bool iscan = type == PackType.CanData;
            if (iscan)
            {
                List<CAN_OBJ> canmsg = PackCanFrame.UnPackCan(obj);
                PublishCanData(canmsg);
            }
            else
            {
                byte[] msg = PackCanFrame.UnPackData(obj);
                PublishData(msg);
            }

            void PublishData(byte[] msg)
            {
                switch (CurWorkMode)
                {
                    case WorkMode.DTUMode:
                        ea.GetEvent<NetRecvToPortEvent>().Publish(msg);
                        break;
                    case WorkMode.NetToPortMode:
                        ea.GetEvent<NetRecvToPortEvent>().Publish(msg);
                        break;
                    default:
                        break;
                }
            }

            void PublishCanData(List<CAN_OBJ> canmsg)
            {
                switch (CurWorkMode)
                {
                    case WorkMode.DTUMode:
                        ea.GetEvent<NetRecvToCanPortEvent>().Publish(canmsg);
                        break;
                    case WorkMode.NetToPortMode:
                        ea.GetEvent<NetRecvToCanPortEvent>().Publish(canmsg);
                        break;
                    default:
                        break;
                }
            }
        }

        private void PortRecvToNetEvent_Sub(byte[] data)
        {
            PortSendDataMsg msg = new PortSendDataMsg();
            msg.Data = data;
            PortSendData(msg);
        }
        protected override void SendCommand_Sub(string sendData)
        {
            if (string.IsNullOrEmpty(sendData)) return;
            byte[] data = _SendSettingVM.GetByte(sendData);
            byte[] pack = PackCanFrame.PackData(data);

            PortSendDataMsg msg = new PortSendDataMsg();
            msg.Data = pack;
            bool ok = PortSendData(msg);
            if (ok)
            {
                _TGInfomation.SendBytes += data.Length;
                _TGInfomation.SendCount += 1;
                string str = _RecvSettingVM.GetFormatString(data);
                if (string.IsNullOrEmpty(str) == false)
                {
                    // 添加到显示区
                    _RecvSettingVM.AppendToFlowDocument(str, data.Length, false);
                }
            }
        }
    }

}

