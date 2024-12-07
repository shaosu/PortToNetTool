using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PortToNet.Ecan
{
    [Flags]
    public enum ECANStatus : uint
    {
        /// <summary>
        ///  error
        /// </summary>
        StatusErr = 0x00000,

        /// <summary>
        /// No error
        /// </summary>
        StatusOk = 0x00001,

    }


    /// <summary>
    /// 2.定义CAN信息帧的数据类型。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CAN_OBJ
    {
        public uint ID;
        public uint TimeStamp;
        public byte TimeFlag;
        public byte SendType;
        public byte RemoteFlag;
        public byte ExternFlag;
        public byte DataLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Reserved;
    }
    /// <summary>
    /// 3.定义CAN控制器状态的数据类型。
    /// </summary>
    public struct BoardInfo
    {
        public ushort HwVersion;
        public ushort FwVersion;
        public ushort DrVersion;
        public ushort InVersion;
        public ushort IrqNum;
        public byte CanNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] StrSerialNum;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] StrHwType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Reserved;
    }

    /// <summary>
    /// 4.定义错误信息的数据类型。
    /// </summary>
    public struct CanErrInfo
    {
        public uint ErrCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] PassiveErrData;
        public byte ArLostErrData;

    }

    /// <summary>
    /// 5.定义初始化CAN的数据类型
    /// </summary>
    public struct InitConfig
    {
        public uint AccCode;
        public uint AccMask;
        public uint Reserved;
        public byte Filter;
        public byte Timing0;
        public byte Timing1;
        public byte Mode;
    }




}

