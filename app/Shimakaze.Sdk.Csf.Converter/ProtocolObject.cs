using System.Text.Json.Serialization;

namespace Shimakaze.Sdk.Csf.Converter;

class ProtocolObject
{
    [JsonPropertyName("protocol")]
    public int Protocol { get; set; }
}