# gRPC 客户端测试程序

这是一个综合的 gRPC 客户端测试程序，可以测试两种不同类型的 gRPC 服务：

## 支持的服务类型

### 1. HTTP + gRPC 服务 (GrpcServiceTest)
- **端口**: 7237 (HTTPS)
- **基础框架**: ASP.NET Core + Grpc.AspNetCore
- **协议**: HTTP/2 + gRPC
- **特点**: 支持中间件、依赖注入、HTTP 网关

### 2. 纯 gRPC 服务 (GrpcPureService)
- **端口**: 50051
- **基础框架**: Grpc.Core
- **协议**: 纯 gRPC
- **特点**: 高性能、无 HTTP 开销

## 使用方法

### 运行客户端测试程序

```bash
cd GrpcConsoleTest
dotnet run
```

程序会显示菜单，让您选择要测试的服务：

```
=== gRPC 客户端测试程序 ===
请选择要测试的服务:
1. HTTP + gRPC 服务 (端口 7237)
2. 纯 gRPC 服务 (端口 50051)
3. 测试所有服务
请输入选择 (1-3):
```

### 测试选项说明

- **选项 1**: 仅测试 HTTP + gRPC 服务
- **选项 2**: 仅测试纯 gRPC 服务
- **选项 3**: 依次测试所有服务

## 测试内容

### Greeter 服务测试
- 发送问候请求
- 接收并显示响应消息
- 支持中文消息

### DeviceStatus 服务测试
- **基本查询**: 获取设备基本信息
- **性能指标查询**: 包含 CPU、内存、温度、网络等指标
- **详细状态查询**: 包含组件状态详情和错误信息

## 服务对比

| 特性 | HTTP + gRPC | 纯 gRPC |
|------|-------------|---------|
| 端口 | 7237 (HTTPS) | 50051 |
| 协议 | HTTP/2 + gRPC | 纯 gRPC |
| 性能 | 中等 | 高 |
| 功能 | 完整 | 核心 |
| 适用场景 | Web API、网关 | 微服务通信 |

## 运行前准备

### 启动 HTTP + gRPC 服务
```bash
cd GrpcServiceTest
dotnet run
```

### 启动纯 gRPC 服务
```bash
cd GrpcPureService
dotnet run
```

## 示例输出

### 纯 gRPC 服务测试输出
```
=== 测试纯 gRPC 服务 ===
--- 测试纯 gRPC Greeter 服务 ---
纯 gRPC Greeter 服务响应: 你好 纯 gRPC 客户端

--- 测试纯 gRPC 设备状态服务 ---
发送基本设备状态查询请求...
设备ID: pure-grpc-device-001
基本查询响应:
设备ID: pure-grpc-device-001
设备名称: 智能环境监测仪-客厅
设备类型: Sensor
状态: Online
固件版本: v2.5.1

发送包含性能指标的查询请求...
性能指标查询响应:
设备ID: pure-grpc-device-002
状态: Online
性能指标:
  CPU 使用率: 15.5%
  内存使用率: 45.2%
  温度: 25.8°C
  网络吞吐量: 5.7 Mbps

发送详细状态查询请求...
详细状态查询响应:
设备ID: pure-grpc-device-003
状态: Online
详细状态:
  NetworkInterface: CONNECTED - Successfully connected to Wi-Fi network 'HomeNet-5G'.
  SensorModule: CALIBRATED - Temperature sensor calibrated successfully.
  Heartbeat: SUCCESS - Sent successful heartbeat to server.
```

## 技术特点

- 支持两种不同的 gRPC 客户端实现
- 完整的错误处理
- 中文界面和消息支持
- 详细的测试输出
- 灵活的测试选项