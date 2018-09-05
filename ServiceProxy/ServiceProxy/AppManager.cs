using ServiceProxy.Core.Config;
using ServiceProxy.Core.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy
{
    public class AppManager
    {
        private ProxyServer proxyServer;
        private ConfigManager configManager;
        public void Init(string[] args)
        {
            configManager = new ConfigManager();
            if (args != null && args.Length > 0)
            {
                configManager.Init(args[0]);
            }
            else
            {
                configManager.Init();
            }
            proxyServer = new ProxyServer();
            proxyServer.Init(configManager.ConfigData);
        }
        public void Start()
        {
            proxyServer.Start();
            RunLoop();
        }
        private void RunLoop()
        {
            while(true)
            {
                Console.ReadLine();
            }
        }
        public void Stop()
        {
            proxyServer.Stop();
        }
    }
}
