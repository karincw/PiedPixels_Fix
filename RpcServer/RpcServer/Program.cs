using RpcServer.DTO;
using RpcServer.Serializer;
using RPCServer;
using RPCServer.RpcService;
using System.Text.Json;

namespace RpcServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<RpcDispatcher>();

            var app = builder.Build();

            app.MapGet("/", () => "Game Server is running!");

            app.MapPost("/rpc", async (HttpRequest request, RpcDispatcher rpcDispatcher) =>
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                DataFormat inputFormat = FormatUtility.ResolveInput(request);
                DataFormat outputFormat = FormatUtility.ResolveOutput(request);

                var rpcRequest = SerializerUtility.DeSerializer(body, inputFormat);

                if (rpcRequest == null)
                {
                    return Results.Json(new { error = "Invalid request" });
                }

                RpcResponseDTO responseDTO = await rpcDispatcher.DispatchAsync(rpcRequest);

                byte[] response = SerializerUtility.Serializer(responseDTO, outputFormat);

                return Results.Bytes(
                    contents: response,
                    contentType: FormatUtility.GetContentType(outputFormat)
                );
            });

            app.Run();
        }
    }
}

