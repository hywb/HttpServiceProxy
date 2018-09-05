using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Common.Extrend
{
    public static class TypeExtend
    {
        public static bool IsNullOrEmpty(this string context)
        {
            return string.IsNullOrEmpty(context);
        }
    }
}
