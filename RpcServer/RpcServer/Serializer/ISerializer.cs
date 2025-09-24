using RpcServer.DTO;

namespace RpcServer.Serializer
{
    public interface ISerializer
    {
        public RpcRequestDTO DeSerialize(string data);
        public RpcRequestDTO DeSerialize(byte[] data);
        public string Serialize(RpcResponseDTO data);
    }
}
