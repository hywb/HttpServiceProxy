using ServiceProxy.Core.Limit;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Core.Models
{
    public class Config
    {
        public Redisconfig redis { get; set; } = new Redisconfig();
        public Proxyconfig proxy { get; set; } = new Proxyconfig();
        public LimintConfig limit { get; set; } = new LimintConfig();
        /// <summary>
        /// 1:all,2.normail,3.main,4.none
        /// </summary>
        /// <value></value>
        public int logLevel { get; set; } = 1;
    }
    public class Redisconfig
    {
        public string server { get; set; } = "server";
        public string auth { get; set; } = "auth";
        public int database { get; set; } = 0;
    }
    public class Proxyconfig
    {
        public string listenaddress { get; set; } = "127.0.0.1";
        public int listenport { get; set; } = 1234;
        public string serveraddress { get; set; } = "baidu.com";
        public int serverport { get; set; } = 80;
    }
    public class LimintConfig
    {
        public LimitingType LimitingType { get; set; } = LimitingType.TokenBucket;
        /// <summary>
        /// 每秒处理能力
        /// </summary>
        public int MaxTPS { get; set; } = 500;
        /// <summary>
        /// 最大同时服务数
        /// </summary>
        public int MaxServiceQum { get; set; } = 1000;
        /// <summary>
        /// 单节点每分钟最大访问频率
        /// </summary>
        public int MaxFreq { get; set; } = 100;
    }
}
