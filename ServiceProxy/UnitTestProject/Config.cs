using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTestProject
{
    [TestClass]
    public class Config
    {
        [TestMethod]
        public void TestConfigSave()
        {
            ServiceProxy.Core.Models.Config config = new ServiceProxy.Core.Models.Config();
            var txt = Newtonsoft.Json.JsonConvert.SerializeObject(config);
            var path = System.AppDomain.CurrentDomain.BaseDirectory + "\\config.json";
            File.WriteAllText(path, txt);
        }
    }
}
