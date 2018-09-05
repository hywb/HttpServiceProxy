using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ServiceProxy.Core.Server
{
    public class ConnectionHub
    {
        public string ClientIP { get; set; }
        public TcpClient Source { get; set; }
        public TcpClient Target { get; set; }
        public string ID { get; set; }
    }
}
