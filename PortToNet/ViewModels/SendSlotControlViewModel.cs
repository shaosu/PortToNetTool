using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortToNet.ViewModels
{
    public class SlotSendCommandArg
    {
        public int SlotIndex { get; set; }
        public string? Data { get; set; }
        public SlotSendCommandArg(int index,string? data)
        {
            SlotIndex = index;
            Data = data;
        }
    }

    public class SendSlotControlViewModel : BindableBase
    {
        private int _SlotIndex;
        public int SlotIndex
        {
            get { return _SlotIndex; }
            set
            {
                SetProperty(ref _SlotIndex, value);
            }
        }

        private string? _Data;
        public string? Data
        {
            get { return _Data; }
            set
            {
                SetProperty(ref _Data, value);
            }
        }

        [JsonIgnore]
        public DelegateCommand<SlotSendCommandArg>? SendCommand { get; set; }
        [JsonIgnore]
        public DelegateCommand InsideSendCommand => new DelegateCommand(InsideSendCommand_Sub);
        private void InsideSendCommand_Sub()
        {
            SendCommand?.Execute(new SlotSendCommandArg(_SlotIndex, _Data));
        }

    }
}
