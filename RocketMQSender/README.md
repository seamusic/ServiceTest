# 消息队列发送者项目

这是一个用于发送消息队列消息的控制台应用程序，使用RabbitMQ作为消息队列。

## 功能特性

- 持续发送RabbitMQ消息
- 异步消息发送
- 结构化日志记录
- 错误处理和重试机制
- 支持单条消息发送

## 配置

在 `appsettings.json` 中配置RabbitMQ连接参数：

```json
{
  "MessageQueue": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "test-queue",
    "SendInterval": 1000
  }
}
```

## 运行

```bash
dotnet run
```

程序将每秒发送一条测试消息，按任意键停止发送。

## 依赖包

- RabbitMQ.Client: RabbitMQ客户端库
- Microsoft.Extensions.Hosting: 托管服务支持
- Microsoft.Extensions.Logging: 日志记录
- Microsoft.Extensions.Configuration: 配置管理
