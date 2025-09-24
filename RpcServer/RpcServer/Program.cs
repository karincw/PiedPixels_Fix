using RpcServer.DTO;
using System.Text.Json;

namespace RpcServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var app = builder.Build();

            app.MapGet("/", () => "Game Server is running!");

            app.MapPost("/rpc", async (HttpRequest request) =>
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                var rpcRequest = JsonSerializer.Deserialize<RpcRequestDTO>(body);

                if (rpcRequest == null)
                {
                    return Results.Json(new { error = "Invalid request" });
                }

                if (rpcRequest.Method == null)
                {
                    return Results.Json(new { error = "Method Name Required" });
                }
                object? result = null;

                if (rpcRequest.Method == "hello")
                {
                    result = $"hello {rpcRequest.Params["name"]}";
                }

                var response = new RpcResponseDTO
                {
                    id = rpcRequest.Id,
                    result = result
                };

                return Results.Json(response);
            });

            app.Run();
        }
    }
}
