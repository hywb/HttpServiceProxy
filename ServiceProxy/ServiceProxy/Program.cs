using System;

namespace ServiceProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            AppManager app = new AppManager();
            app.Init(args);
            app.Start();
        }
    }
}
