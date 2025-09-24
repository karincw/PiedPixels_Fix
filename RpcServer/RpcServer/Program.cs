using RpcServer.DTO;
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

            //                                                  자동으로 넣어줌
            app.MapPost("/rpc", async (HttpRequest request, RpcDispatcher rpcDispatcher) =>
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                var rpcRequest = JsonSerializer.Deserialize<RpcRequestDTO>(body);

                if (rpcRequest == null)
                {
                    return Results.Json(new { error = "Invalid request" });
                }

                var response = await rpcDispatcher.DispatchAsync(rpcRequest);

                return Results.Json(response);
            });

            app.Run();
        }
    }
}

