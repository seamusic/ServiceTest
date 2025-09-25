using Grpc.Core;
using Microsoft.Extensions.Logging;
using GrpcShared;

namespace GrpcPureService.Services
{
    public class DeviceStatusService(ILogger<DeviceStatusService> logger) : GrpcShared.DeviceStatusService.DeviceStatusServiceBase
    {
        public override Task<DeviceResponse> GetDeviceStatus(DeviceRequest request, ServerCallContext context)
        {
            logger.LogInformation("收到设备状态查询请求，设备ID: {DeviceId}", request.DeviceId);

            try
            {
                // 模拟设备数据查询
                var deviceResponse = new DeviceResponse
                {
                    DeviceId = request.DeviceId,
                    DeviceName = "智能环境监测仪-客厅",
                    DeviceType = DeviceType.Sensor,
                    Status = DeviceStatus.Online,
                    LastActiveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    FirmwareVersion = "v2.5.1"
                };

                // 如果请求包含性能指标
                if (request.IncludeMetrics)
                {
                    deviceResponse.Metrics = new DeviceResponse.Types.PerformanceMetrics
                    {
                        CpuUsage = 15.5,
                        MemoryUsage = 45.2,
                        Temperature = 25.8,
                        NetworkThroughput = 5.7
                    };
                }

                // 如果请求详细状态信息
                if (request.Verbose)
                {
                    deviceResponse.StatusDetails.AddRange(new[]
                    {
                        new StatusDetail
                        {
                            Component = "NetworkInterface",
                            StatusCode = "CONNECTED",
                            Description = "Successfully connected to Wi-Fi network 'HomeNet-5G'."
                        },
                        new StatusDetail
                        {
                            Component = "SensorModule",
                            StatusCode = "CALIBRATED",
                            Description = "Temperature sensor calibrated successfully."
                        },
                        new StatusDetail
                        {
                            Component = "Heartbeat",
                            StatusCode = "SUCCESS",
                            Description = "Sent successful heartbeat to server."
                        }
                    });
                }

                logger.LogInformation("成功返回设备状态，设备ID: {DeviceId}, 状态: {Status}", 
                    request.DeviceId, deviceResponse.Status);

                return Task.FromResult(deviceResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "查询设备状态时发生错误，设备ID: {DeviceId}", request.DeviceId);
                
                // 返回错误响应
                return Task.FromResult(new DeviceResponse
                {
                    DeviceId = request.DeviceId,
                    DeviceName = "未知设备",
                    DeviceType = DeviceType.Unknown,
                    Status = DeviceStatus.Error,
                    LastActiveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    FirmwareVersion = "未知",
                    ErrorInfo = new ErrorInfo
                    {
                        ErrorCode = "QUERY_FAILED",
                        ErrorMessage = "查询设备状态失败",
                        OccurredAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        TroubleshootingTips = { "检查设备连接", "验证设备ID是否正确", "联系技术支持" }
                    }
                });
            }
        }
    }
}
