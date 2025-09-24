using RpcServer.DTO;
using RpcServer.Serializer;
using RPCServer;
using System.Buffers;
using System.Net.WebSockets;

namespace RpcServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<RpcDispatcher>();

            var app = builder.Build();
            app.UseWebSockets();

            app.MapGet("/", () => "Game Server is running!");

            app.MapPost("/rpc", async (HttpRequest request, RpcDispatcher rpcDispatcher) =>
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                DataFormat inputFormat = FormatUtility.ResolveInput(request);
                DataFormat outputFormat = FormatUtility.ResolveOutput(request);

                RpcRequestDTO rpcRequest = SerializerUtility.DeSerializer(body, inputFormat);

                if (rpcRequest == null)
                {
                    return Results.Json(new RpcResponseDTO
                    {
                        id = 0,
                        error = new RpcErrorDTO(code: -32700, message: "Parse error", null),
                        result = null
                    });
                }

                RpcResponseDTO responseDTO = await rpcDispatcher.DispatchAsync(rpcRequest);

                byte[] response = SerializerUtility.Serializer(responseDTO, outputFormat);

                return Results.Bytes(
                    contents: response,
                    contentType: FormatUtility.GetContentType(outputFormat)
                );
            });

            app.MapGet("/ws", async (HttpContext context, RpcDispatcher rpcDispatcher) =>
            {
                if (!context.WebSockets.IsWebSocketRequest)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }

                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                byte[] buffer = null;

                while (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        buffer = ArrayPool<byte>.Shared.Rent(1024 * 4);

                        using var memoryStream = new MemoryStream();

                        WebSocketReceiveResult receiveResult;
                        do
                        {
                            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                            if (receiveResult.MessageType == WebSocketMessageType.Close)
                            {
                                await webSocket.CloseAsync(
                                    WebSocketCloseStatus.NormalClosure,
                                    "Closed by client",
                                    CancellationToken.None);
                                return;
                            }

                            memoryStream.Write(buffer, 0, receiveResult.Count);

                        } while (!receiveResult.EndOfMessage);

                        if (receiveResult.MessageType != WebSocketMessageType.Text)
                        {
                            continue;
                        }

                        DataFormat inputFormat = FormatUtility.ResolveInput(context);
                        DataFormat outputFormat = FormatUtility.ResolveOutput(context);

                        RpcRequestDTO rpcRequest = SerializerUtility.DeSerializer(memoryStream.ToArray(), inputFormat);

                        RpcResponseDTO rpcResponse = await rpcDispatcher.DispatchAsync(rpcRequest);


                        var responseBytes = SerializerUtility.Serializer(rpcResponse, outputFormat);

                        await webSocket.SendAsync(
                            new ArraySegment<byte>(responseBytes),
                            WebSocketMessageType.Text,
                            endOfMessage: true,
                            cancellationToken: CancellationToken.None);
                    }
                    catch (WebSocketException wsex)
                    {
                        Console.WriteLine($"[WebSocketException] {wsex.Message}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Unhandled] {ex.Message}");
                        break;
                    }
                    finally
                    {
                        if (buffer != null)
                            ArrayPool<byte>.Shared.Return(buffer, true);
                    }
                }
            });

            app.Run();
        }
    }
}

