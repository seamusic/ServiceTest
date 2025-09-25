# 消息队列订阅者项目

这是一个用于订阅消息队列消息的控制台应用程序，使用RabbitMQ作为消息队列。

## 功能特性

- 订阅RabbitMQ消息
- 异步消息处理
- 结构化日志记录
- 优雅的服务启动和停止
- 消息确认机制

## 配置

在 `appsettings.json` 中配置RabbitMQ连接参数：

```json
{
  "MessageQueue": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "QueueName": "test-queue"
  }
}
```

## 运行

```bash
dotnet run
```

## 依赖包

- RabbitMQ.Client: RabbitMQ客户端库
- Microsoft.Extensions.Hosting: 托管服务支持
- Microsoft.Extensions.Logging: 日志记录
- Microsoft.Extensions.Configuration: 配置管理
