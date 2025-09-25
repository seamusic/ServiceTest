using Grpc.Core;
using GrpcPureService.Services;
using Microsoft.Extensions.Logging;

namespace GrpcPureService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // 创建日志记录器
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            // 创建 gRPC 服务器
            var server = new Server
            {
                Services = 
                {
                    // 注册 Greeter 服务
                    GrpcShared.Greeter.BindService(new Services.GreeterService(loggerFactory.CreateLogger<Services.GreeterService>())),
                    // 注册 DeviceStatus 服务
                    GrpcShared.DeviceStatusService.BindService(new Services.DeviceStatusService(loggerFactory.CreateLogger<Services.DeviceStatusService>()))
                },
                Ports = { new ServerPort("localhost", 50051, ServerCredentials.Insecure) }
            };

            try
            {
                // 启动服务器
                server.Start();
                logger.LogInformation("gRPC 服务器已启动，监听端口: 50051");
                logger.LogInformation("按 Ctrl+C 停止服务器");

                // 等待服务器关闭
                await server.ShutdownTask;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "服务器启动失败");
            }
            finally
            {
                // 确保服务器正确关闭
                await server.ShutdownAsync();
                logger.LogInformation("gRPC 服务器已关闭");
            }
        }
    }
}
