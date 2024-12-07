using PortToNet.Model;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;

namespace PortToNet.ViewModels
{
    public class NetPortSettingControlViewModel : PortSettingControlViewModelBase
    {
        public override event Action<string>? ErrorEvent;

        public override event Action<byte[]>? RecvDataEvent;

        TcpListener? listener;
        TcpClient? tcp;
        Socket localUdpSocket;
        Socket udpsocketRemote;
        EndPoint remoteEP;
        EndPoint localEP;
        EndPoint clientEP; // 只支持一个UDP客户端

        #region " 网口相关设置  "

        private ObservableCollection<NetProtocolTypeDescription> _ProtocolList;
        [JsonIgnore]
        public ObservableCollection<NetProtocolTypeDescription> ProtocolList
        {
            get { return _ProtocolList; }
            set
            {
                SetProperty(ref _ProtocolList, value);
            }
        }


        private NetProtocolTypeDescription _Protocol;
        public NetProtocolTypeDescription Protocol
        {
            get { return _Protocol; }
            set
            {
                bool ch = SetProperty(ref _Protocol, value);
                if (ch)
                {
                    UpdateString();
                }
            }
        }


        private string? _IPAddress;
        public string? IPAddress
        {
            get { return _IPAddress; }
            set
            {
                SetProperty(ref _IPAddress, value);
            }
        }


        private string? _IPAddressStr;
        /// <summary>
        /// <para>UDP和TCP Client: RemoteIPAddress</para> 
        /// <para>TCP Server: LocalIPAddress</para> 
        /// </summary>
        [JsonIgnore]
        public string? IPAddressStr
        {
            get { return _IPAddressStr; }
            set
            {
                SetProperty(ref _IPAddressStr, value);
            }
        }

        private int _Port;
        public int Port
        {
            get { return _Port; }
            set
            {
                SetProperty(ref _Port, value);
            }
        }


        private string _LocalIPAddressStr;
        public string LocalIPAddressStr
        {
            get { return _LocalIPAddressStr; }
            set
            {
                SetProperty(ref _LocalIPAddressStr, value);
            }
        }


        private ObservableCollection<string> _LocalIPAddressList;
        [JsonIgnore]
        public ObservableCollection<string> LocalIPAddressList
        {
            get { return _LocalIPAddressList; }
            set
            {
                SetProperty(ref _LocalIPAddressList, value);
            }
        }


        private int _UdpLocalPort;
        public int UdpLocalPort
        {
            get { return _UdpLocalPort; }
            set
            {
                SetProperty(ref _UdpLocalPort, value);
            }
        }

        private string? _ConnectStr;
        /// <summary>
        /// <para>UDP和TCP Client: Connect</para> 
        /// <para>TCP Or UDPClient :Connecting</para>
        /// <para>TCP Server: StartListion</para> 
        /// </summary>
        [JsonIgnore]
        public string? ConnectStr
        {
            get { return _ConnectStr; }
            set
            {
                SetProperty(ref _ConnectStr, value);
            }
        }

        #endregion

        public NetPortSettingControlViewModel() : base()
        {
            _ProtocolList = new ObservableCollection<NetProtocolTypeDescription>();
            _ProtocolList.AddRange(NetProtocolTypeDescription.BuildWorkMode(App.UIL10N).ToArray());
            _Protocol = _ProtocolList.First();
            _LocalIPAddressList = new ObservableCollection<string>();
            _LocalIPAddressList.AddRange(PubMod.GetLoacalIPMaybeVirtualNetwork());
            UpdateString();
            _Port = 15000;
            _UdpLocalPort = _Port + 1;
        }

        public void LanguageChanged_Sub()
        {
            UpdateString();
        }

        protected override void UpdateString()
        {
            if (App.UIL10N == null) return;
            var type = _Protocol.Mode;
            switch (type)
            {
                case NetProtocolType.TCP_Client:
                case NetProtocolType.UDP:
                    ConnectStr = (string)App.UIL10N["Connect"] ?? "Connect";
                    IPAddressStr = (string)App.UIL10N["RemoteIPAddress"] ?? "Remote IP Address";
                    break;
                case NetProtocolType.UDP_Server:
                case NetProtocolType.TCP_Server:
                    ConnectStr = (string)App.UIL10N["StartListion"] ?? "Start Listion";
                    IPAddressStr = (string)App.UIL10N["LocalIPAddress"] ?? "Local IP Address";
                    break;
                default:
                    break;
            }
        }

        public async Task<bool> OpenTcpClientAsync()
        {
            bool ok = false;
            tcp = new TcpClient();
            await tcp.ConnectAsync(System.Net.IPAddress.Parse(_IPAddress), _Port);
            ok = tcp.Connected;
            if (ok)
            {
                Task.Factory.StartNew(() => { StartRecvTcpClinetData(tcp.Client, "Tcp"); });
            }
            return ok;
        }
        protected override async Task<bool> Open()
        {
            OpenedCB?.Invoke(false);
            ErrorEvent?.Invoke(App.GetLanguage("Connecting..."));
            bool ok = false;
            toListion = false;
            try
            {
                switch (_Protocol.Mode.Value)
                {
                    case NetProtocolType.TCP_Client:
                        ok = await OpenTcpClientAsync();
                        break;
                    case NetProtocolType.UDP:
                        localUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpsocketRemote = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        localEP = new IPEndPoint(System.Net.IPAddress.Parse(LocalIPAddressStr), UdpLocalPort);
                        remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), _Port);
                        localUdpSocket.Connect(remoteEP);
                        ok = true;
                        Task.Run(() => { StartRecvUdpClinetData(localUdpSocket, "UDP"); });
                        break;
                    case NetProtocolType.UDP_Server:
                        localUdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        udpsocketRemote = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        localEP = new IPEndPoint(System.Net.IPAddress.Parse(LocalIPAddressStr), UdpLocalPort);
                        remoteEP = new IPEndPoint(System.Net.IPAddress.Parse(IPAddress), _Port);

                        //LingerOption lo = new LingerOption(true, 1);  
                        //localUdpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lo);  // 不支持
                        localUdpSocket.Bind(localEP);
                        clientEP = new IPEndPoint(System.Net.IPAddress.Any, 0);
                        toListion = ok = true;
                        Task.Run(() => { StartRecvUdpServerData(localUdpSocket, "UDP_Server"); });
                        Task.Run(HeartBeatLoop);
                        break;
                    case NetProtocolType.TCP_Server:
                        TcpServerOfClients = new Dictionary<EndPoint, Socket>();
                        TcpServerOfClientStr.Clear();
                        TcpServerOfClientStr.Add(SelectedALL);
                        TcpServerSelectedItem = SelectedALL;
                        listener = new TcpListener(System.Net.IPAddress.Any, _Port);

                        LingerOption lo2 = new LingerOption(true, 1);
                        listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lo2);
                        listener.Start();
                        toListion = ok = true;
                        Task.Run(TcpListenerLoop);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex.Message);
            }
            if (ok)
                ErrorEvent?.Invoke(App.GetLanguage("ConnectionSuccessful"));
            else
                ErrorEvent?.Invoke(App.GetLanguage("ConnectionException"));
            return ok;
        }


        private CancellationTokenSource TcpServerCTS;
        private async void TcpListenerLoop()
        {
            if (listener == null) return;
            TcpServerCTS = new CancellationTokenSource();


            while (toListion)
            {
                try
                {
                    var client = await listener.AcceptTcpClientAsync();

                    if (TcpServerOfClients.ContainsKey(client.Client.RemoteEndPoint) == false)
                    {
                        TcpServerOfClients.Add(client.Client.RemoteEndPoint, client.Client);
                        UIListAdd(client.Client.RemoteEndPoint);
                        Task.Run(() => { TcpServer_StartRecvTcpClinetData(client.Client, "TCP"); }, TcpServerCTS.Token);
                    }
                    else
                    {
                        Task.Run(() => { TcpServer_StartRecvTcpClinetData(client.Client, "TCP"); }, TcpServerCTS.Token);
                    }
                }
                catch (Exception ex)
                {
                    if (toListion == true)
                        ErrorEvent?.Invoke(ex.Message);
                }
            }
            TcpServerCTS.Cancel();
            listener?.Stop();
            //listener?.Dispose();
            listener = null;
            TcpServerOfClients.Clear();
            IsOpened = false;
        }
        private void StartRecvUdpClinetData(Socket loclient, string title)
        {
            byte[] buff = new byte[4096];
            ArraySegment<byte> buffer = new ArraySegment<byte>(buff);

            localUdpSocket.SendTo(PackCanFrame.PackHeart(), remoteEP);
            while (true)
            {
                try
                {
                    int c = 0;
                    c = loclient.ReceiveFrom(buffer, SocketFlags.None, ref remoteEP);
                    this.RecvDataEvent?.Invoke(buffer.Slice(0, c).ToArray());
                }
                catch (Exception ex)
                {
                    if (_IsOpened)
                    {
                        ErrorEvent?.Invoke($"{title}{App.GetLanguage("ClientReceiveException")}{Environment.NewLine}{ex.Message}");
                    }
                    break;
                }
            }

            loclient.Close();
            loclient.Dispose();
            IsOpened = false;
        }

        private bool CheckIPEPortIsZero(EndPoint clientEP)
        {
            var ipe = clientEP as IPEndPoint;
            if (ipe == null)
                return true;
            if (ipe.Port == 0)
            {
                return true;
            }
            return false;
        }
        private void StartRecvUdpServerData(Socket loclient, string title)
        {
            byte[] buff = new byte[4096];
            ArraySegment<byte> buffer = new ArraySegment<byte>(buff);
            bool FirstRecvUdpData = true;

            while (toListion)
            {
                try
                {
                    int c = 0;
                    c = loclient.ReceiveFrom(buffer, SocketFlags.None, ref clientEP);
                    if (FirstRecvUdpData) // 握手
                    {
                        FirstRecvUdpData = false;
                        this.RecvDataEvent?.Invoke(buffer.Slice(0, c).ToArray());
                        c = 0;
                        ErrorEvent?.Invoke($"{clientEP}{App.GetLanguage("ClientConnectionStartsToReceive")}");
                    }
                    IPEndPoint ipe = clientEP as IPEndPoint;
                    if (ipe != null && ipe.Port != 0)
                    {
                        Port = ipe.Port;
                    }

                    if (c > 0)
                    {
                        this.RecvDataEvent?.Invoke(buffer.Slice(0, c).ToArray());
                        CheckHeartBeat(DateTime.Now, true);
                    }
                }
                catch (Exception sk)
                {
                    if (_IsOpened)
                    {
                        IPEndPoint? ipe = clientEP as IPEndPoint;
                        ErrorEvent?.Invoke($"{title}{App.GetLanguage("ReceiveException")}{Environment.NewLine}{sk.Message} Port:{ipe?.Port}");
                        if (ipe != null)
                        {
                            ipe.Port = 0;
                            FirstRecvUdpData = true;
                        }
                    }
                }
            }

            loclient.Close();
            loclient.Dispose();
            IsOpened = false;


        }

        private void TcpServer_StartRecvTcpClinetData(Socket client, string title)
        {
            byte[] buff = new byte[4096];
            ArraySegment<byte> buffer = new ArraySegment<byte>(buff);
            string? RemoteEndPoint = client.RemoteEndPoint.ToString();
            while (true)
            {
                if (TcpServerCTS.Token.IsCancellationRequested)
                {
                    break;
                }
                try
                {
                    if (client.Connected == false) break;

                    int c = client.Receive(buffer, SocketFlags.None);
                    if (c <= 0)
                    {
                        ErrorEvent?.Invoke($"{title}{App.GetLanguage("Client")}{RemoteEndPoint}{App.GetLanguage("ReceiveException")}{Environment.NewLine}");
                        break;
                    }
                    if (c > 0)
                    {
                        this.RecvDataEvent?.Invoke(buffer.Slice(0, c).ToArray());
                    }
                    if (client.RemoteEndPoint != null && TcpServerOfClients.ContainsKey(client.RemoteEndPoint) == false)
                    {
                        TcpServerOfClients.Add(client.RemoteEndPoint, client);
                        UIListAdd(client.RemoteEndPoint);
                    }
                }
                catch (Exception ex)
                {
                    if (_IsOpened)
                    {
                        ErrorEvent?.Invoke($"{title}{App.GetLanguage("Client")}{RemoteEndPoint}{App.GetLanguage("ReceiveException")}{Environment.NewLine}{ex.Message}");
                    }

                    if (RemoteEndPoint != null && client != null)
                        UIListRemove(client.RemoteEndPoint);
                    break;
                }
            }

            UIListRemove(client.RemoteEndPoint);
            client.Close();
            client.Dispose();

        }


        private void StartRecvTcpClinetData(Socket client, string title)
        {
            byte[] buff = new byte[4096];
            ArraySegment<byte> buffer = new ArraySegment<byte>(buff);
            while (true)
            {
                string? RemoteEndPoint = client.RemoteEndPoint.ToString();
                try
                {
                    if (client.Connected == false) break;
                    int c = client.Receive(buffer, SocketFlags.None);
                    if (c > 0)
                    {
                        this.RecvDataEvent?.Invoke(buffer.Slice(0, c).ToArray());
                    }
                }
                catch (Exception ex)
                {
                    if (_IsOpened)
                    {
                        ErrorEvent?.Invoke($"{title}{App.GetLanguage("Client")}{RemoteEndPoint}{App.GetLanguage("ReceiveException")}{Environment.NewLine}{ex.Message}");
                    }
                    break;
                }
            }

            client.Close();
            client.Dispose();
            IsOpened = false;
        }

        public override Task<bool> Close()
        {
            bool tmp_IsOpening = false;
            toListion = false;
            try
            {
                switch (_Protocol.Mode.Value)
                {
                    case NetProtocolType.TCP_Client:
                        tcp?.Close();
                        tcp?.Dispose();
                        break;
                    case NetProtocolType.UDP:
                    case NetProtocolType.UDP_Server:
                        localUdpSocket?.Close();
                        localUdpSocket?.Dispose();
                        break;
                    case NetProtocolType.TCP_Server:
                        listener?.Stop();
                        listener = null;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex.Message);
            }
            TcpServerOfClientStr?.Clear();
            IsOpened = false;
            RaisePropertyChanged(nameof(IsOpened));
            return Task.FromResult(tmp_IsOpening);
        }

        private object? _TcpServerSelectedItem;
        [JsonIgnore]
        public object? TcpServerSelectedItem
        {
            get { return _TcpServerSelectedItem; }
            set
            {
                bool ch = SetProperty(ref _TcpServerSelectedItem, value);
                if (ch)
                {
                    if (_TcpServerSelectedItem is string)
                    {
                        TcpServerSelectedALL = true;
                    }
                    else
                    {
                        TcpServerSelectedALL = false;
                    }
                }
            }
        }
        bool TcpServerSelectedALL = true;
        private const string SelectedALL = "ALL";
        private ObservableCollection<object> _TcpServerOfClientStr = new();
        [JsonIgnore]
        public ObservableCollection<object> TcpServerOfClientStr
        {
            get { return _TcpServerOfClientStr; }
            set
            {
                SetProperty(ref _TcpServerOfClientStr, value);
            }
        }
        private void UIListAdd(EndPoint socket)
        {
            App.UIDispatcherDoAction(() => { TcpServerOfClientStr.Add(socket); });
        }
        private void UIListRemove(EndPoint socket)
        {
            App.UIDispatcherDoAction(() =>
            {
                if (_TcpServerSelectedItem == socket)
                {
                    TcpServerSelectedItem = SelectedALL;
                }
                TcpServerOfClientStr.Remove(socket);
            });
        }

        [NotNull]
        public Dictionary<EndPoint, Socket>? TcpServerOfClients { get; set; }

        private int TcpServerPortWrite(byte[] data)
        {
            List<EndPoint> dels = new List<EndPoint>();
            int len = 0;
            if (TcpServerSelectedALL)
            {
                bool ok = false;
                foreach (var item in TcpServerOfClients)
                {
                    try
                    {
                        ok |= data.Length == item.Value.Send(data);
                    }
                    catch (Exception)
                    {
                        dels.Add(item.Key);
                    }
                }
                if (ok)
                    len = data.Length;
                else
                    len = int.MinValue;
            }
            else
            {
                EndPoint point = null;
                try
                {
                    point = (EndPoint)_TcpServerSelectedItem;
                    Socket item = null;
                    if (TcpServerOfClients.TryGetValue(point, out item))
                    {
                        len = item.Send(data);
                    }
                }
                catch (Exception)
                {
                    if (point != null)
                        dels.Add(point);
                }
            }


            foreach (var del in dels)
            {
                TcpServerOfClients.Remove(del);
                UIListRemove(del);
            }
            return len;
        }

        #region  UDP_Server 心跳包

        DateTime TcpClientlastSend = DateTime.Now;
        public const int HeartTime = 3000;
        private void CheckHeartBeat(DateTime cur, bool ok)
        {
            if (ok)
            {
                TcpClientlastSend = cur;
            }
            else
            {
                PortWriteHeart();
                TcpClientlastSend = cur;
            }
        }
        private void PortWriteHeart()
        {
            if ((DateTime.Now - TcpClientlastSend).TotalMilliseconds > HeartTime)
            {
                PortSendDataMsg msg = new PortSendDataMsg();
                msg.Data = PackCanFrame.PackHeart();
                PortWrite(msg);
                ErrorEvent?.Invoke($"Send Heart");
            }
        }
        private void HeartBeatLoop()
        {
            TcpClientlastSend = DateTime.Now;
            while (toListion)
            {
                PortWriteHeart();
                Thread.Sleep(HeartTime);
            }
        }

        #endregion
        protected override bool PortWrite(PortSendDataMsg msg)
        {
            byte[] data = msg.BuildNetData();

            int len = 0;
            bool skipErrorEvent = false;
            switch (_Protocol.Mode.Value)
            {
                case NetProtocolType.TCP_Client:
                    len = tcp.Client.Send(data);

                    break;
                case NetProtocolType.UDP:
                    len = localUdpSocket.SendTo(data, remoteEP);
                    break;
                case NetProtocolType.UDP_Server:
                    var ipe = clientEP as IPEndPoint;
                    if (ipe != null && ipe.Port == 0)
                    {
                        ErrorEvent?.Invoke(App.GetLanguage("NoClient"));
                        skipErrorEvent = true;
                    }
                    else
                    {
                        len = localUdpSocket.SendTo(data, clientEP);
                        CheckHeartBeat(DateTime.Now, len == data.Length);
                    }
                    break;
                case NetProtocolType.TCP_Server:
                    len = TcpServerPortWrite(data);
                    break;
                default:
                    break;
            }
            if (_Protocol.Mode.Value == NetProtocolType.TCP_Server)
            {
                if (len == int.MinValue)
                {
                    if (len != data.Length && skipErrorEvent == false)
                        ErrorEvent?.Invoke(App.GetLanguage("SendFailedNoClient"));
                }
                else
                {
                    if (len != data.Length && skipErrorEvent == false)
                        ErrorEvent?.Invoke($"{App.GetLanguage("SendFailedNoClient")}{PubMod.ArrayToHexString(data)}");
                }
            }
            else
            {
                if (len != data.Length && skipErrorEvent == false)
                    ErrorEvent?.Invoke($"{App.GetLanguage("SendFailedNoClient")}{PubMod.ArrayToHexString(data)}");
            }

            return len == data.Length;
        }


    }
}
