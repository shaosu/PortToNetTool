using PortToNet.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ZLGCAN;
using static PortToNet.Ecan.EnumObj;

namespace PortToNet.Ecan
{
    public class CanBaudrate
    {
        /// <summary>
        /// 索引
        /// </summary>
        public int Index { set; get; }
        /// <summary>
        /// 波特率值
        /// </summary>
        public int Rate { get; set; }
        /// <summary>
        /// 显示文本
        /// </summary>
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public static readonly List<CanBaudrate> CanBaudrateList = new List<CanBaudrate>
            {
            new CanBaudrate {Index = 0, Rate = 1000, Text = "1M Bit/S"},
            new CanBaudrate {Index = 1, Rate = 800, Text = "800 KBit/S"},
            new CanBaudrate {Index = 2, Rate = 666, Text = "666 KBit/S"},
            new CanBaudrate {Index = 3, Rate = 500, Text = "500 KBit/S"},
            new CanBaudrate {Index = 4, Rate = 400, Text = "400 KBit/S"},
            new CanBaudrate {Index = 5, Rate = 250, Text = "250 KBit/S"},
            new CanBaudrate {Index = 6, Rate = 200, Text = "200 KBit/S"},
            new CanBaudrate {Index = 7, Rate = 125, Text = "125 KBit/S"},
            new CanBaudrate {Index = 8, Rate = 100, Text = "100 KBit/S"},
            new CanBaudrate {Index = 9, Rate = 80, Text = "80 KBit/S"},
            new CanBaudrate {Index = 10, Rate = 50, Text = "50 KBit/S"},
            };

    }

    public class CanTypeInfos
    {
        public EnumObj.CanType_Enum CanType { get; set; }
        public uint DevType { get; set; }
        public string CanName { get; set; }

        public override string ToString()
        {
            return CanName;
        }

        public static List<CanTypeInfos> CanList = new List<CanTypeInfos>()
        {
            new CanTypeInfos(){ CanName="USBCAN-II C V502",DevType=1, CanType=CanType_Enum.PACE_CAN },
            new CanTypeInfos(){ CanName="TYPE_CAN",DevType=0, CanType=CanType_Enum.ZLG_CAN },
            new CanTypeInfos(){ CanName="ZCAN_USBCAN1",DevType=3, CanType=CanType_Enum.ZLG_CAN },
            new CanTypeInfos(){ CanName="ZCAN_USBCAN2",DevType=4, CanType=CanType_Enum.ZLG_CAN },
            new CanTypeInfos(){ CanName="ZCAN_USBCAN_E_U",DevType=20, CanType=CanType_Enum.ZLG_CAN },
            new CanTypeInfos(){ CanName="ZCAN_USBCAN_2E_U",DevType=21, CanType=CanType_Enum.ZLG_CAN },
        };
    }
    public class EnumObj
    {
        public enum Communication
        {
            /// <summary>
            /// 无
            /// </summary>
            None,
            /// <summary>
            /// 通讯正常
            /// </summary>
            Normal,
            /// <summary>
            /// 通讯超时
            /// </summary>
            TimeOut
        }
        public enum CanType_Enum
        {
            PACE_CAN,
            ZLG_CAN
        }
    }
    public class ComProc
    {
        public const uint AutoCodeCanID = 0x7F1;
        public const string RecvDirectionStr = "↓";
        public const string SendDirectionStr = "↑";
        public event Action<List<CAN_OBJ>> CanRecvDataEvent;

        public int Can1_BaudrateIndex;
        public object CanLock = new object();

        public EnumObj.Communication CommStatus_Mc = EnumObj.Communication.None;

        public bool IsOpened;

        public ManualResetEvent FrmClosedWaitHandler = new ManualResetEvent(true);

        public bool IsDisposed = false;
        public ComProc()
        {
            IsOpened = false;
        }

        private int CloseNum;

        private int Can_Send_TrySend(CAN_OBJ mMsg, int maxTryCount = 3)
        {
            int failCount = 0;
            do
            {
                switch (CanTypeInfo.CanType)
                {
                    case EnumObj.CanType_Enum.PACE_CAN:
                        {
                            uint mLen = 1;
                            var result = ECANDLL.Transmit(CanTypeInfo.DevType, DevIndex, CanIndex, ref mMsg, (ushort)mLen);
                            if (result == ECANStatus.StatusOk)
                            {
                                failCount = 0;
                            }
                            else
                            {
                                failCount++;
                            }
                        }
                        break;
                    case EnumObj.CanType_Enum.ZLG_CAN:
                        {
                            ZCAN_Transmit_Data can_data = new ZCAN_Transmit_Data();

                            can_data.frame.can_id = Method.MakeCanId(mMsg.ID, mMsg.ExternFlag, mMsg.RemoteFlag, 0);
                            can_data.frame.data = mMsg.data;
                            can_data.frame.can_dlc = mMsg.DataLen;
                            can_data.transmit_type = 0;
                            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
                            Marshal.StructureToPtr(can_data, ptr, true);
                            var result = Method.ZCAN_Transmit(channel_handle_zlg, ptr, 1);
                            if (result == 1)
                            {
                                failCount = 0;
                            }
                            else
                            {
                                failCount++;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            while (IsDisposed == false && failCount > 0 && failCount < maxTryCount && IsOpened);
            return failCount;
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sendList"></param>
        /// <param name="isSetting">设置命令</param>
        /// <param name="isBroadcast">广播</param>
        /// <returns></returns>
        public bool Can_Send(List<CAN_OBJ> sendList, bool isSetting = false, bool isBroadcast = false)
        {
            bool allSuccess = false;
            foreach (var canObj in sendList)
            {
                if (IsOpened == false)
                {
                    allSuccess = false;
                    break;
                }
                var mMsg = canObj;
                int failCount = 0;
                int maxTryCount = 3;

                failCount = Can_Send_TrySend(mMsg, maxTryCount);
                if (failCount > maxTryCount)
                {
                    allSuccess = false;
                    if (IsOpened && IsDisposed == false)
                    {
                        try
                        {
                            lock (CanLock)
                            {
                                CloseNum++;
                                if (CloseNum > 20000)
                                {
                                    CloseNum = 0;
                                }
                                if (CloseNum < 100 && CloseNum % 15 == 1)
                                {
                                    ECANDLL.CloseDevice(1, 0);
                                    Thread.Sleep(100);
                                    ECANDLL.OpenDevice(1, 0, 0); //重建连接
                                    Thread.Sleep(100);
                                    //var initConfig = GetInitConfig();
                                    //ECANDLL.InitCAN(1, 0, 0, ref initConfig);
                                    Thread.Sleep(100);
                                    ECANDLL.StartCAN(1, 0, 0);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    allSuccess = true;
                }
            }



            if (allSuccess)
            {
                CloseNum = 0;
                if (isBroadcast)
                {
                    Thread.Sleep(200);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Can_Send(CAN_OBJ mMsg, bool isBroadcast = false)
        {
            bool allSuccess = false;
            if (IsOpened == false)
            {
                allSuccess = false;
            }
            int failCount = 0;
            int maxTryCount = 3;
            failCount = Can_Send_TrySend(mMsg, maxTryCount);
            if (failCount >= maxTryCount)
            {
                allSuccess = false;
                if (IsOpened && IsDisposed == false)
                {
                    try
                    {
                        lock (CanLock)
                        {
                            CloseNum++;
                            if (CloseNum > 20000)
                            {
                                CloseNum = 0;
                            }
                            if (CloseNum < 100 && CloseNum % 15 == 1)
                            {
                                ECANDLL.CloseDevice(1, 0);
                                Thread.Sleep(100);
                                ECANDLL.OpenDevice(1, 0, 0); //重建连接
                                Thread.Sleep(100);
                                //var initConfig = GetInitConfig();
                                //ECANDLL.InitCAN(1, 0, 0, ref initConfig);
                                Thread.Sleep(100);
                                ECANDLL.StartCAN(1, 0, 0);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                allSuccess = true;
            }

            if (allSuccess)
            {
                CloseNum = 0;
                if (isBroadcast)
                {
                    Thread.Sleep(200);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool Can_Send(byte[] data, uint id, bool externFlag, bool isBroadcast = false)
        {
            if (data.Length > 8 || data.Length <= 0) return false;
            CAN_OBJ mMsg = new CAN_OBJ();
            mMsg.ID = id;
            int min = Math.Min(data.Length, 8);
            mMsg.data = new byte[8];
            Array.Copy(data, mMsg.data, min);

            mMsg.DataLen = (byte)min;
            mMsg.ExternFlag = (byte)(externFlag ? 1 : 0);
            return Can_Send(mMsg, isBroadcast);
        }
        public void Dispose()
        {
            IsDisposed = true;
            if (IsOpened)
            {
                IsOpened = false;
                CloseDevice(false);
            }
        }
        public void ReadErrInfo(out CanErrInfo errorInfo)
        {
            ECANDLL.ReadErrInfo(1, 0, 0, out errorInfo);
        }

        #region CAN专用      
        public uint DevIndex = 0;
        public uint CanIndex = 1;
        public CanTypeInfos CanTypeInfo = new CanTypeInfos();

        IntPtr device_handle_zlg;
        IntPtr channel_handle_zlg;

        #region CanReadLoop
        System.Threading.Timer _recTimer;

        private List<CAN_OBJ> Can_RecData()
        {
            var recList = new List<CAN_OBJ>();
            switch (CanTypeInfo.CanType)
            {
                case EnumObj.CanType_Enum.PACE_CAN:
                    {
                        CAN_OBJ mMsg;
                        uint mLen = 1;
                        var result = ECANDLL.Receive(CanTypeInfo.DevType, DevIndex, CanIndex, out mMsg, mLen, 1);
                        if (!((result == ECANStatus.StatusOk) & (mLen > 0))) break;
                        if (mMsg.RemoteFlag > 0)
                            mMsg.DataLen = 0;
                        recList.Add(mMsg);
                    }
                    break;
                case EnumObj.CanType_Enum.ZLG_CAN:
                    {
                        ZCAN_Receive_Data[] can_data = new ZCAN_Receive_Data[100];
                        uint len = Method.ZCAN_GetReceiveNum(channel_handle_zlg, 0);
                        if (len > 0)
                        {
                            int size = Marshal.SizeOf(typeof(ZCAN_Receive_Data));
                            IntPtr ptr = Marshal.AllocHGlobal((int)len * size);
                            len = Method.ZCAN_Receive(channel_handle_zlg, ptr, len, 50);
                            for (int i = 0; i < len; i++)
                            {
                                var zlg_frame = (ZCAN_Receive_Data)Marshal.PtrToStructure((IntPtr)((Int64)ptr + i * size), typeof(ZCAN_Receive_Data));
                                CAN_OBJ obj = new CAN_OBJ();
                                obj.ID = Method.GetZCanIdOnlyID(zlg_frame.frame.can_id);
                                obj.DataLen = zlg_frame.frame.can_dlc;
                                obj.data = zlg_frame.frame.data;
                                var msg = Method.GetZCanIdInfo(zlg_frame.frame.can_id);
                                obj.ExternFlag = msg.ExternFlag;
                                obj.RemoteFlag = msg.RemoteFlag;
                                // obj.TimeStamp = zlg_frame.timestamp;
                                recList.Add(obj);
                            }
                            //OnRecvCANDataEvent(can_data, len);
                            Marshal.FreeHGlobal(ptr);
                        }
                    }
                    break;
                default:
                    break;
            }
            return recList;
        }

        System.Diagnostics.Stopwatch sw;
        List<CAN_OBJ> Sw_recList;
        public int RecTimer_Sw = 200;
        private void Can_RecTimerTick(object mObject)
        {
            if (!IsOpened || IsDisposed) return;
            var recList = Can_RecData();
            if (recList.Count > 0)
            {
                Sw_recList.AddRange(recList);
            }
            if (sw.ElapsedMilliseconds > RecTimer_Sw && Sw_recList.Count > 0)
            {
                CanRecvDataEvent?.Invoke(Sw_recList);
                Sw_recList.Clear();
                sw.Restart();
            }
            _recTimer.Change(10, -1);
        }
        void StartCanReadLoop()
        {
            FrmClosedWaitHandler.WaitOne();
            var tcb = new TimerCallback(Can_RecTimerTick);
            sw = new System.Diagnostics.Stopwatch();
            Sw_recList = new List<CAN_OBJ>();
            sw.Start();
            _recTimer = new System.Threading.Timer(tcb);
            _recTimer.Change(10, -1);
        }
        #endregion
        private bool OpenDevice()
        {
            switch (CanTypeInfo.CanType)
            {
                case EnumObj.CanType_Enum.PACE_CAN:
                    return ECANDLL.OpenDevice(CanTypeInfo.DevType, DevIndex, CanIndex) == ECANStatus.StatusOk;
                case EnumObj.CanType_Enum.ZLG_CAN:
                    device_handle_zlg = Method.ZCAN_OpenDevice(CanTypeInfo.DevType, DevIndex, 0);
                    if (0 == (int)device_handle_zlg)
                    {
                        return false;
                    }
                    return true;
                default:
                    return false;
            }
        }
        private bool InitDevice()
        {
            Thread.Sleep(200);
            switch (CanTypeInfo.CanType)
            {
                case EnumObj.CanType_Enum.PACE_CAN:
                    {
                        var initConfig = ECANDLL.GetInitConfig(Can1_BaudrateIndex);
                        return ECANDLL.InitCAN(CanTypeInfo.DevType, DevIndex, CanIndex, ref initConfig) == ECANStatus.StatusOk;
                    }
                case EnumObj.CanType_Enum.ZLG_CAN:
                    {
                        var type = CanTypeInfo.DevType;
                        bool netDevice = type == Define.ZCAN_CANETTCP || type == Define.ZCAN_CANETUDP ||
                   type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_400U_UDP ||
                   type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_200U_UDP || type == Define.ZCAN_CANFDNET_800U_TCP ||
                   type == Define.ZCAN_CANFDNET_800U_UDP;
                        bool canfdnetDevice = type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_400U_UDP ||
                            type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_200U_UDP || type == Define.ZCAN_CANFDNET_800U_TCP ||
                            type == Define.ZCAN_CANFDNET_800U_UDP;
                        bool pcieCanfd = type == Define.ZCAN_PCIECANFD_100U ||
                            type == Define.ZCAN_PCIECANFD_200U ||
                            type == Define.ZCAN_PCIECANFD_400U ||
                            type == Define.ZCAN_PCIECANFD_200U_EX;
                        bool usbCanfd = type == Define.ZCAN_USBCANFD_100U ||
                            type == Define.ZCAN_USBCANFD_200U ||
                            type == Define.ZCAN_USBCANFD_MINI;
                        bool canfdDevice = usbCanfd || pcieCanfd;

                        //(!setBaudrate(kBaudrate[comboBox_baud.SelectedIndex]))

                        string path = CanIndex + "/baud_rate";
                        string value = (CanBaudrate.CanBaudrateList[Can1_BaudrateIndex].Rate * 1000) + "";
                        //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
                        //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
                        var isok = Method.ZCAN_SetValue(device_handle_zlg, path, Encoding.ASCII.GetBytes(value)) == 1;
                        if (isok == false) return false;

                        var config_ = new ZCAN_CHANNEL_INIT_CONFIG
                        {
                            can_type = Define.TYPE_CAN
                        };
                        config_.can.filter = 0;
                        config_.can.acc_code = 0;
                        config_.can.acc_mask = 0xFFFFFFFF;
                        config_.can.mode = 0;

                        IntPtr pConfig = Marshal.AllocHGlobal(Marshal.SizeOf(config_));
                        Marshal.StructureToPtr(config_, pConfig, true);

                        channel_handle_zlg = Method.ZCAN_InitCAN(device_handle_zlg, CanIndex, pConfig);
                        Marshal.FreeHGlobal(pConfig);
                        return (0 != (int)channel_handle_zlg);

                    }
                default:
                    return false;
            }
        }
        private bool StartDevice(bool isRetry)
        {
            bool isok = false;
            Thread.Sleep(200);
            switch (CanTypeInfo.CanType)
            {
                case EnumObj.CanType_Enum.PACE_CAN:
                    {
                        isok = ECANDLL.StartCAN(CanTypeInfo.DevType, DevIndex, CanIndex) == ECANStatus.StatusOk;
                        if (isRetry) { }
                        else
                        {
                            IsOpened = isok;
                        }
                    }
                    break;
                case EnumObj.CanType_Enum.ZLG_CAN:
                    {
                        isok = Method.ZCAN_StartCAN(channel_handle_zlg) == Define.STATUS_OK;
                        if (isRetry) { }
                        else
                        {
                            IsOpened = isok;
                        }
                    }
                    break;
                default:
                    isok = false;
                    break;
            }
            if (isok)
            {
                StartCanReadLoop();
            }
            return isok;
        }
        private bool CloseDevice(bool isRetry)
        {
            if (isRetry == false)
            {
                IsOpened = false;
            }
            switch (CanTypeInfo.CanType)
            {
                case EnumObj.CanType_Enum.PACE_CAN:
                    return ECANDLL.CloseDevice(CanTypeInfo.DevType, DevIndex) == ECANStatus.StatusOk;
                case EnumObj.CanType_Enum.ZLG_CAN:
                    {
                        Method.ZCAN_CloseDevice(device_handle_zlg);
                        return true;
                    }
                default:
                    return false;
            }
        }
        #endregion

        public void OpenDev()
        {
            try
            {
                RecTimer_Sw = MyAppSetting.Default.GeneralSetting.CanPackInterval;
                if (this.IsOpened) return;
                lock (this.CanLock)
                {
                    if (this.OpenDevice() == false)
                    {
                        return;
                    }
                    if (this.InitDevice() == false)
                    {
                        ECANDLL.CloseDevice(1, 0);
                        return;
                    }
                    if (this.StartDevice(false))
                    {
                        this.CommStatus_Mc = EnumObj.Communication.None;
                        this.IsOpened = true;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        public void CloseDev()
        {
            CloseDevice(false);
        }
    }

}

