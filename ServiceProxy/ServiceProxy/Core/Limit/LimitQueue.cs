using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceProxy.Core.Limit
{
    /// <summary>
    /// 限流模式
    /// </summary>
    public enum LimitingType
    {
        TokenBucket = 0,//令牌桶模式
        LeakageBucket = 1//漏桶模式
    }

    public class LimitedQueue<T> : Queue<T>
    {
        private int limit = 0;
        public const string QueueFulled = "TTP-StreamLimiting-1001";

        public int Limit
        {
            get { return limit; }
            set { limit = value; }
        }

        public LimitedQueue()
             : this(0)
        { }

        public LimitedQueue(int limit)
             : base(limit)
        {
            this.Limit = limit;
        }

        public new bool Enqueue(T item)
        {
            if (limit > 0 && this.Count >= this.Limit)
            {
                return false;
            }
            base.Enqueue(item);
            return true;
        }
    }

    public class NodeLimit
    {
        /// <summary>
        /// 节点
        /// </summary>
        public string Node { get; set; }
        /// <summary>
        /// 1小时统计
        /// </summary>
        public int Count { get; set; }
    }
}
