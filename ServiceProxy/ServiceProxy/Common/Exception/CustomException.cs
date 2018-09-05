using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Common.Exception
{
    public class CustomException : System.Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}
