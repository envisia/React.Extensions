using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Envisia.React.Extensions
{
    public static class EnvisiaReactConstants
    {
        public static readonly JsonSerializerSettings JsonCamelCaseSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };
    }
}