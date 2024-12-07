using HandyControl.Tools;
using PortToNet.Ecan;
using PortToNet.Model;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace PortToNet.ViewModels
{
    public class RecvSettingControlViewModel : BindableBase
    {
        private bool _HexRecv;
        public bool HexRecv
        {
            get { return _HexRecv; }
            set
            {
                SetProperty(ref _HexRecv, value);
            }
        }

        private bool _PauseShow;
        [JsonIgnore]
        public bool PauseShow
        {
            get { return _PauseShow; }
            set
            {
                SetProperty(ref _PauseShow, value);
            }
        }

        private bool _TrimEndBlackAndNewLine;
        public bool TrimEndBlackAndNewLine
        {
            get { return _TrimEndBlackAndNewLine; }
            set
            {
                SetProperty(ref _TrimEndBlackAndNewLine, value);
            }
        }

        private bool _ShowSendData;
        public bool ShowSendData
        {
            get { return _ShowSendData; }
            set
            {
                SetProperty(ref _ShowSendData, value);
            }
        }

        private bool _AutoScrollToEnd;
        public bool AutoScrollToEnd
        {
            get { return _AutoScrollToEnd; }
            set
            {
                SetProperty(ref _AutoScrollToEnd, value);
            }
        }

        private bool _ShowFrameLength;
        public bool ShowFrameLength
        {
            get { return _ShowFrameLength; }
            set
            {
                SetProperty(ref _ShowFrameLength, value);
            }
        }

        private bool _EnableLog;
        [JsonIgnore]
        public bool EnableLog
        {
            get { return _EnableLog; }
            set
            {
                bool ch = SetProperty(ref _EnableLog, value);
                if (ch && _EnableLog)
                {
                    _EnableLog = SelectLogFile();
                }

                if (_EnableLog == false)
                {
                    if (LogStream != null)
                    {
                        LogStream.Close();
                        LogStream.Dispose();
                    }
                }
            }
        }
        private System.IO.FileStream LogStream;
        private bool SelectLogFile()
        {
            Microsoft.Win32.OpenFileDialog op = new Microsoft.Win32.OpenFileDialog();
            bool? ok = op.ShowDialog();
            if (ok == true)
            {
                LogFileName = op.FileName;
                if (LogStream != null)
                {
                    LogStream.Close();
                    LogStream.Dispose();
                }
                LogStream = System.IO.File.Open(LogFileName, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            }
            else
            {
                ok = false;
            }
            return ok.Value;
        }

        private string? _LogFileName;
        [JsonIgnore]
        public string? LogFileName
        {
            get { return _LogFileName; }
            set
            {
                SetProperty(ref _LogFileName, value);
            }
        }

        private ObservableCollection<ShowTimeFormatDescription> _ShowTimeFormatList;
        [JsonIgnore]
        public ObservableCollection<ShowTimeFormatDescription> ShowTimeFormatList
        {
            get { return _ShowTimeFormatList; }
            set
            {
                SetProperty(ref _ShowTimeFormatList, value);
            }
        }

        private ShowTimeFormatDescription? _CurShowTimeFormat;
        public ShowTimeFormatDescription? CurShowTimeFormat
        {
            get { return _CurShowTimeFormat; }
            set
            {
                SetProperty(ref _CurShowTimeFormat, value);
            }
        }

        private string? _RegexFormat;
        public string? RegexFormat
        {
            get { return _RegexFormat; }
            set
            {
                SetProperty(ref _RegexFormat, value);
            }
        }

        private int _CurShowLineCount = 0;
        private int _LimitShowLineCount;
        public int LimitShowLineCount
        {
            get { return _LimitShowLineCount; }
            set
            {
                int c = PubMod.ClampC(value, 100, 100 * 10000);
                SetProperty(ref _LimitShowLineCount, c);
            }
        }

        public RecvSettingControlViewModel()
        {
            _ShowTimeFormatList = new ObservableCollection<ShowTimeFormatDescription>();
            _ShowTimeFormatList.AddRange(ShowTimeFormatDescription.BuildWorkMode(App.UIL10N));
            _CurShowTimeFormat = _ShowTimeFormatList[1];
            _LimitShowLineCount = 10000;
            _AutoScrollToEnd = true;
            _HexRecv = true;
            _ShowSendData = true;
        }

        public void LanguageChanged_Sub()
        {
            CurShowTimeFormat = ShowTimeFormatDescription.OnLanguageChanged(CurShowTimeFormat, ShowTimeFormatList, App.UIL10N);
        }

        public string GetFormatString(byte[] data)
        {
            if (_PauseShow)
                return string.Empty;

            if (_HexRecv)
            {
                return PubMod.ArrayToHexString(data);
            }
            else
            {
                string rt = ASCIIEncoding.ASCII.GetString(data, 0, data.Length);
                if (string.IsNullOrWhiteSpace(_RegexFormat) == false)
                {
                    Regex reg = new Regex(_RegexFormat);
                    if (reg.IsMatch(rt) == false)
                    {
                        return string.Empty;
                    }
                }

                if (_TrimEndBlackAndNewLine)
                {
                    rt = rt.TrimEnd();
                    rt = rt.TrimEnd((char[])"/n/r".ToCharArray());
                }
                return rt;
            }
        }

        public string GetFormatString(CAN_OBJ can)
        {
            if (_PauseShow)
                return string.Empty;

            if (can.RemoteFlag > 0)
            {
                string ext = can.ExternFlag > 0 ? "Extern" : "Standard";
                string rtre = $"ID:0x{can.ID:X4} DLC:{can.DataLen} -Remote -{ext}";
                return rtre;
            }

            if (_HexRecv)
            {

                var datastr = PubMod.ArrayToHexString(can.data, can.DataLen);
                string rt;
                if (can.ExternFlag > 0)
                {
                    rt = $"ID:0x{can.ID:X4} Data:{datastr} DLC:{can.DataLen}  -Extern";
                }
                else
                {
                    rt = $"ID:0x{can.ID:X4} Data:{datastr} DLC:{can.DataLen}  -Standard";
                }
                return rt;
            }
            else
            {
                int min = Math.Min(can.DataLen, can.data.Length);
                string datastr = ASCIIEncoding.ASCII.GetString(can.data, 0, min);
                if (string.IsNullOrWhiteSpace(_RegexFormat) == false)
                {
                    Regex reg = new Regex(_RegexFormat);
                    if (reg.IsMatch(datastr) == false)
                    {
                        return string.Empty;
                    }
                }

                if (_TrimEndBlackAndNewLine)
                {
                    datastr = datastr.TrimEnd();
                    datastr = datastr.TrimEnd((char[])"/n/r".ToCharArray());
                }
                string rt;
                if (can.ExternFlag > 0)
                {
                    rt = $"ID:0x{can.ID:X4} {datastr} DLC:{can.DataLen}  -Extern";
                }
                else
                {
                    rt = $"ID:0x{can.ID:X4} {datastr} DLC:{can.DataLen}  -Standard";
                }
                return rt;
            }
        }

        /// <summary>
        /// 数据添加到Document
        /// <para>会自动添加格式信息:如时间,长度等</para>
        /// </summary>
        /// <param name="str">为添加其他信息的数据</param>
        /// <param name="recv">接收还是发送</param>
        public void AppendToFlowDocument(string formatStr, int strOrginByteCount, bool recv)
        {
            if (_PauseShow)
                return;

            if (string.IsNullOrEmpty(formatStr)) return;
            if (recv == false && _ShowSendData == false) return;
            System.Windows.Media.Color color = Colors.Red;
            if (recv == false) color = Colors.Green;

            App.UIDispatcherDoAction(() =>
            {
                var gsb = BuildParagraph(color, formatStr, strOrginByteCount);
                if (gsb.g == null) return;

                if (_EnableLog && LogStream != null && LogStream.CanWrite)
                {
                    string dd = gsb.sb.ToString();

                    Task.Run(async () =>
                    {
                        byte[] lb = ASCIIEncoding.ASCII.GetBytes(dd);
                        await LogStream.WriteAsync(lb);
                        await LogStream.FlushAsync();
                    });
                }

                _CurShowLineCount++;
                if (_CurShowLineCount >= _LimitShowLineCount)
                {
                    FlowDocumentClear();
                }
                else
                {
                    LogFlowDocument.Blocks.Add(gsb.g);
                    if (_AutoScrollToEnd)
                    {
                        rich.ScrollToEnd();  //滚动到控件光标处  
                    }
                }
            });
        }

        private (Paragraph g, StringBuilder sb) BuildParagraph(System.Windows.Media.Color color, string formatStr, int strOrginByteCount)
        {
            var g = new Paragraph();
            g.LineHeight = 0.01;

            StringBuilder sb = new StringBuilder();
            sb.Append(_CurShowTimeFormat.GetTimeString(_CurShowTimeFormat.Mode.Value, " "));

            if (_ShowFrameLength && strOrginByteCount >= 1)
            {
                sb.Append($"[{strOrginByteCount}]:");
            }
            else
            {
                if (sb.Length >= 1)
                    sb.Append(":");
            }
            if (sb.Length >= 1)
            {
                Run r1 = new Run(sb.ToString());
                r1.Foreground = new SolidColorBrush(Colors.DarkMagenta);
                g.Inlines.Add(r1);
            }
            sb.Append(formatStr);
            sb.Append(Environment.NewLine);
            var r2 = new Run(formatStr);
            r2.Foreground = new SolidColorBrush(color);
            g.Inlines.Add(r2);
            return (g, sb);
        }

        /// <summary>
        /// 故障信息添加到Document
        /// </summary>
        /// <param name="str"></param>
        public void AppendErrorToFlowDocument(string str)
        {
            AppendToFlowDocument(str, -1, true);
        }

        RichTextBox rich;

        private FlowDocument LogFlowDocument;
        public void BindRichTextBox(RichTextBox txt)
        {
            rich = txt;
            LogFlowDocument = rich.Document;
            LogFlowDocument.LineHeight = 0.01;
            LogFlowDocument.Blocks.Clear();
        }
        public void FlowDocumentClear()
        {
            LogFlowDocument.Blocks.Clear();
            _CurShowLineCount = 0;
        }
    }
}
