# 纯 gRPC 服务项目

这是一个基于 `Grpc.Core` 的纯 gRPC 服务项目，不依赖 HTTP 或 ASP.NET Core。

## 项目特点

- 使用 `Grpc.Core` 而不是 `Grpc.AspNetCore`
- 不依赖 HTTP 服务器
- 直接使用 gRPC 协议进行通信
- 支持两个服务：Greeter 和 DeviceStatus

## 项目结构

```
GrpcPureService/
├── Program.cs                 # 服务器入口点
├── Services/
│   ├── GreeterService.cs      # Greeter 服务实现
│   └── DeviceStatusService.cs # DeviceStatus 服务实现
├── Protos/
│   ├── greet.proto           # Greeter 服务定义
│   └── device_status.proto   # DeviceStatus 服务定义
└── GrpcPureService.csproj    # 项目文件
```

## 运行服务器

```bash
cd GrpcPureService
dotnet run
```

服务器将在 `localhost:50051` 端口启动。

## 测试客户端

使用 `GrpcPureClient` 项目来测试服务：

```bash
# 在另一个终端窗口中
cd GrpcPureClient
dotnet run
```

## 服务说明

### Greeter 服务
- **方法**: `SayHello`
- **功能**: 返回问候消息
- **请求**: `HelloRequest` (包含 name 字段)
- **响应**: `HelloReply` (包含 message 字段)

### DeviceStatus 服务
- **方法**: `GetDeviceStatus`
- **功能**: 查询设备状态信息
- **请求**: `DeviceRequest` (包含设备ID和查询选项)
- **响应**: `DeviceResponse` (包含设备详细状态信息)

## 与 HTTP 版本的区别

| 特性 | HTTP 版本 (GrpcServiceTest) | 纯 gRPC 版本 (GrpcPureService) |
|------|---------------------------|-------------------------------|
| 基础框架 | ASP.NET Core | Grpc.Core |
| 协议支持 | HTTP/2 + gRPC | 纯 gRPC |
| 依赖 | WebApplication | Server |
| 端口配置 | 通过 ASP.NET Core | 直接配置 |
| 中间件支持 | 是 | 否 |
| 性能 | 略低（HTTP 开销） | 更高（纯 gRPC） |

## 使用场景

纯 gRPC 服务适用于：
- 微服务间的高性能通信
- 不需要 HTTP 网关的场景
- 对性能要求较高的应用
- 内部服务通信

HTTP + gRPC 服务适用于：
- 需要 HTTP 网关的场景
- 需要与现有 HTTP 基础设施集成
- 需要中间件支持的应用
- 对外提供 API 的服务
