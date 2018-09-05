# HttpServiceProxy
根据实际业务开发使用
该项目实际以下功能：
1.对http请求进行流量限制（使用令牌桶算法）
2.对单一客户端进行请求频率限制

使用dotnetcore开发，运行请安装dotnet core2.1

如需要发布到特定环境请使用：

--win64
dotnet publish -c Release -r win7-x64

--ubuntu
dotnet publish -c Release -r ubuntu.14.04-x64

--mac os
dotnet publish -c Release -r osx.10.10-x64

运行请配置配置文件config.json(默认)
以ubuntu为例：
./HttpServiceProxy config.json
./HttpServiceProxy
两种方式运行是一样的效果


配置文件格式如下：
{
  "redis": {
    "server": "server",
    "auth": "auth",
    "database": 0
  },
  "proxy": {
    "listenaddress": "10.108.52.25",
    "listenport": 20100,
    "serveraddress": "192.168.0.211",
    "serverport": 85
  },
  "limit":{"LimitingType":0,"MaxTPS":100,"MaxServiceQum":10000,"MaxFreq":7200},
  "logLevel": 1
}

LimitingType:0 *TokenBucketLimitingService
MaxTPS：每秒向令牌童中增加令牌的数量
MaxServiceQum：令牌桶的总大小
MaxFreq：单一客户端每小时的请求数量，限制分精确到分釧，如7200则为每分钟120次请求，从每小时的0分开始计算
限制分为小时限制和天限制，使用同一参数
如参数为MaxFreq：7200，一小时请大请求量为7200次，一天的限制为 3 小时量，即为 7200 * 3


