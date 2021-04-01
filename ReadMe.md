# Dapr Grpc Service Invocation Demo

Demonstrates how to invoke a gRPC service using Dapr.

## Prerequisites
- Install [Docker Desktop](https://www.docker.com/products/docker-desktop) running Linux containers.
- Install [Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/).
- Run `dapr init`, `dapr --version`, `docker ps`
- [Dapr Visual Studio Extension](https://github.com/microsoft/vscode-dapr) (for debugging).

## gRPC Service with Dapr

- Follow the [Dapr ASP.NET Core gRPC example](https://github.com/dapr/dotnet-sdk/blob/master/examples/AspNetCore/GrpcServiceSample/README.md).

1. Create an ASPNET Core [gRPC Service](https://docs.microsoft.com/en-us/aspnet/core/grpc/aspnetcore) project.
    ```
    dotnet new grpc -o GrpcGreeterService    
    ```
   - Disable TLS (required to [run on Mac](https://docs.microsoft.com/en-us/aspnet/core/grpc/troubleshoot?view=aspnetcore-5.0#unable-to-start-aspnet-core-grpc-app-on-macos)). 
     - Edit `Program.CreateHostBuilder` as follows:

    ```csharp
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(options =>
                {
                    // Setup a HTTP/2 endpoint without TLS.
                    options.ListenLocalhost(5000, o => o.Protocols = 
                        HttpProtocols.Http2);
                });
                webBuilder.UseStartup<Startup>();
            });
    ```

2. Update `Startup.ConfigureServices` to register  `DaprClient`.
    ```csharp
    services.AddDaprClient();
    ```
3. Remove the service from the `greet.proto` file.
4. Add the `Dapr.AspNetCore` package.
5. Update the `Protobuf` section of the GrpcServiceSample.csproj file.
    ```xml
    <ItemGroup>
        <Protobuf Include="Protos\*.proto" ProtoRoot="Protos" GrpcServices="None" />
    </ItemGroup>
    ```
6. Derive `GreeterService` from `AppCallback.AppCallbackBase`.
   - Override `OnInvoke` to call `SayHello`.
    ```csharp
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
    ```
7. Run the service.
    ```
    dapr run --app-id grpcsample --app-port 5050 --app-protocol grpc -- dotnet run
    ```

## gRPC Client with Dapr

1. Create a Console app.
   - Add packages:
    ```
    Dapr.Client
    Google.Protobuf
    Grpc.Tools
    ```
2. Update .csproj file.
    ```xml
    <ItemGroup>
      <Protobuf Include="..\GrpcServiceSample\Protos\*.proto" ProtoRoot="..\GrpcServiceSample\Protos\" GrpcServices="None" />
    </ItemGroup>
    ```
3. Create a `DaprClient` using `DaprClientBuilder`.
   - Call `client.InvokeMethodGrpcAsync`, specifying app id and method name.
    ```csharp
    static async Task Main(string[] args)
    {
        using var client = new DaprClientBuilder().Build();

        var request = new HelloRequest() { Name = "Greeter" };
        Console.WriteLine($"Invoking grpc sayhello: {request.Name}");
        var reply = await client.InvokeMethodGrpcAsync<HelloRequest, HelloReply>("grpcsample", "sayhello", request);

        Console.WriteLine($"Reply: {reply.Message}");
        Console.WriteLine("Completed grpc sayhello");
    }
    ```
