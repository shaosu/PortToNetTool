using HandyControl.Controls;
using PortToNet.Ecan;
using PortToNet.EventAggregator;
using PortToNet.Model;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace PortToNet.ViewModels
{

    internal class SerialPortPageViewModel : PageViewModelBase
    {
        public SerialPortPageViewModel(IEventAggregator ea) : base(ea)
        {
            _ComPortSettingVM = MyAppSetting.Default.SerialPort.ComPortSettingVM;
            _ComPortSettingVM.RecvDataEvent += Port_RecvData;
            _ComPortSettingVM.ErrorEvent += Port_ErrorEvent;
            _ComPortSettingVM.RecvDataEvent += Port_RecvData_ForDTU;
            _ComPortSettingVM.OpenedCB = OpenedCB_Sub;
            ea.GetEvent<NetRecvToPortEvent>().Subscribe(NetRecvToPortEvent_Sub);
            ea.GetEvent<NetRecvToCanPortEvent>().Subscribe(NetRecvToCanPortEvent_Sub);
            ea.GetEvent<MainWindowClosingEvent>().Subscribe(MainWindowClosingEvent_Sub);


            _CanPortSettingVM = MyAppSetting.Default.SerialPort.CanPortSettingVM;
            _CanPortSettingVM.CanRecvDataEvent += CanPort_RecvDataEvent;
            _CanPortSettingVM.ErrorEvent += Port_ErrorEvent;
            _CanPortSettingVM.CanRecvDataEvent += CanPort_RecvData_ForDTU;
            _CanPortSettingVM.OpenedCB = OpenedCB_Sub;

            _CanSendSettingVM = MyAppSetting.Default.SerialPort.CanSendSettingVM;
            LanguageChanged_Sub();

            RecvSettingVM = MyAppSetting.Default.SerialPort.RecvSettingVM;
        }

        [JsonIgnore]
        public bool IsCan { get; set; }
        private void OpenedCB_Sub(bool isCan)
        {
            IsCan = isCan;
        }


        private void CanPort_RecvDataEvent(List<CAN_OBJ> list)
        {
            foreach (var obj in list)
            {
                string data = _RecvSettingVM.GetFormatString(obj);
                _RecvSettingVM.AppendToFlowDocument(data, obj.DataLen, true);
                _TGInfomation.RecvBytes += obj.DataLen;
                _TGInfomation.RecvCount += 1;
            }
        }
        private void MainWindowClosingEvent_Sub(CancelEventArgs args)
        {
            MyAppSetting.Default.SerialPort.SendData = SendData;
            _ComPortSettingVM.Close();
            _CanPortSettingVM.Close();
        }

        protected override void InitSetting()
        {
            InitSetting(MyAppSetting.Default.SerialPort);
        }

        [NotNull]
        private ComPortSettingControlViewModel? _ComPortSettingVM;
        public ComPortSettingControlViewModel? ComPortSettingVM
        {
            get { return _ComPortSettingVM; }
            set
            {
                SetProperty(ref _ComPortSettingVM, value);
            }
        }


        private CanPortSettingControlViewModel _CanPortSettingVM;
        public CanPortSettingControlViewModel CanPortSettingVM
        {
            get { return _CanPortSettingVM; }
            set
            {
                SetProperty(ref _CanPortSettingVM, value);
            }
        }


        private CanSendSettingControlViewModel _CanSendSettingVM;
        public CanSendSettingControlViewModel CanSendSettingVM
        {
            get { return _CanSendSettingVM; }
            set
            {
                SetProperty(ref _CanSendSettingVM, value);
            }
        }

        protected override void LanguageChanged_Sub()
        {
            _ComPortSettingVM.LanguageChanged_Sub();
            RecvSettingVM.LanguageChanged_Sub();
        }

        protected override bool PortSendData(PortSendDataMsg data)
        {
            if (data.IsCan == false)
            {
                if (data.Data == null || data.Data.Length == 0) return false;

                bool ok = _ComPortSettingVM.SendData(data);
                return ok;
            }
            else
            {
                bool ok = _CanPortSettingVM.SendData(data);
                return ok;
            }
        }

        protected override void SendCommand_Sub(string sendData)
        {
            if (string.IsNullOrEmpty(sendData)) return;

            byte[] data = _SendSettingVM.GetByte(sendData);

            PortSendDataMsg msg = new PortSendDataMsg();
            msg.Data = data;
            msg.IsCan = IsCan;
            msg.ExternFlag = _CanSendSettingVM.ExternFlag;
            msg.RemoteFlag = _CanSendSettingVM.RemoteFlag;
            msg.CanID = _CanSendSettingVM.CanID;

            bool ok = PortSendData(msg);
            if (ok)
            {
                _TGInfomation.SendBytes += data.Length;
                _TGInfomation.SendCount += 1;
                if (msg.IsCan)
                {
                    CAN_OBJ can = msg.BuildCanData();
                    string str = _RecvSettingVM.GetFormatString(can);
                    if (string.IsNullOrEmpty(str) == false)
                    {
                        // 添加到显示区
                        _RecvSettingVM.AppendToFlowDocument(str, data.Length, false);
                    }
                }
                else
                {
                    string str = _RecvSettingVM.GetFormatString(data);
                    if (string.IsNullOrEmpty(str) == false)
                    {
                        // 添加到显示区
                        _RecvSettingVM.AppendToFlowDocument(str, data.Length, false);
                    }
                }

            }
        }
        private void NetRecvToPortEvent_Sub(byte[] data)
        {
            PortSendDataMsg msg = new PortSendDataMsg();
            msg.IsCan = false;
            msg.Data = data;
            PortSendData(msg);
        }
        private void NetRecvToCanPortEvent_Sub(List<CAN_OBJ> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                PortSendDataMsg msg = PortSendDataMsg.BuildMsg(list[i]);
                PortSendData(msg);
            }
        }

        private void Port_RecvData_ForDTU(byte[] data)
        {
            var pack = PackCanFrame.PackData(data);
            switch (CurWorkMode)
            {
                case WorkMode.SingleWorkingMode:
                    break;
                case WorkMode.DTUMode:
                    ea.GetEvent<PortRecvToNetEvent>().Publish(pack);
                    break;
                case WorkMode.PortToNetMode:
                    ea.GetEvent<PortRecvToNetEvent>().Publish(pack);
                    break;
                case WorkMode.NetToPortMode:
                    break;
                default:
                    break;
            }
        }

        private void CanPort_RecvData_ForDTU(List<CAN_OBJ> obj)
        {
            var pack = PackCanFrame.PackCan(obj);

            switch (CurWorkMode)
            {
                case WorkMode.SingleWorkingMode:
                    break;
                case WorkMode.DTUMode:
                    ea.GetEvent<CanPortRecvToNetEvent>().Publish(pack);
                    break;
                case WorkMode.PortToNetMode:
                    ea.GetEvent<CanPortRecvToNetEvent>().Publish(pack);
                    break;
                case WorkMode.NetToPortMode:
                    break;
                default:
                    break;
            }
        }

    }
}

