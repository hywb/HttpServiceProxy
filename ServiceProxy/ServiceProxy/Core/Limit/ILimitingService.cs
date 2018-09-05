using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Core.Limit
{
    public interface ILimitingService : IDisposable
    {
        /// <summary>
        /// 申请流量处理
        /// </summary>
        /// <returns>true：获取成功，false：获取失败</returns>
        bool Request(string node);
        /// <summary>
        /// 获取可用令牌数
        /// </summary>
        /// <returns></returns>
        int GetAvailableTokenNum();

        NodeLimit Get1HNodeLimit(string node);
        NodeLimit Get24HNodeLimit(string node);
    }
}
