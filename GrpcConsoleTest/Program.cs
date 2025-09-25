using Grpc.Net.Client;
using Grpc.Core;
using GrpcShared;

namespace GrpcConsoleTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== gRPC 客户端测试程序 ===");
            Console.WriteLine("请选择要测试的服务:");
            Console.WriteLine("1. HTTP + gRPC 服务 (端口 7237)");
            Console.WriteLine("2. 纯 gRPC 服务 (端口 50051)");
            Console.WriteLine("3. 测试所有服务");
            Console.Write("请输入选择 (1-3): ");
            
            var choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    await TestHttpGrpcServices();
                    break;
                case "2":
                    await TestPureGrpcServices();
                    break;
                case "3":
                    await TestHttpGrpcServices();
                    Console.WriteLine("\n" + new string('=', 60) + "\n");
                    await TestPureGrpcServices();
                    break;
                default:
                    Console.WriteLine("无效选择，默认测试 HTTP + gRPC 服务");
                    await TestHttpGrpcServices();
                    break;
            }
            
            Console.WriteLine("\n按任意键退出...");
            try
            {
                Console.ReadKey();
            }
            catch (InvalidOperationException)
            {
                // 当输入被重定向时忽略此异常
                Console.WriteLine("程序结束");
            }
        }
        
        static async Task TestHttpGrpcServices()
        {
            Console.WriteLine("\n=== 测试 HTTP + gRPC 服务 ===");
            
            // 创建 gRPC 通道连接到 HTTP + gRPC 服务器
            using var channel = GrpcChannel.ForAddress("https://localhost:7237");
            
            // 测试 Greeter 服务
            await TestGreeterService(channel);
            
            Console.WriteLine("\n" + new string('-', 50) + "\n");
            
            // 测试设备状态服务
            await TestDeviceStatusService(channel);
        }
        
        static async Task TestPureGrpcServices()
        {
            Console.WriteLine("\n=== 测试纯 gRPC 服务 ===");
            
            // 创建 gRPC 通道连接到纯 gRPC 服务器
            var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
            
            try
            {
                // 测试纯 gRPC Greeter 服务
                await TestPureGrpcGreeterService(channel);
                
                Console.WriteLine("\n" + new string('-', 50) + "\n");
                
                // 测试纯 gRPC 设备状态服务
                await TestPureGrpcDeviceStatusService(channel);
            }
            finally
            {
                // 关闭通道
                await channel.ShutdownAsync();
            }
        }

        static async Task TestGreeterService(GrpcChannel channel)
        {
            try
            {
                Console.WriteLine("--- 测试 HTTP + gRPC Greeter 服务 ---");
                
                // 创建 Greeter 客户端
                var greeterClient = new GrpcShared.Greeter.GreeterClient(channel);
                
                // 调用 SayHello 方法
                var request = new GrpcShared.HelloRequest { Name = "HTTP + gRPC 客户端" };
                var reply = await greeterClient.SayHelloAsync(request);
                
                Console.WriteLine($"Greeter 服务响应: {reply.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Greeter 服务测试失败: {ex.Message}");
            }
        }
        
        static async Task TestPureGrpcGreeterService(Channel channel)
        {
            try
            {
                Console.WriteLine("--- 测试纯 gRPC Greeter 服务 ---");
                
                // 创建纯 gRPC Greeter 客户端
                var greeterClient = new GrpcShared.Greeter.GreeterClient(channel);
                
                // 调用 SayHello 方法
                var request = new GrpcShared.HelloRequest { Name = "纯 gRPC 客户端" };
                var reply = await greeterClient.SayHelloAsync(request);
                
                Console.WriteLine($"纯 gRPC Greeter 服务响应: {reply.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"纯 gRPC Greeter 服务测试失败: {ex.Message}");
            }
        }

        static async Task TestDeviceStatusService(GrpcChannel channel)
        {
            try
            {
                Console.WriteLine("--- 测试 HTTP + gRPC 设备状态服务 ---");
                
                // 创建设备状态服务客户端
                var deviceClient = new GrpcShared.DeviceStatusService.DeviceStatusServiceClient(channel);
                
                // 测试设备状态查询
                var request = new GrpcShared.DeviceRequest
                {
                    DeviceId = "http-grpc-device-001",
                    IncludeMetrics = true,
                    Verbose = true
                };

                Console.WriteLine("发送设备状态查询请求...");
                Console.WriteLine($"设备ID: {request.DeviceId}");
                Console.WriteLine($"包含性能指标: {request.IncludeMetrics}");
                Console.WriteLine($"详细状态: {request.Verbose}");
                Console.WriteLine();

                var response = await deviceClient.GetDeviceStatusAsync(request);

                Console.WriteLine("收到设备状态响应:");
                Console.WriteLine($"设备ID: {response.DeviceId}");
                Console.WriteLine($"设备名称: {response.DeviceName}");
                Console.WriteLine($"设备类型: {response.DeviceType}");
                Console.WriteLine($"状态: {response.Status}");
                Console.WriteLine($"最后活动时间: {response.LastActiveTimestamp}");
                Console.WriteLine($"固件版本: {response.FirmwareVersion}");

                if (response.Metrics != null)
                {
                    Console.WriteLine("\n性能指标:");
                    Console.WriteLine($"CPU 使用率: {response.Metrics.CpuUsage}%");
                    Console.WriteLine($"内存使用率: {response.Metrics.MemoryUsage}%");
                    Console.WriteLine($"温度: {response.Metrics.Temperature}°C");
                    Console.WriteLine($"网络吞吐量: {response.Metrics.NetworkThroughput} Mbps");
                }

                if (response.StatusDetails.Count > 0)
                {
                    Console.WriteLine("\n详细状态信息:");
                    foreach (var detail in response.StatusDetails)
                    {
                        Console.WriteLine($"- {detail.Component}: {detail.Description}");
                    }
                }

                if (response.ErrorInfo != null)
                {
                    Console.WriteLine("\n错误信息:");
                    Console.WriteLine($"错误代码: {response.ErrorInfo.ErrorCode}");
                    Console.WriteLine($"错误消息: {response.ErrorInfo.ErrorMessage}");
                    Console.WriteLine($"发生时间: {response.ErrorInfo.OccurredAt}");
                    if (response.ErrorInfo.TroubleshootingTips.Count > 0)
                    {
                        Console.WriteLine("故障排除建议:");
                        foreach (var tip in response.ErrorInfo.TroubleshootingTips)
                        {
                            Console.WriteLine($"- {tip}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HTTP + gRPC 设备状态服务测试失败: {ex.Message}");
            }
        }
        
        static async Task TestPureGrpcDeviceStatusService(Channel channel)
        {
            try
            {
                Console.WriteLine("--- 测试纯 gRPC 设备状态服务 ---");
                
                // 创建纯 gRPC 设备状态服务客户端
                var deviceClient = new GrpcShared.DeviceStatusService.DeviceStatusServiceClient(channel);
                
                // 测试基本查询
                var basicRequest = new GrpcShared.DeviceRequest
                {
                    DeviceId = "pure-grpc-device-001",
                    IncludeMetrics = false,
                    Verbose = false
                };

                Console.WriteLine("发送基本设备状态查询请求...");
                Console.WriteLine($"设备ID: {basicRequest.DeviceId}");
                var basicResponse = await deviceClient.GetDeviceStatusAsync(basicRequest);
                
                Console.WriteLine("基本查询响应:");
                Console.WriteLine($"设备ID: {basicResponse.DeviceId}");
                Console.WriteLine($"设备名称: {basicResponse.DeviceName}");
                Console.WriteLine($"设备类型: {basicResponse.DeviceType}");
                Console.WriteLine($"状态: {basicResponse.Status}");
                Console.WriteLine($"固件版本: {basicResponse.FirmwareVersion}");
                
                Console.WriteLine();
                
                // 测试包含性能指标的查询
                var metricsRequest = new GrpcShared.DeviceRequest
                {
                    DeviceId = "pure-grpc-device-002",
                    IncludeMetrics = true,
                    Verbose = false
                };

                Console.WriteLine("发送包含性能指标的查询请求...");
                var metricsResponse = await deviceClient.GetDeviceStatusAsync(metricsRequest);
                
                Console.WriteLine("性能指标查询响应:");
                Console.WriteLine($"设备ID: {metricsResponse.DeviceId}");
                Console.WriteLine($"状态: {metricsResponse.Status}");
                
                if (metricsResponse.Metrics != null)
                {
                    Console.WriteLine("性能指标:");
                    Console.WriteLine($"  CPU 使用率: {metricsResponse.Metrics.CpuUsage}%");
                    Console.WriteLine($"  内存使用率: {metricsResponse.Metrics.MemoryUsage}%");
                    Console.WriteLine($"  温度: {metricsResponse.Metrics.Temperature}°C");
                    Console.WriteLine($"  网络吞吐量: {metricsResponse.Metrics.NetworkThroughput} Mbps");
                }
                
                Console.WriteLine();
                
                // 测试详细状态查询
                var verboseRequest = new GrpcShared.DeviceRequest
                {
                    DeviceId = "pure-grpc-device-003",
                    IncludeMetrics = true,
                    Verbose = true
                };

                Console.WriteLine("发送详细状态查询请求...");
                var verboseResponse = await deviceClient.GetDeviceStatusAsync(verboseRequest);
                
                Console.WriteLine("详细状态查询响应:");
                Console.WriteLine($"设备ID: {verboseResponse.DeviceId}");
                Console.WriteLine($"状态: {verboseResponse.Status}");
                
                if (verboseResponse.StatusDetails.Count > 0)
                {
                    Console.WriteLine("详细状态:");
                    foreach (var detail in verboseResponse.StatusDetails)
                    {
                        Console.WriteLine($"  {detail.Component}: {detail.StatusCode} - {detail.Description}");
                    }
                }
                
                if (verboseResponse.ErrorInfo != null)
                {
                    Console.WriteLine("\n错误信息:");
                    Console.WriteLine($"错误代码: {verboseResponse.ErrorInfo.ErrorCode}");
                    Console.WriteLine($"错误消息: {verboseResponse.ErrorInfo.ErrorMessage}");
                    Console.WriteLine($"发生时间: {verboseResponse.ErrorInfo.OccurredAt}");
                    if (verboseResponse.ErrorInfo.TroubleshootingTips.Count > 0)
                    {
                        Console.WriteLine("故障排除建议:");
                        foreach (var tip in verboseResponse.ErrorInfo.TroubleshootingTips)
                        {
                            Console.WriteLine($"- {tip}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"纯 gRPC 设备状态服务测试失败: {ex.Message}");
            }
        }
    }
}
