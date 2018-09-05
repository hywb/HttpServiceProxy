using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServiceProxy.Common.Log
{
    public class LogManager
    {
        private static LogManager _instance;
        public static LogManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogManager();
                    _instance.path = AppDomain.CurrentDomain.BaseDirectory + "\\run.log";
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        private string path = "";

        public void Log(ELogtype type, string context, System.Exception ex = null)
        {
            string msg = DateTime.Now.ToString() + "\t" + type.ToString() + ":" + context +"\r\n";
            File.AppendAllText(path, msg);
            System.Console.WriteLine(msg);
            if (ex != null && ex.InnerException != null)
            {
                File.AppendAllText(path, ex.InnerException.Message);
                System.Console.WriteLine(ex.InnerException.Message);
            }
        }
    }

    public enum ELogtype
    {
        Info,
        Error

    }
}
