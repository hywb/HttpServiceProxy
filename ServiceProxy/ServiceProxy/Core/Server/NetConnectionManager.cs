using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using ServiceProxy.Core.Limit;
using ServiceProxy.Common.Log;

namespace ServiceProxy.Core.Server
{
    public class NetConnectionManager
    {
        public Models.Config Config { get; set; }

        public ILimitingService limitingService { get; set; }

        public TcpListener tcpListener;
        private System.Collections.Concurrent.ConcurrentDictionary<string, bool> connection = new System.Collections.Concurrent.ConcurrentDictionary<string, bool>();

        public void Init(Models.Config config)
        {
            Config = config;
            limitingService = LimitingFactory.Build(Config.limit.LimitingType, Config.limit.MaxTPS,Config.limit.MaxServiceQum,Config.limit.MaxFreq);
        }

        public void Start()
        {
            StartListener();
            AcceptClient();
        }
        private void StartListener()
        {
            Console.WriteLine("Agent Start...");
            Console.WriteLine("Listen Address " + Config.proxy.listenaddress);
            Console.WriteLine("Listen Port " + Config.proxy.listenport);
            Console.WriteLine("Server Address:" + Config.proxy.serveraddress);
            Console.WriteLine("Server Port:" + Config.proxy.serverport);
            Console.WriteLine("MaxRequestQum:" + Config.limit.MaxServiceQum);
            Console.WriteLine("MaxTPS:" + Config.limit.MaxTPS);
            Console.WriteLine("NodeMaxFreq:" + Config.limit.MaxFreq);
            Console.WriteLine("------------------------------------------------------------------------");            
            var ip = IPAddress.Parse(Config.proxy.listenaddress);
            tcpListener = new TcpListener(ip, Config.proxy.listenport);
            tcpListener.Start();
            Console.WriteLine("Service preparation connection..");
        }

        private void AcceptClient()
        {
            int i = 0;
            while (true)
            {
                try
                {
                    i++;
                    TcpClient tc1 = tcpListener.AcceptTcpClient();//这里是等待数据再执行下边，不会100%占用cpu
                    tc1.SendTimeout = 300000;//设定超时，否则端口将一直被占用，即使失去连接
                    tc1.ReceiveTimeout = 300000;
                    var guid = Guid.NewGuid().ToString("N");
                    ConnectionHub obj1 = new ConnectionHub { ID = guid, Source = tc1};
                    //限流要做两件事
                    //限制当前同时连接数
                    //限制一个同一IP请求频率                    
                    //总体限流
                    string remotepoint = (tc1.Client.RemoteEndPoint as IPEndPoint).Address.ToString();
                    //Console.WriteLine("Client " + remotepoint);
                    var r01 = limitingService.Request(remotepoint);
                    if (i >= 1000)
                    {
                        LogManager.Instance.Log(ELogtype.Info, GetNodeNum(remotepoint));
                        LogManager.Instance.Log(ELogtype.Info, "剩余可用令牌:" + limitingService.GetAvailableTokenNum());
                        i = 0;
                    }                    
                    if (r01)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(transfer), obj1);                        
                    }
                    else
                    {
                        var E509 = remotepoint + " 限制访问,剩余可用令牌:" + limitingService.GetAvailableTokenNum();
                        SendMessage(tc1.GetStream(), Encoding.UTF8.GetBytes(E509));
                        LogManager.Instance.Log(ELogtype.Info, E509);
                        tc1.Close();
                    }
                    //Console.WriteLine("Client " + remotepoint + " Disconnect..");
                }
                catch (Exception e01)
                {
                    LogManager.Instance.Log(ELogtype.Error, "",e01);
                }
            }
        }
        private string GetNodeNum(string node)
        {
            var node1 = limitingService.Get1HNodeLimit(node);
            var node24 = limitingService.Get24HNodeLimit(node);
            StringBuilder sb01 = new StringBuilder();
            sb01.Append("Node:" + node + "\t");
            sb01.Append("1H:" + node1 == null ? "null" : node1.Count + "");
            sb01.Append("24H:" + node24 == null ? "null" : node24.Count + "");
            return sb01.ToString();
        }
        private void transfer(Object obj)
        {
            ConnectionHub connectionHub = obj as ConnectionHub;
            TcpClient tc1 = connectionHub.Source;
            var ns01 = tc1.GetStream();
            try
            {
                string clientmessage = "";
                byte[] bytes = ReadMessage(ref ns01);
                if (bytes == null || bytes.Length == 0)
                {
                    return;
                }
                ConnTargetServer(ns01,"", bytes);
            }
            catch (Exception e01)
            {                                
            }
            finally
            {
                tc1.Close();
                ns01.Close();
                //tc2.Close();
            }
        }

        private void ConnTargetServer(NetworkStream ns,string url,byte[] databuff)
        {
            //链接服务器
            IPAddress[] address = Dns.GetHostAddresses(Config.proxy.serveraddress);
            //解析出要访问的服务器地址
            IPEndPoint ipEndpoint = new IPEndPoint(address[0], Config.proxy.serverport);
            Socket IPsocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //创建连接Web服务器端的Socket对象
            IPsocket.Connect(ipEndpoint);
            IPsocket.ReceiveTimeout = 30000;
            IPsocket.SendTimeout = 30000;
            //Socket连Web接服务器
            IPsocket.Send(databuff, databuff.Length, 0);
            //代理访问软件对服务器端传送HTTP请求命令
            int packlength = 409600;
            byte[] RecvBytes = new byte[packlength];
            Int32 rBytes = packlength;// IPsocket.Receive(RecvBytes, RecvBytes.Length, 0);
            //代理访问软件接收来自Web服务器端的反馈信息
            //Console.WriteLine("接收字节数：" + rBytes.ToString());
            byte[] Rdatabuff = new byte[0];
            try
            {
                var SdataLength = 0;
                while (rBytes == packlength)
                {
                    rBytes = IPsocket.Receive(RecvBytes, RecvBytes.Length, 0);                    
                    var data01 = RecvBytes.Take(rBytes).ToArray();                    
                    //从内容从获取数据长度，如果当前获取长度与实际长度不一致，则继续获取
                    if (Rdatabuff.Length == 0)
                    {
                        SdataLength = GetPackLength(data01);
                    }
                    var RdataLength = rBytes - 104 + Rdatabuff.Length;
                    Rdatabuff = Rdatabuff.Concat(data01).ToArray();
                    if (SdataLength != 0 && SdataLength > RdataLength) {
                        rBytes = packlength;
                        continue;
                    }                    
                    if (rBytes >0)
                    {                        
                        SendMessage(ns, Rdatabuff.ToArray());
                        if (rBytes < packlength)
                        {
                            byte[] bytes = ReadMessage(ref ns);
                            //var debugtxt2 = Encoding.UTF8.GetString(bytes);
                            IPsocket.Send(bytes);
                        }
                        rBytes = packlength;
                        Rdatabuff = new byte[0];
                    }
                }
            }
            catch(Exception e01)
            {
            }
            IPsocket.Shutdown(SocketShutdown.Both);
            IPsocket.Close();
        }

        private int GetPackLength(byte[] datacontext)
        {
            if (datacontext == null)
            {
                datacontext = new byte[0];
            }
            var context = "";
            if (datacontext.Length < 200)
            {
                context = Encoding.UTF8.GetString(datacontext);
            }
            else
            {
                context = Encoding.UTF8.GetString(datacontext.Take(200).ToArray());
            }
            
            var items = context.Split('\r');
            int length = 0;
            if (items.Length >= 2 && items[1].Contains("Content-Length"))
            {
                
                var txt = items[1].Replace("Content-Length: ", "").Replace("\n","");
                int.TryParse(txt, out length);                
            }
            return length;
        }

        private void ProcessData(ref string context)
        {
            if (Config.logLevel == 1)
            {
                //Console.WriteLine(context);
            }
        }

        private string ConvertString(byte[] data)
        {
            StringBuilder sb01 = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] != 0x00)
                {
                    sb01.Append((char)data[i]);
                }
            }
            return sb01.ToString();
        }

        public bool IsOnline(TcpClient c)
        {
            return !((c.Client.Poll(1000, SelectMode.SelectRead) && (c.Client.Available == 0)) || !c.Client.Connected);
        }

        //接收客户端的HTTP请求数据
        private byte[] ReadMessage(ref NetworkStream ns)
        {
            byte[] bytebuff = new byte[4096];
            int rBytes = 4096;
            List<byte> Rdatabuff = new List<byte>();
            rBytes = ns.Read(bytebuff, 0, 4096);
            return bytebuff.Skip(0).Take(rBytes).ToArray();
            //Rdatabuff = Rdatabuff.Concat(bytebuff.Skip(0).Take(rBytes).ToArray()).ToList();
            //return Rdatabuff.ToArray();
        }
        //传送从Web服务器反馈的数据到客户端
        private void SendMessage(NetworkStream ns, byte[] databuff)
        {
            ns.Write(databuff, 0, databuff.Length);
        }
    }
}

