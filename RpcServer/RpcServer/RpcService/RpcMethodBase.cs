using RpcServer.DTO;

namespace RPCServer.RpcService
{
    public interface IRpcMethodBase
    {
        public Task<MethodResultDTO> HelloAsync(Dictionary<string, object>? parameters);
    }
}
