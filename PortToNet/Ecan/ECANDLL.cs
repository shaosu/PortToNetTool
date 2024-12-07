using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PortToNet.Ecan
{
    public static class ECANDLL
    {

        private const string canDllPath = "Config\\ZL\\ECanVci.dll";
        //private const string canDllPath = "Config\\ZL\\ControlCan.dll";
        [DllImport(canDllPath, EntryPoint = "OpenDevice")]
        public static extern ECANStatus OpenDevice(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 Reserved);

        [DllImport(canDllPath, EntryPoint = "CloseDevice")]
        public static extern ECANStatus CloseDevice(
            UInt32 DeviceType,
            UInt32 DeviceInd);


        [DllImport(canDllPath, EntryPoint = "InitCAN")]
        public static extern ECANStatus InitCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            ref InitConfig InitConfig);


        [DllImport(canDllPath, EntryPoint = "StartCAN")]
        public static extern ECANStatus StartCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd);


        [DllImport(canDllPath, EntryPoint = "ResetCAN")]
        public static extern ECANStatus ResetCAN(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd);


        [DllImport(canDllPath, EntryPoint = "Transmit")]
        public static extern ECANStatus Transmit(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            ref CAN_OBJ Send,
            UInt16 length);


        [DllImport(canDllPath, EntryPoint = "Receive")]
        public static extern ECANStatus Receive(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            out CAN_OBJ Receive,
            UInt32 length,
            UInt32 WaitTime);


        [DllImport(canDllPath, EntryPoint = "ReadErrInfo")]
        public static extern ECANStatus ReadErrInfo(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            UInt32 CANInd,
            out CanErrInfo ReadErrInfo);



        [DllImport(canDllPath, EntryPoint = "ReadBoardInfo")]
        public static extern ECANStatus ReadBoardInfo(
            UInt32 DeviceType,
            UInt32 DeviceInd,
            out BoardInfo ReadErrInfo);

        public static InitConfig GetInitConfig(int Can1_BaudrateIndex)
        {
            var cg = new InitConfig
            {
                AccCode = 0,
                AccMask = 0xffffff,
                Filter = 0
            };
            switch (Can1_BaudrateIndex)
            {
                case 0: //1000
                    cg.Timing0 = 0;
                    cg.Timing1 = 0x14;
                    break;
                case 1: //800
                    cg.Timing0 = 0;
                    cg.Timing1 = 0x16;
                    break;
                case 2: //666
                    cg.Timing0 = 0x80;
                    cg.Timing1 = 0xb6;
                    break;
                case 3: //500
                    cg.Timing0 = 0;
                    cg.Timing1 = 0x1c;
                    break;
                case 4: //400
                    cg.Timing0 = 0x80;
                    cg.Timing1 = 0xfa;
                    break;
                case 5: //250
                    cg.Timing0 = 0x01;
                    cg.Timing1 = 0x1c;
                    break;
                case 6: //200
                    cg.Timing0 = 0x81;
                    cg.Timing1 = 0xfa;
                    break;
                case 7: //125
                    cg.Timing0 = 0x03;
                    cg.Timing1 = 0x1c;
                    break;
                case 8: //100
                    cg.Timing0 = 0x04;
                    cg.Timing1 = 0x1c;
                    break;
                case 9: //80
                    cg.Timing0 = 0x83;
                    cg.Timing1 = 0xff;
                    break;
                case 10: //50
                    cg.Timing0 = 0x09;
                    cg.Timing1 = 0x1c;
                    break;
            }
            cg.Mode = 0;
            return cg;
        }



    }
}

