using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Core.Limit
{
    public class LimitingFactory
    {
        /// <summary>
        /// 创建限流服务对象
        /// </summary>
        /// <param name="limitingType">限流模型</param>
        /// <param name="maxQPS">最大QPS</param>
        /// <param name="limitSize">最大可用票据数</param>
        public static ILimitingService Build(LimitingType limitingType = LimitingType.TokenBucket, int maxQPS = 100, int limitSize = 100, int nodeLimitQPS = 100)
        {
            switch (limitingType)
            {
                case LimitingType.TokenBucket:
                default:
                    return new TokenBucketLimitingService(maxQPS, limitSize,nodeLimitQPS);
                case LimitingType.LeakageBucket:
                    throw new Exception("未实现");
            }
        }
    }
}
