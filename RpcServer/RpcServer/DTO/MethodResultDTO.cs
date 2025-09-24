namespace RpcServer.DTO
{
    public record MethodResultDTO(object result, RpcErrorDTO error);
}
