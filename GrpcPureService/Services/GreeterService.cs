using Grpc.Core;
using Microsoft.Extensions.Logging;
using GrpcShared;

namespace GrpcPureService.Services
{
    public class GreeterService(ILogger<GreeterService> logger) : GrpcShared.Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            logger.LogInformation("收到来自 {Name} 的消息", request.Name);

            return Task.FromResult(new HelloReply
            {
                Message = "你好 " + request.Name
            });
        }
    }
}
