using RpcServer.DTO;
using RPCServer.RpcService;

namespace RPCServer
{
    public class RpcDispatcher
    {
        // Key : String
        // Value : Func
        // Func : [ Params : Nullable<Dictionary<string, object>>, Return : Task<object> ]
        private Dictionary<string, Func<Dictionary<string, object>?, Task<object>>> rpcMethodDictionary = new();

        public RpcDispatcher()
        {
            RpcMethod rpcMethod = new();

            var methods = typeof(IRpcMethodBase).GetMethods();
            foreach (var method in methods)
            {
                string rpcName = method.Name.Replace("Async", "").ToLower();

                rpcMethodDictionary[rpcName] = async (parameter) =>
                {
                    var result = method.Invoke(rpcMethod, new object?[] { parameter });
                    if (result is Task task)
                    {
                        await task; // Task 완료까지 기다림

                        // Task<T> 인 경우 결과 꺼내기
                        var taskType = task.GetType();
                        if (taskType.IsGenericType)
                        {
                            return taskType.GetProperty("Result")?.GetValue(task);
                        }
                        return null; // void Task인 경우
                    }

                    // Task가 아닌 경우(동기 메서드라면) 그냥 반환
                    return result;
                };

            }
        }

        public async Task<RpcResponseDTO> DispatchAsync(RpcRequestDTO request)
        {
            RpcResponseDTO ResponseDTO = new();
            if (rpcMethodDictionary.TryGetValue(request.Method.ToLower(), out var handler))
            {
                ResponseDTO.id = request.Id;
                try
                {
                    object result = await handler(request.Params);

                    if (result is MethodResultDTO methodResult)
                    {
                        ResponseDTO.JsonRpc = request.JsonRpc;
                        ResponseDTO.result = methodResult.result;
                        ResponseDTO.error = methodResult.error;
                    }
                    else
                    {
                        ResponseDTO.result = result;
                    }
                }
                catch (Exception ex)
                {
                    ResponseDTO.error = new RpcErrorDTO(-32000, "Server error", ex.Message);
                }
            }
            else
            {
                ResponseDTO.error = new RpcErrorDTO(-32601, "Method not found", null);
            }
            return ResponseDTO;
        }


    }
}
