# ServiceTest - 微服务测试项目

这是一个综合的微服务测试项目，包含 gRPC 服务、消息队列和相关的测试客户端。项目使用 .NET 10.0 开发，旨在演示和测试现代微服务架构中的各种通信模式。

## 🏗️ 项目结构

```
ServiceTest/
├── GrpcServiceTest/          # HTTP + gRPC 服务 (ASP.NET Core)
├── GrpcPureService/          # 纯 gRPC 服务 (Grpc.Core)
├── GrpcConsoleTest/          # gRPC 客户端测试程序
├── GrpcShared/               # 共享的 Protocol Buffers 定义
├── RocketMQSender/           # 消息队列发送者 (RabbitMQ)
├── RocketMQSubscriber/       # 消息队列订阅者 (RabbitMQ)
└── ServiceTest.slnx          # 解决方案文件
```

## 🚀 快速开始

### 前置条件

- .NET 10.0 SDK
- RabbitMQ 服务器 (用于消息队列测试)
- Visual Studio 2022 或 VS Code (推荐)

### 1. 克隆项目

```bash
git clone https://github.com/seamusic/ServiceTest.git
cd ServiceTest
```

### 2. 启动 RabbitMQ (可选)

如果使用 Docker：
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 3. 运行项目

#### 启动 gRPC 服务

**HTTP + gRPC 服务 (端口 7237):**
```bash
cd GrpcServiceTest
dotnet run
```

**纯 gRPC 服务 (端口 50051):**
```bash
cd GrpcPureService
dotnet run
```

#### 测试 gRPC 服务

```bash
cd GrpcConsoleTest
dotnet run
```

#### 测试消息队列

**启动消息订阅者:**
```bash
cd RocketMQSubscriber
dotnet run
```

**启动消息发送者:**
```bash
cd RocketMQSender
dotnet run
```

## 📋 项目组件详解

### 🔌 gRPC 服务

#### GrpcServiceTest (HTTP + gRPC)
- **端口**: 7237 (HTTPS)
- **框架**: ASP.NET Core + Grpc.AspNetCore
- **特点**: 支持中间件、依赖注入、HTTP 网关
- **适用场景**: Web API、需要 HTTP 网关的服务

#### GrpcPureService (纯 gRPC)
- **端口**: 50051
- **框架**: Grpc.Core
- **特点**: 高性能、无 HTTP 开销
- **适用场景**: 微服务间通信、高性能要求

#### 服务对比

| 特性 | HTTP + gRPC | 纯 gRPC |
|------|-------------|---------|
| 端口 | 7237 (HTTPS) | 50051 |
| 协议 | HTTP/2 + gRPC | 纯 gRPC |
| 性能 | 中等 | 高 |
| 功能 | 完整 | 核心 |
| 中间件支持 | ✅ | ❌ |
| 依赖注入 | ✅ | ❌ |

### 📨 消息队列 (RabbitMQ)

#### RocketMQSender
- 持续发送测试消息
- 支持单条消息发送
- 错误处理和重试机制
- 可配置发送间隔

#### RocketMQSubscriber
- 订阅并处理消息
- 异步消息处理
- 消息确认机制
- 自动重连功能

### 🧪 测试客户端

#### GrpcConsoleTest
- 支持测试两种 gRPC 服务
- 交互式菜单选择
- 完整的错误处理
- 中文界面支持

## 🔧 配置说明

### gRPC 服务配置

**GrpcServiceTest/appsettings.json:**
```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:7237"
      }
    }
  }
}
```

**GrpcPureService:**
- 默认端口: 50051
- 可通过代码修改端口配置

### 消息队列配置

**appsettings.json:**
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

## 📊 服务接口

### Greeter 服务
```protobuf
service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
}

message HelloRequest {
  string name = 1;
}

message HelloReply {
  string message = 1;
}
```

### DeviceStatus 服务
```protobuf
service DeviceStatusService {
  rpc GetDeviceStatus (DeviceRequest) returns (DeviceResponse);
}

message DeviceRequest {
  string device_id = 1;
  bool include_metrics = 2;
  bool include_detailed_status = 3;
}

message DeviceResponse {
  string device_id = 1;
  string device_name = 2;
  DeviceType device_type = 3;
  DeviceStatus status = 4;
  string firmware_version = 5;
  int64 last_active_timestamp = 6;
  PerformanceMetrics metrics = 7;
  DetailedStatus detailed_status = 8;
}
```

## 🛠️ 开发指南

### 添加新的 gRPC 服务

1. 在 `GrpcShared/Protos/` 中定义 `.proto` 文件
2. 在服务项目中实现服务类
3. 在 `Program.cs` 中注册服务
4. 更新客户端测试代码

### 添加新的消息队列功能

1. 修改 `appsettings.json` 配置
2. 更新消息处理逻辑
3. 添加新的消息类型支持

## 🧪 测试

### 运行所有测试

```bash
# 启动所有服务
dotnet run --project GrpcServiceTest &
dotnet run --project GrpcPureService &
dotnet run --project RocketMQSubscriber &

# 运行客户端测试
dotnet run --project GrpcConsoleTest
dotnet run --project RocketMQSender
```

### 性能测试

项目支持两种 gRPC 实现方式的性能对比：
- HTTP + gRPC: 适合需要 HTTP 网关的场景
- 纯 gRPC: 适合微服务间高性能通信

## 📝 开发规范

项目遵循以下开发规范：
- 使用 C# 编码标准
- 遵循 gRPC 最佳实践
- 结构化日志记录
- 完整的错误处理
- 中文注释和文档

## 🤝 贡献

欢迎提交 Issue 和 Pull Request 来改进项目。

## 📄 许可证

本项目采用 MIT 许可证。

## 🔗 相关链接

- [gRPC 官方文档](https://grpc.io/docs/)
- [RabbitMQ 官方文档](https://www.rabbitmq.com/documentation.html)
- [.NET gRPC 文档](https://docs.microsoft.com/en-us/aspnet/core/grpc/)
- [Protocol Buffers 文档](https://developers.google.com/protocol-buffers)

---

**注意**: 项目中的 "RocketMQ" 命名实际使用的是 RabbitMQ 实现。如需使用真正的 RocketMQ，需要替换相应的客户端库和配置。
