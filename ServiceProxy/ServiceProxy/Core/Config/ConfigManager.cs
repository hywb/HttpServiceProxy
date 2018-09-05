using ServiceProxy.Common;
using ServiceProxy.Common.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Core.Config
{
    public class ConfigManager
    {

        public Models.Config ConfigData { get; set; }

        public void Init(string configname = "config.json")
        {
            LogManager.Instance.Log(ELogtype.Info, "Config Init :" + configname);
            var context = System.IO.File.ReadAllText(configname);
            ConfigData = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Config>(context);
        }
    }
}
