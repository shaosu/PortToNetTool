using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PortToNet.ViewModels
{
    public class SendSettingControlViewModel : BindableBase
    {
        private bool _HexSend;
        public bool HexSend
        {
            get { return _HexSend; }
            set
            {
                SetProperty(ref _HexSend, value);
            }
        }


        private bool _EnableSendRegionLoop;
        public bool EnableSendRegionLoop
        {
            get { return _EnableSendRegionLoop; }
            set
            {
                SetProperty(ref _EnableSendRegionLoop, value);
                if (_EnableSendRegionLoop)
                {
                    EnableSlotRegionLoop = false;
                }
                LoopCount = 0;
            }
        }


        private bool _EnableSlotRegionLoop;
        public bool EnableSlotRegionLoop
        {
            get { return _EnableSlotRegionLoop; }
            set
            {
                SetProperty(ref _EnableSlotRegionLoop, value);
                if (_EnableSlotRegionLoop)
                {
                    EnableSendRegionLoop = false;
                }
                LoopCount = 0;
            }
        }


        private int _LoopCount;
        public int LoopCount
        {
            get { return _LoopCount; }
            set
            {
                SetProperty(ref _LoopCount, value);
            }
        }


        private int _LoopInterval;
        public int LoopInterval
        {
            get { return _LoopInterval; }
            set
            {
                int v = PubMod.ClampC(value, 100, 1000 * 3600);
                SetProperty(ref _LoopInterval, v);
            }
        }


        private string? _SloatNumerList;
        public string? SloatNumerList
        {
            get { return _SloatNumerList; }
            set
            {
                SetProperty(ref _SloatNumerList, value);
            }
        }

        public SendSettingControlViewModel()
        {
            _LoopInterval = 1000;
            _HexSend = true;


        }

        Regex rx = new Regex(@"\s*", RegexOptions.Compiled);
        private byte[] GetHexCurJsonComm_NoCheckFormat(string val)
        {
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
            return new byte[0];
        }

        public byte[] GetByte(string cmd)
        {
            if (_HexSend)
            {
                if (PubMod.CheckStringIsHexFormat(cmd))
                {
                    return GetHexCurJsonComm_NoCheckFormat(cmd);
                }
                else
                {
                    return new byte[0];
                }
            }
            else
            {
                return ASCIIEncoding.ASCII.GetBytes(cmd);
            }
        }
    }
}
