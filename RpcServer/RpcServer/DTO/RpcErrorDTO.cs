namespace RpcServer.DTO
{
    public record RpcErrorDTO(int code, string message, object? data);
}
