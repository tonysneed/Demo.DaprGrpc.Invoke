using System.Threading.Tasks;
using Dapr.AppCallback.Autogen.Grpc.v1;
using Dapr.Client;
using Dapr.Client.Autogen.Grpc.v1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace GrpcServiceSample
{
    /// <summary>
    /// Greeting gRPC service
    /// </summary>
    public class GreeterService : AppCallback.AppCallbackBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(DaprClient daprClient, ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var reply = new HelloReply
            {
                Message = "Hello " + request.Name
            };
            _logger.LogInformation("Sending reply: {reply}", reply.Message);
            return Task.FromResult(reply);
        }

        public override async Task<InvokeResponse> OnInvoke(InvokeRequest request, ServerCallContext context)
        {
            _logger.LogInformation("OnInvoke method: {method}", request.Method);
            var response = new InvokeResponse();
            switch (request.Method)
            {
                case "sayhello":                
                    var input = request.Data.Unpack<HelloRequest>();
                    var output = await SayHello(input, context);
                    response.Data = Any.Pack(output);
                    break;
                default:
                    break;
            }
            return response;
        }
    }
}
