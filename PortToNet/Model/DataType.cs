using HandyControl.Tools.Extension;
using PortToNet.Ecan;
using System;
using System.Data;
using static PubMod;

namespace PortToNet.Model
{
    public enum WorkMode
    {
        SingleWorkingMode,
        DTUMode,
        PortToNetMode,
        NetToPortMode
    }

    public enum NetProtocolType
    {
        TCP_Client,
        TCP_Server,
        UDP,
        UDP_Server
    }

    public enum ShowTimeFormat
    {
        None,
        Default,
        Second,
        SecondAndMs,
        /// <summary>
        /// 比较
        /// </summary>
        Compare,
        CompareSecond,
        CpSecondAndMs,
        /// <summary>
        /// 比较相对的SecondAndMs
        /// </summary>
        CpRelativeSecondAndMs,
        UnixSecond,
        Tick,
        UnixSecondAndMs,
    }


    public enum PackType : byte
    {
        Heart,
        Data,
        CanData,
    }
    public class PackCanFrame
    {
        public PackType Type { get; set; }
        public ushort PDULength { get; set; }
        public byte[] PDU { get; set; }

        public static byte[] PackHeart()
        {
            var rt = new byte[1 + 2];
            rt[0] = (byte)PackType.Heart;
            return rt;
        }
        public static byte[] PackData(byte[] obj)
        {
            var rt = new byte[1 + 2 + obj.Length];
            rt[0] = (byte)PackType.Data;
            PubMod.ShortHL t32 = new PubMod.ShortHL();
            t32.data = (ushort)obj.Length;
            rt[1] = t32.HByte;
            rt[2] = t32.LByte;
            Array.Copy(obj, 0, rt, 3, obj.Length);
            return rt;
        }
        public static byte[] UnPackData(byte[] data)
        {
            PackType type = GetPackType(data);
            if (type != PackType.Data) return new byte[0];

            int index = 1;
            int PDULength = ShortHL.ReadUShortData(data, ref index);
            byte[] rt = new byte[PDULength];
            Array.Copy(data, index, rt, 0, PDULength);
            return rt;
        }

        private static byte[] Pack(CAN_OBJ obj)
        {
            List<byte> bytes = new List<byte>();
            byte rr = 0;
            if (obj.RemoteFlag > 0)
            {
                rr |= 1;
            }
            if (obj.ExternFlag > 0)
            {
                rr |= 2;
            }
            bytes.Add(rr);
            PubMod.Int32Bytes t32 = new PubMod.Int32Bytes();
            t32.data = obj.ID;
            bytes.Add(t32.HHByte);
            bytes.Add(t32.HLByte);
            bytes.Add(t32.LHByte);
            bytes.Add(t32.LLByte);
            bytes.Add(obj.DataLen); // DLC
            byte[] d = new byte[obj.DataLen];
            int min = Math.Min(obj.DataLen, obj.data.Length);
            Array.Copy(obj.data, d, min);
            bytes.AddRange(d);
            return bytes.ToArray();
        }
        private static CAN_OBJ UnPack(byte[] data, ref int index)
        {
            CAN_OBJ obj = new CAN_OBJ();
            obj.data = new byte[8];
            byte rr = data[index++];
            if ((rr & 1) == 1)
            {
                obj.RemoteFlag = 1;
            }
            if ((rr & 2) == 2)
            {
                obj.ExternFlag = 1;
            }

            obj.ID = Int32Bytes.ReadUInt32Data(data, ref index);
            obj.DataLen = data[index++];
            for (int i = 0; i < obj.DataLen; i++)
            {
                obj.data[i] = data[index + i];
            }
            index += obj.DataLen;
            return obj;
        }
        public static byte[] PackCan(List<CAN_OBJ> obj)
        {
            List<byte> bs = new List<byte>();
            for (int i = 0; i < obj.Count; i++)
            {
                bs.AddRange(Pack(obj[i]));
            }

            List<byte> bs2 = new List<byte>();
            bs2.Add((byte)PackType.CanData);
            PubMod.ShortHL t32 = new PubMod.ShortHL();
            t32.data = (ushort)bs.Count;
            bs2.Add(t32.HByte);
            bs2.Add(t32.LByte);

            bs2.AddRange(bs);
            return bs2.ToArray();
        }
        public static List<CAN_OBJ> UnPackCan(byte[] data)
        {
            PackType type = GetPackType(data);
            if (type != PackType.CanData) return new List<CAN_OBJ>();
            int index = 1;
            int PDULength = ShortHL.ReadUShortData(data, ref index);
            List<CAN_OBJ> rt = new List<CAN_OBJ>();
            while (index < data.Length)
            {
                CAN_OBJ obj = UnPack(data, ref index);
                rt.Add(obj);
            }
            return rt;
        }
        public static byte[] UnPackCan_Data(byte[] data)
        {
            PackType type = GetPackType(data);
            if (type != PackType.CanData) return new byte[0];

            int index = 1;
            int PDULength = ShortHL.ReadUShortData(data, ref index);
            byte[] rt = new byte[PDULength];
            Array.Copy(data, index, rt, 0, PDULength);
            return rt;
        }


        public static PackType GetPackType(byte[] data)
        {
            return (PackType)data[0];
        }

    }

    public class PortSendDataMsg
    {
        public bool IsCan { get; set; }
        public byte[] Data { get; set; }
        public uint CanID { get; set; }
        public bool ExternFlag { get; set; }
        public bool RemoteFlag { get; set; }

        public CAN_OBJ BuildCanData()
        {
            CAN_OBJ can = new CAN_OBJ();
            can.ID = CanID;
            can.data = new byte[8];
            int min = Math.Min(8, Data.Length);
            Array.Copy(Data, can.data, min);
            can.ExternFlag = (byte)(ExternFlag ? 1 : 0);
            can.RemoteFlag = (byte)(RemoteFlag ? 1 : 0);
            can.DataLen = (byte)min;
            return can;
        }

        public static PortSendDataMsg BuildMsg(CAN_OBJ can)
        {
            PortSendDataMsg msg = new PortSendDataMsg();
            msg.IsCan = true;
            msg.CanID = can.ID;
            msg.ExternFlag = can.ExternFlag > 0;
            msg.RemoteFlag = can.RemoteFlag > 0;
            msg.Data = new byte[can.DataLen];
            int min = Math.Min(8, (int)can.DataLen);
            Array.Copy(can.data, msg.Data, min);
            return msg;
        }
        public byte[] BuildNetData()
        {
            if (IsCan)
            {
                List<byte> bytes = new List<byte>();
                PubMod.Int32Bytes t32 = new PubMod.Int32Bytes();
                t32.data = CanID;
                bytes.Add(t32.HHByte);
                bytes.Add(t32.HLByte);
                bytes.Add(t32.LHByte);
                bytes.Add(t32.LLByte);
                bytes.Add((byte)(ExternFlag ? 1 : 0));
                bytes.Add((byte)(RemoteFlag ? 1 : 0));
                bytes.Add((byte)Data.Length);
                bytes.AddRange(Data);
                return bytes.ToArray();
            }
            else
            {
                return Data;
            }
        }
    }
}
