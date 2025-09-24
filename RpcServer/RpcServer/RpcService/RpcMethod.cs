
using RpcServer.DTO;

namespace RPCServer.RpcService
{
    public class RpcMethod : IRpcMethodBase
    {
        public Task<MethodResultDTO> HelloAsync(Dictionary<string, object>? parameters)
        {
            string paramName = "name";
            if (!ParameterCheck(parameters, paramName))
            {
                return Task.FromResult(new MethodResultDTO(result: $"Error!!", error: new RpcErrorDTO(-32602, $"Invalid params : [{paramName}]", null)));
            }

            //Json데이터 속 Params에 Name을 Key로 가진 데이터를 가져와서 스트링으로 만든 뒤 병합
            var name = parameters?["name"].ToString();
            return Task.FromResult(new MethodResultDTO(result: $"hello, {name}!", error: null));
        }

        private static bool ParameterCheck(Dictionary<string, object>? parameters, string find)
        {
            if (parameters == null || !parameters.ContainsKey(find) || parameters[find] == null)
            {
                return false;
            }
            return true;
        }
    }
}
