using Dapr.Client;
using GrpcServiceSample;
using System;
using System.Threading.Tasks;

namespace Samples.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var client = new DaprClientBuilder().Build();

            var request = new HelloRequest() { Name = "Greeter" };
            Console.WriteLine($"Invoking grpc sayhello: {request.Name}");
            var reply = await client.InvokeMethodGrpcAsync<HelloRequest, HelloReply>("grpcsample", "sayhello", request);

            Console.WriteLine($"Reply: {reply.Message}");
            Console.WriteLine("Completed grpc sayhello");
        }
    }
}
