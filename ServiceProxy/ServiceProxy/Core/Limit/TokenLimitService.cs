using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceProxy.Core.Limit
{
    public class TokenBucketLimitingService : ILimitingService
    {
        private LimitedQueue<object> limitedQueue = null;
        private System.Collections.Concurrent.ConcurrentDictionary<string, NodeLimit> Node1HTPS;
        private System.Collections.Concurrent.ConcurrentDictionary<string, NodeLimit> Node24HTPS;
        private CancellationTokenSource cancelToken;
        private Task task = null;
        /// <summary>
        /// 最大每秒处理能力
        /// </summary>
        private int maxTPS;
        /// <summary>
        /// 最大数量
        /// </summary>
        private int limitSize;
        private int nodeLimitQPH;
        private object lckObj = new object();
        private DateTime H24StartCountTime;
        private DateTime H1StartCountTime;
        /// <summary>
        /// 令牌桶服务
        /// </summary>
        /// <param name="maxTPS">最大每秒处理能力</param>
        /// <param name="limitSize">最大数量</param>
        /// <param name="limitQPH">每小时请求数量</param>
        public TokenBucketLimitingService(int maxTPS, int limitSize,int limitQPH)
        {
            this.limitSize = limitSize;
            this.maxTPS = maxTPS;

            if (this.limitSize <= 0)
                this.limitSize = 100;
            if (this.maxTPS <= 0)
                this.maxTPS = 1;

            limitedQueue = new LimitedQueue<object>(limitSize);
            for (int i = 0; i < limitSize; i++)
            {
                limitedQueue.Enqueue(new object());
            }
            cancelToken = new CancellationTokenSource();
            task = Task.Factory.StartNew(new Action(TokenProcess), cancelToken.Token);
            Node1HTPS = new System.Collections.Concurrent.ConcurrentDictionary<string, NodeLimit>();
            Node24HTPS = new System.Collections.Concurrent.ConcurrentDictionary<string, NodeLimit>();
            nodeLimitQPH = limitQPH;
            H1StartCountTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
            H24StartCountTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0); 
        }

        /// <summary>
        /// 定时消息令牌
        /// </summary>
        private void TokenProcess()
        {
            int sleep = 1000 / maxTPS;
            if (sleep == 0)
                sleep = 1;

            DateTime start = DateTime.Now;
            while (cancelToken.Token.IsCancellationRequested == false)
            {
                try
                {
                    lock (lckObj)
                    {
                        limitedQueue.Enqueue(new object());
                    }
                }
                catch
                {
                }
                finally
                {
                    if (DateTime.Now - start < TimeSpan.FromMilliseconds(sleep))
                    {
                        int newSleep = sleep - (int)(DateTime.Now - start).TotalMilliseconds;
                        if (newSleep > 1)
                            Thread.Sleep(newSleep - 1); //做一下时间上的补偿
                    }
                    start = DateTime.Now;
                }
            }
        }

        public void Dispose()
        {
            cancelToken.Cancel();
        }

        private bool Node1HLimit(string node,int limit)
        {
            var ts = DateTime.Now - H1StartCountTime;
            NodeLimit model = null;
            //1小时请求量不能超过指定数量
            if (ts.TotalHours > 1)
            {
                Node1HTPS.Clear();
                H1StartCountTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
            }
            if (Node1HTPS.ContainsKey(node))
            {
                model = Node1HTPS[node];
                if (model.Count > (limit/60) * ts.TotalMinutes)
                {
                    return false;
                }                
                model.Count ++;
            }
            else
            {
                model = new NodeLimit();
                model.Node = node;
                model.Count = 1;
                Node1HTPS.TryAdd(node, model);
            }
            return true;
        }
        private bool Node24HLimit(string node, int limit)
        {
            var ts = DateTime.Now - H24StartCountTime;
            NodeLimit model = null;
            //24小时请求量不能超过指定数量
            if (ts.TotalDays > 1)
            {
                Node24HTPS.Clear();
                H24StartCountTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            }
            if (Node24HTPS.ContainsKey(node))
            {
                model = Node24HTPS[node];
                if (model.Count > (limit * 3) * ts.TotalHours)
                {
                    return false;
                }                
                model.Count++;
            }
            else
            {
                model = new NodeLimit();
                model.Node = node;
                model.Count = 1;
                Node24HTPS.TryAdd(node, model);
            }
            return true;
        }

        /// <summary>
        /// 请求令牌
        /// </summary>
        /// <returns>true：获取成功，false：获取失败</returns>
        public bool Request(string node)
        {
            if (!string.IsNullOrEmpty(node))
            {
                var r01 = Node1HLimit(node, nodeLimitQPH);
                var r02 = Node24HLimit(node, nodeLimitQPH);
                if (!r01 || !r02)
                {
                    return false;
                }
            }            
            if (limitedQueue.Count <= 0)
                return false;
            lock (lckObj)
            {
                if (limitedQueue.Count <= 0)
                    return false;

                object data = limitedQueue.Dequeue();
                if (data == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取可用令牌数
        /// </summary>
        /// <returns></returns>
        public int GetAvailableTokenNum()
        {
            return limitedQueue.Count;
        }

        public NodeLimit Get1HNodeLimit(string node)
        {
            if (Node1HTPS.ContainsKey(node))
            {
                return Node1HTPS[node];
            }
            return null;
        }

        public NodeLimit Get24HNodeLimit(string node)
        {
            if (Node24HTPS.ContainsKey(node))
            {
                return Node24HTPS[node];
            }
            return null;
        }
    }
}
