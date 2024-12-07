using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortToNet.ViewModels
{
    public class CanSendSettingControlViewModel : Prism.Mvvm.BindableBase
    {
        private uint _CanID;
        public uint CanID
        {
            get
            {
                return _CanID;
            }
            set
            {

                if (_ExternFlag)
                {
                    value = value & 0x1FFF_FFFF;
                    SetProperty(ref _CanID, value);
                }
                else
                {
                    value = value & 0x7FF;
                    SetProperty(ref _CanID, value);
                }
            }
        }
        public string CanIDStr
        {
            get { return _CanID.ToString("X"); }
            set
            {
                uint tmp;
                bool ok = uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out tmp);
                if (ok)
                {
                    CanID = tmp;
                    RaisePropertyChanged(nameof(CanID));
                    RaisePropertyChanged(nameof(CanIDStr));
                }
            }
        }

        private bool _ExternFlag;
        public bool ExternFlag
        {
            get { return _ExternFlag; }
            set
            {
                SetProperty(ref _ExternFlag, value);
            }
        }

        private bool _RemoteFlag;
        public bool RemoteFlag
        {
            get { return _RemoteFlag; }
            set
            {
                SetProperty(ref _RemoteFlag, value);
            }
        }

    }
}

