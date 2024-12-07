using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


public sealed class PubMod
{
    /// <summary>
    /// 闭式钳位（可以取到最大/最小值）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static T ClampC<T>(T value, T min, T max) where T : struct, IComparable<T>
    {
        T val = value;
        if (value.CompareTo(min) < 0)
        {
            val = min;
        }
        if (value.CompareTo(max) > 0)
        {
            val = max;
        }
        return val;
    }
    public static bool HexStringToUint(string value, out uint tmp)
    {
        tmp = 0;
        try
        {
            tmp = Convert.ToUInt32(value, 16);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public static string ValueToBinString(int value, int SpaceSize = 4)
    {
        string tmp1 = Convert.ToString(value, 2);
        return StringToAppendSpace(tmp1);
    }

    public static string StringToAppendSpace(string tmp, int SpaceSize = 4)
    {
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("(.{4})");
        switch (SpaceSize)
        {
            case 8:
                regex = new System.Text.RegularExpressions.Regex("(.{8})");
                break;
            case 16:
                regex = new System.Text.RegularExpressions.Regex("(.{16})");
                break;
            default:
                break;
        }
        return regex.Replace(tmp, "$1 ");
    }

    public static bool CheckStringIsHexFormat(string val)
    {
        if (string.IsNullOrWhiteSpace(val))
            return true;
        Regex rx = new Regex(@"[^0-9a-fA-F\s]");
        bool ok = !rx.Match(val).Success;
        if (ok)
        {
            Regex rx2 = new Regex(@"\s*");
            string val2 = rx2.Replace(val, "");
            return (val2.Length >= 2 && val2.Length % 2 == 0);
        }
        return ok;
    }

    public static byte[] GetHexFromString(string val)
    {
        bool IsHExFormat = CheckStringIsHexFormat(val);
        if (IsHExFormat)
        {
            Regex rx = new Regex(@"\s*");
            string val2 = rx.Replace(val, "");
            if (val2.Length >= 2 && val2.Length % 2 == 0)
            {
                byte[] data = new byte[val2.Length / 2];
                byte Cur = 0;
                string tmp = "";
                int j = 0;
                for (int i = 0; i < val2.Length; i += 2)
                {
                    tmp = val2.Substring(i, 2);
                    Cur = Convert.ToByte(tmp, 16);
                    data[j] = Cur;
                    j++;
                }
                return data;
            }
        }
        return new byte[0];
    }

    /// <summary>
    /// 功能    :整数转换为二进制数
    /// 更新人  :su.
    /// 更新时间:2020年12月25日17:16:49
    /// </summary>
    /// <param name="value">值</param>
    /// <param name="SpaceSize">个几个数产生空格</param>
    /// <returns>
    /// 6 0110
    /// </returns>
    public static string ValueToBinString(uint value, int SpaceSize = 4)
    {
        string tmp1 = Convert.ToString(value, 2);
        return StringToAppendSpace(tmp1);
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct FloatHL
    {
        [FieldOffset(0)]
        public float data;

        [FieldOffset(0)]
        public byte A;

        [FieldOffset(1)]
        public byte B;

        [FieldOffset(2)]
        public byte C;

        [FieldOffset(3)]
        public byte D;

        public FloatHL(byte a, byte b, byte c, byte d)
        {
            data = 0;
            D = d;
            C = c;
            A = a;
            B = b;
        }
        public static implicit operator FloatHL(float data)
        {
            FloatHL hl = new FloatHL();
            hl.data = data;
            return hl;
        }
        public static implicit operator float(FloatHL hl)
        {
            return hl.data;
        }
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct ShortHL
    {
        [FieldOffset(0)]
        public short sdata;
        [FieldOffset(0)]
        public ushort data;
        [FieldOffset(1)]
        [MarshalAs(UnmanagedType.I1)]
        public byte HByte;
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I1)]
        public byte LByte;

        public ShortHL(byte h, byte l)
        {
            data = 0;
            sdata = 0;
            HByte = h;
            LByte = l;
        }
        public static implicit operator ShortHL(ushort data)
        {
            ShortHL hl = new ShortHL();
            hl.data = data;
            return hl;
        }
        //public static implicit operator ShortHL(short data)
        //{
        //    ShortHL hl = new ShortHL();
        //    hl.sdat = data;
        //    return hl;
        //}
        public static implicit operator ushort(ShortHL hl)
        {
            return hl.data;
        }

        public static ushort ReadUShortData(byte[] src, ref int index)
        {
            if (src.Length < index + 2) return 0;
            ShortHL hl = (ushort)0;
            hl.HByte = src[index];
            hl.LByte = src[index + 1];
            index += 2;
            return hl.data;
        }
        public static short ReadShortData(byte[] src, ref int index)
        {
            var data = ReadUShortData(src, ref index);
            return (short)data;
        }
        public static void WriteUShortData(byte[] src, ref int index, ushort data)
        {
            if (src.Length < index + 2) return;
            ShortHL hl = data;
            hl.HByte = src[index] = hl.HByte;
            hl.LByte = src[index + 1] = hl.LByte;
            index += 2;
        }
        public static void WriteShortData(byte[] src, ref int index, short data)
        {
            WriteUShortData(src, ref index, (ushort)data);
        }


    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Int32Bytes
    {
        [FieldOffset(0)]
        public int sdata;
        [FieldOffset(0)]
        public uint data;

        [FieldOffset(3)]
        [MarshalAs(UnmanagedType.I1)]
        public byte HHByte;
        [FieldOffset(2)]
        [MarshalAs(UnmanagedType.I1)]
        public byte HLByte;
        [FieldOffset(1)]
        [MarshalAs(UnmanagedType.I1)]
        public byte LHByte;
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I1)]
        public byte LLByte;
        public Int32Bytes(byte hh, byte hl, byte lh, byte ll)
        {
            data = 0;
            sdata = 0;

            HHByte = hh;
            HLByte = hl;
            LHByte = lh;
            LLByte = ll;
        }
        public static implicit operator Int32Bytes(uint data)
        {
            Int32Bytes hl = new Int32Bytes();
            hl.data = data;
            return hl;
        }
        public static implicit operator Int32Bytes(int data)
        {
            Int32Bytes hl = new Int32Bytes();
            hl.sdata = data;
            return hl;
        }
        public static implicit operator uint(Int32Bytes hl)
        {
            return hl.data;
        }
        public static uint ReadUInt32Data(byte[] src, ref int index)
        {
            if (src.Length < index + 4) return 0;
            Int32Bytes hl = (uint)0;
            hl.HHByte = src[index];
            hl.HLByte = src[index + 1];
            hl.LHByte = src[index + 2];
            hl.LLByte = src[index + 3];
            index += 4;
            return hl.data;
        }
        public static int ReadInt32Data(byte[] src, ref int index)
        {
            var data = ReadUInt32Data(src, ref index);
            return (int)data;
        }
        public static void WriteUInt32Data(byte[] src, ref int index, uint data)
        {
            if (src.Length < index + 4) return;
            Int32Bytes hl = (uint)data;
            src[index] = hl.HHByte;
            src[index + 1] = hl.HLByte;
            src[index + 2] = hl.LHByte;
            src[index + 3] = hl.LLByte;
            index += 4;
        }
        public static void WriteInt32Data(byte[] src, ref int index, int data)
        {
            WriteUInt32Data(src, ref index, (uint)data);
        }
    }

    public static string GetDateTimeNowString()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH-mm-sss.fff");
    }

    /// <summary>
    /// 获取校验和
    /// </summary>
    /// <param name="Command"></param>
    /// <returns></returns>
    public static string ADD2(string Command)
    {
        byte[] data = Encoding.ASCII.GetBytes(Command);
        int sum = 0;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i];
        }
        sum = sum % 256;

        sum = ~sum + 1;
        byte a = (byte)sum;
        string bin = Convert.ToString(a, 2);
        string verify = Convert.ToInt32(bin, 2).ToString("X2");

        return verify;
    }

    /// <summary>
    /// BBC校验(异或校验)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GetBCC(byte[] data)
    {
        String ret = "";
        byte[] BCC = new byte[1];
        for (int i = 0; i < data.Length; i++)
        {
            BCC[0] = (byte)(BCC[0] ^ data[i]);
        }
        String hex = (BCC[0] & 0xFF).ToString("X");

        if (hex.Length == 1)
        {
            hex = '0' + hex;
        }
        ret += hex.ToUpper();
        return ret;
    }

    /// <summary>
    /// CS校验:Eg:标准188协议校验
    /// </summary>
    /// <param name="data">待校验Byte数组</param>
    /// <returns>CS校验值</returns>
    public static byte CS(byte[] data)
    {
        byte cs = 0;
        for (int i = 0; i < data.Length; i++)
        {
            cs = (byte)((cs + data[i]) % 256);
        }
        return cs;
    }

    /// <summary>
    /// LRC和校验
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte LRC(byte[] data)
    {
        byte lrc = 0;
        foreach (byte c in data)
        {
            lrc += c;
        }
        return (byte)-lrc;
    }

    /// <summary> 
    /// CRC8位校验表 
    /// </summary> 
    private readonly static byte[] CRC8Table = new byte[] {
        0,94,188,226,97,63,221,131,194,156,126,32,163,253,31,65,
        157,195,33,127,252,162,64,30, 95,1,227,189,62,96,130,220,
        35,125,159,193,66,28,254,160,225,191,93,3,128,222,60,98,
        190,224,2,92,223,129,99,61,124,34,192,158,29,67,161,255,
        70,24,250,164,39,121,155,197,132,218,56,102,229,187,89,7,
        219,133,103,57,186,228,6,88,25,71,165,251,120,38,196,154,
        101,59,217,135,4,90,184,230,167,249,27,69,198,152,122,36,
        248,166,68,26,153,199,37,123,58,100,134,216,91,5,231,185,
        140,210,48,110,237,179,81,15,78,16,242,172,47,113,147,205,
        17,79,173,243,112,46,204,146,211,141,111,49,178,236,14,80,
        175,241,19,77,206,144,114,44,109,51,209,143,12,82,176,238,
        50,108,142,208,83,13,239,177,240,174,76,18,145,207,45,115,
        202,148,118,40,171,245,23,73,8,86,180,234,105,55,213,139,
        87,9,235,181,54,104,138,212,149,203, 41,119,244,170,72,22,
        233,183,85,11,136,214,52,106,43,117,151,201,74,20,246,168,
        116,42,200,150,21,75,169,247,182,232,10,84,215,137,107,53 };

    public static byte GetCRC8(byte[] buffer)
    {
        return GetCRC8(buffer, 0, buffer.Length);
    }

    public static byte GetCRC8(byte[] buffer, int off, int len)
    {
        byte crc = 0;
        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }
        if (off < 0 || len < 0 || off + len > buffer.Length)
        {
            throw new ArgumentOutOfRangeException();
        }

        for (int i = off; i < len; i++)
        {
            crc = CRC8Table[crc ^ buffer[i]];
        }
        return crc;
    }

    /// <summary>
    /// 表格计算
    /// </summary>
    /// <param name="baseCrc">>第一次为0xFFFF</param>
    /// <param name="buf"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    public static ushort CRC16_Base(ushort baseCrc, byte[] buf, int len)
    {
        byte hi, lo, tmp;
        ushort crc;
        hi = (byte)(baseCrc >> 8);
        lo = (byte)(baseCrc & 0xFF);
        int i = 0;
        while (len > 0)
        {
            len--;
            tmp = (byte)(lo ^ (buf[i]));
            i++;
            lo = (byte)(hi ^ gpbtCRCLo[tmp]);
            hi = gpbtCRCHi[tmp];
        }
        crc = hi;
        crc <<= 8;
        crc += lo;
        return crc;
    }

    /// <summary>
    /// CRC16 低位在前
    /// </summary>
    /// <param name="Crc16_num"></param>
    /// <param name="nLength"></param>
    /// <returns></returns>
    public static ushort Get_CRC16(byte[] Crc16_num, int nLength)
    {
        ushort crc = 0xffff;
        crc = CRC16_Base(crc, Crc16_num, nLength);
        return crc;
    }

    private readonly static UInt32[] Crc32Table = {
0x00000000,0x77073096,0xEE0E612C,0x990951BA,
0x076DC419,0x706AF48F,0xE963A535,0x9E6495A3,
0x0EDB8832,0x79DCB8A4,0xE0D5E91E,0x97D2D988,
0x09B64C2B,0x7EB17CBD,0xE7B82D07,0x90BF1D91,
0x1DB71064,0x6AB020F2,0xF3B97148,0x84BE41DE,
0x1ADAD47D,0x6DDDE4EB,0xF4D4B551,0x83D385C7,
0x136C9856,0x646BA8C0,0xFD62F97A,0x8A65C9EC,
0x14015C4F,0x63066CD9,0xFA0F3D63,0x8D080DF5,
0x3B6E20C8,0x4C69105E,0xD56041E4,0xA2677172,
0x3C03E4D1,0x4B04D447,0xD20D85FD,0xA50AB56B,
0x35B5A8FA,0x42B2986C,0xDBBBC9D6,0xACBCF940,
0x32D86CE3,0x45DF5C75,0xDCD60DCF,0xABD13D59,
0x26D930AC,0x51DE003A,0xC8D75180,0xBFD06116,
0x21B4F4B5,0x56B3C423,0xCFBA9599,0xB8BDA50F,
0x2802B89E,0x5F058808,0xC60CD9B2,0xB10BE924,
0x2F6F7C87,0x58684C11,0xC1611DAB,0xB6662D3D,
0x76DC4190,0x01DB7106,0x98D220BC,0xEFD5102A,
0x71B18589,0x06B6B51F,0x9FBFE4A5,0xE8B8D433,
0x7807C9A2,0x0F00F934,0x9609A88E,0xE10E9818,
0x7F6A0DBB,0x086D3D2D,0x91646C97,0xE6635C01,
0x6B6B51F4,0x1C6C6162,0x856530D8,0xF262004E,
0x6C0695ED,0x1B01A57B,0x8208F4C1,0xF50FC457,
0x65B0D9C6,0x12B7E950,0x8BBEB8EA,0xFCB9887C,
0x62DD1DDF,0x15DA2D49,0x8CD37CF3,0xFBD44C65,
0x4DB26158,0x3AB551CE,0xA3BC0074,0xD4BB30E2,
0x4ADFA541,0x3DD895D7,0xA4D1C46D,0xD3D6F4FB,
0x4369E96A,0x346ED9FC,0xAD678846,0xDA60B8D0,
0x44042D73,0x33031DE5,0xAA0A4C5F,0xDD0D7CC9,
0x5005713C,0x270241AA,0xBE0B1010,0xC90C2086,
0x5768B525,0x206F85B3,0xB966D409,0xCE61E49F,
0x5EDEF90E,0x29D9C998,0xB0D09822,0xC7D7A8B4,
0x59B33D17,0x2EB40D81,0xB7BD5C3B,0xC0BA6CAD,
0xEDB88320,0x9ABFB3B6,0x03B6E20C,0x74B1D29A,
0xEAD54739,0x9DD277AF,0x04DB2615,0x73DC1683,
0xE3630B12,0x94643B84,0x0D6D6A3E,0x7A6A5AA8,
0xE40ECF0B,0x9309FF9D,0x0A00AE27,0x7D079EB1,
0xF00F9344,0x8708A3D2,0x1E01F268,0x6906C2FE,
0xF762575D,0x806567CB,0x196C3671,0x6E6B06E7,
0xFED41B76,0x89D32BE0,0x10DA7A5A,0x67DD4ACC,
0xF9B9DF6F,0x8EBEEFF9,0x17B7BE43,0x60B08ED5,
0xD6D6A3E8,0xA1D1937E,0x38D8C2C4,0x4FDFF252,
0xD1BB67F1,0xA6BC5767,0x3FB506DD,0x48B2364B,
0xD80D2BDA,0xAF0A1B4C,0x36034AF6,0x41047A60,
0xDF60EFC3,0xA867DF55,0x316E8EEF,0x4669BE79,
0xCB61B38C,0xBC66831A,0x256FD2A0,0x5268E236,
0xCC0C7795,0xBB0B4703,0x220216B9,0x5505262F,
0xC5BA3BBE,0xB2BD0B28,0x2BB45A92,0x5CB36A04,
0xC2D7FFA7,0xB5D0CF31,0x2CD99E8B,0x5BDEAE1D,
0x9B64C2B0,0xEC63F226,0x756AA39C,0x026D930A,
0x9C0906A9,0xEB0E363F,0x72076785,0x05005713,
0x95BF4A82,0xE2B87A14,0x7BB12BAE,0x0CB61B38,
0x92D28E9B,0xE5D5BE0D,0x7CDCEFB7,0x0BDBDF21,
0x86D3D2D4,0xF1D4E242,0x68DDB3F8,0x1FDA836E,
0x81BE16CD,0xF6B9265B,0x6FB077E1,0x18B74777,
0x88085AE6,0xFF0F6A70,0x66063BCA,0x11010B5C,
0x8F659EFF,0xF862AE69,0x616BFFD3,0x166CCF45,
0xA00AE278,0xD70DD2EE,0x4E048354,0x3903B3C2,
0xA7672661,0xD06016F7,0x4969474D,0x3E6E77DB,
0xAED16A4A,0xD9D65ADC,0x40DF0B66,0x37D83BF0,
0xA9BCAE53,0xDEBB9EC5,0x47B2CF7F,0x30B5FFE9,
0xBDBDF21C,0xCABAC28A,0x53B39330,0x24B4A3A6,
0xBAD03605,0xCDD70693,0x54DE5729,0x23D967BF,
0xB3667A2E,0xC4614AB8,0x5D681B02,0x2A6F2B94,
0xB40BBE37,0xC30C8EA1,0x5A05DF1B,0x2D02EF8D};

    /// <summary>
    /// 获取文件的CRC32标识
    /// </summary>
    /// <param name="FilePath"></param>
    /// <returns></returns>
    public static String GetFileCRC32(string FilePath)
    {
        const string FOO = "-";
        if (string.IsNullOrEmpty(FilePath))
        {
            return FOO;
        }
        if (!File.Exists(FilePath))
        {
            return FOO;
        }

        // 最大50M
        const int MAX_SIZE = 50 * 1024 * 1024;
        var f = new FileInfo(FilePath);
        if (f.Length >= MAX_SIZE)
        {
            return FOO;
        }
        return GetCRC32(File.ReadAllBytes(FilePath));
    }

    public static string GetCRC32(byte[] bin)
    {
        UInt32 crc = 0xFFFFFFFF;
        foreach (byte b in bin)
        {
            crc = ((crc >> 8) & 0x00FFFFFF) ^ Crc32Table[(crc ^ b) & 0xFF];
        }
        crc = crc ^ 0xFFFFFFFF;
        return crc.ToString("X");
    }

    /// <summary>
    /// 获取BitCount位数据需要占用多少字节
    /// </summary>
    /// <param name="BitCount"></param>
    /// <returns></returns>
    public static int GetBitByteCount(int BitCount)
    {
        int c = BitCount / 8;
        if (BitCount % 8 != 0)
            c++;
        return c;
    }

    /// <summary>
    /// CRC16 低位在前
    /// </summary>
    /// <param name="Crc16_num"></param>
    /// <param name="nLength"></param>
    /// <returns></returns>
    public static ushort Get_CRC16(ushort crc, byte[] Crc16_num, int nLength)
    {
        crc = CRC16_Base(crc, Crc16_num, nLength);
        return crc;
    }
    /// <summary>
    /// CRC16低位表
    /// </summary>
    private static readonly byte[] gpbtCRCLo =
    {
    0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0,
    0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41,
    0x00, 0xc1, 0x81, 0x40, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0,
    0x80, 0x41, 0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1, 0x81, 0x40,
    0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1,
    0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0, 0x80, 0x41,
    0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1,
    0x81, 0x40, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41,
    0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0,
    0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x00, 0xc1, 0x81, 0x40,
    0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1,
    0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1, 0x81, 0x40,
    0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0,
    0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x00, 0xc1, 0x81, 0x40,
    0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0,
    0x80, 0x41, 0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1, 0x81, 0x40,
    0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0,
    0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41,
    0x00, 0xc1, 0x81, 0x40, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0,
    0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41,
    0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0,
    0x80, 0x41, 0x00, 0xc1, 0x81, 0x40, 0x00, 0xc1, 0x81, 0x40,
    0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0, 0x80, 0x41, 0x00, 0xc1,
    0x81, 0x40, 0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41,
    0x00, 0xc1, 0x81, 0x40, 0x01, 0xc0, 0x80, 0x41, 0x01, 0xc0,
    0x80, 0x41, 0x00, 0xc1, 0x81, 0x40
};
    /// <summary>
    /// CRC16高位表
    /// </summary>
    private static readonly byte[] gpbtCRCHi =
    {
    0x00, 0xc0, 0xc1, 0x01, 0xc3, 0x03, 0x02, 0xc2, 0xc6, 0x06,
    0x07, 0xc7, 0x05, 0xc5, 0xc4, 0x04, 0xcc, 0x0c, 0x0d, 0xcd,
    0x0f, 0xcf, 0xce, 0x0e, 0x0a, 0xca, 0xcb, 0x0b, 0xc9, 0x09,
    0x08, 0xc8, 0xd8, 0x18, 0x19, 0xd9, 0x1b, 0xdb, 0xda, 0x1a,
    0x1e, 0xde, 0xdf, 0x1f, 0xdd, 0x1d, 0x1c, 0xdc, 0x14, 0xd4,
    0xd5, 0x15, 0xd7, 0x17, 0x16, 0xd6, 0xd2, 0x12, 0x13, 0xd3,
    0x11, 0xd1, 0xd0, 0x10, 0xf0, 0x30, 0x31, 0xf1, 0x33, 0xf3,
    0xf2, 0x32, 0x36, 0xf6, 0xf7, 0x37, 0xf5, 0x35, 0x34, 0xf4,
    0x3c, 0xfc, 0xfd, 0x3d, 0xff, 0x3f, 0x3e, 0xfe, 0xfa, 0x3a,
    0x3b, 0xfb, 0x39, 0xf9, 0xf8, 0x38, 0x28, 0xe8, 0xe9, 0x29,
    0xeb, 0x2b, 0x2a, 0xea, 0xee, 0x2e, 0x2f, 0xef, 0x2d, 0xed,
    0xec, 0x2c, 0xe4, 0x24, 0x25, 0xe5, 0x27, 0xe7, 0xe6, 0x26,
    0x22, 0xe2, 0xe3, 0x23, 0xe1, 0x21, 0x20, 0xe0, 0xa0, 0x60,
    0x61, 0xa1, 0x63, 0xa3, 0xa2, 0x62, 0x66, 0xa6, 0xa7, 0x67,
    0xa5, 0x65, 0x64, 0xa4, 0x6c, 0xac, 0xad, 0x6d, 0xaf, 0x6f,
    0x6e, 0xae, 0xaa, 0x6a, 0x6b, 0xab, 0x69, 0xa9, 0xa8, 0x68,
    0x78, 0xb8, 0xb9, 0x79, 0xbb, 0x7b, 0x7a, 0xba, 0xbe, 0x7e,
    0x7f, 0xbf, 0x7d, 0xbd, 0xbc, 0x7c, 0xb4, 0x74, 0x75, 0xb5,
    0x77, 0xb7, 0xb6, 0x76, 0x72, 0xb2, 0xb3, 0x73, 0xb1, 0x71,
    0x70, 0xb0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
    0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9c, 0x5c,
    0x5d, 0x9d, 0x5f, 0x9f, 0x9e, 0x5e, 0x5a, 0x9a, 0x9b, 0x5b,
    0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4b, 0x8b,
    0x8a, 0x4a, 0x4e, 0x8e, 0x8f, 0x4f, 0x8d, 0x4d, 0x4c, 0x8c,
    0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
    0x43, 0x83, 0x41, 0x81, 0x80, 0x40
};

    public static string ArrayToHexString<T>(T[] data, bool AddendSpace = true)
    {
        if (data == null || data.Length == 0) return "";
        StringBuilder sb = new StringBuilder();
        int C = data.Length;
        int i = 0;
        while (C > i)
        {
            sb.Append(string.Format("{0:X2}", data[i]));
            if (AddendSpace)
                sb.Append(" ");
            i++;
        }
        if (sb.Length >= 1)
            sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
    public static string ArrayToHexString<T>(T[] data, int len, bool AddendSpace = true)
    {
        if (len == 0) return string.Empty;
        if (data == null || data.Length == 0) return string.Empty;

        StringBuilder sb = new StringBuilder();
        int C = Math.Min(data.Length, len);
        int i = 0;
        while (C > i)
        {
            sb.Append(string.Format("{0:X2}", data[i]));
            if (AddendSpace)
                sb.Append(" ");
            i++;
        }
        if (sb.Length >= 1)
            sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
    public static ushort[] ByteArryToUshortArry(byte[] data)
    {
        if (data == null) return null;
        int c = data.Length;
        if (data.Length % 2 != 0) c--;
        ushort[] rt = new ushort[c / 2];
        for (int i = 0; i < c / 2; i++)
        {
            rt[i] = (ushort)((data[i * 2] << 8) | data[i * 2 + 1]);
        }
        return rt;
    }

    public static byte[] UshortArryToByteArry(ushort[] data)
    {
        if (data == null) return null;
        byte[] rt = new byte[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            rt[i * 2] = (byte)(data[i] >> 8);
            rt[i * 2 + 1] = (byte)(data[i] & 0xFF);
        }
        return rt;
    }

    /// <summary>
    /// OK
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool[] ByteArryToBitArry(byte[] data)
    {
        if (data == null) return null;
        List<bool> rt = new List<bool>(data.Length * 8);
        for (int i = 0; i < data.Length; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                bool tmp = (data[i] & (1 << j)) > 0;
                rt.Add(tmp);
            }
        }
        return rt.ToArray();
    }

    /// <summary>
    /// OK
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool[] Uint32ToBitArry(UInt32 data)
    {
        List<bool> rt = new List<bool>(32);
        for (int i = 0; i < 32; i++)
        {
            rt.Add((data & (1 << i)) > 0);
        }
        return rt.ToArray();
    }

    /// <summary>
    /// OK
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static byte[] BitArryToByteArry(bool[] data)
    {
        if (data == null) return null;
        int len = data.Length / 8;
        if (data.Length % 8 != 0)
        {
            len += 1;
        }
        byte[] rt = new byte[len];
        for (int i = 0; i < len; i++)
        {
            byte tmpv = 0;
            for (int j = 0; j < 8; j++)
            {
                if (i * 8 + j < data.Length && data[i * 8 + j])
                {
                    tmpv = (byte)(tmpv | (1 << j));
                }
            }
            rt[i] = tmpv;
        }
        return rt;
    }

    public enum MyOSPlatform
    {
        Win = 0,
        UnixLike = 1,
    }

    public static bool IsWindows
    {
        get
        {
            PlatformID id = Environment.OSVersion.Platform;
            return id == PlatformID.Win32Windows || id == PlatformID.Win32NT; // WinCE not supported
        }
    }

    /// <summary>
    /// 获取操作系统平台
    /// </summary>
    /// <returns></returns>
    public static MyOSPlatform GetPlatform()
    {
        if (IsWindows)
            return MyOSPlatform.Win;
        return MyOSPlatform.UnixLike;
    }

    public static string GetLocalIPv4(NetworkInterfaceType _type)
    {
        string output = "";
        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
            // 网络类型是所规定的并且网络再运行状态
            if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        output = ip.Address.ToString();
                    }
                }
            }
        }
        return output;
    }
    public static List<string> GetLoacalIPMaybeVirtualNetwork()
    {
        List<string> ips = new List<string>();
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    ips.Add(ip.ToString());
                }
            }
        }
        catch (Exception)
        {
            if (ips.Count == 0)
                ips.Add("127.0.0.1");
        }
        return ips;
    }
    public static string IPV4()
    {
        string ipv4 = GetLocalIPv4(NetworkInterfaceType.Wireless80211);
        // 如果不是无线网卡，则获取有线网卡的地址
        if (ipv4 == "")
        {
            ipv4 = GetLocalIPv4(NetworkInterfaceType.Ethernet);
            // 如果有线网卡也没有获取到数据，则使用最开始可能包含虚拟网卡的方法来获取IP
            if (ipv4 == "")
            {
                ipv4 = GetLoacalIPMaybeVirtualNetwork().First();
            }
        }
        return ipv4;
    }

}


