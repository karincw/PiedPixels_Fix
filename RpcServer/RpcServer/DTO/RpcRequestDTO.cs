using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RpcServer.DTO
{
    public record RpcRequestDTO
    {
        public string? JsonRpc { get; set; } = "2.0";
        public string? Method { get; set; } = "";
        public Dictionary<string, object>? Params { get; set; }
        public int Id { get; set; }
    }
}
