using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Core.Server
{
    public class ProxyServer
    {
        public Models.Config Config { get; set; }
        public NetConnectionManager connectionManager { get; set; }

        public void Init(Models.Config config)
        {
            Config = config;
            connectionManager = new NetConnectionManager();
            connectionManager.Init(config);
        }

        public void Start()
        {
            connectionManager.Start();
        }
        public void Stop()
        {
            
        }
    }
}
