using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Entity.Payload
{
    public class MojiFetchPayload
    {
        [JsonPropertyName("wordId")]
        public string? WordId { get; init; }

        [JsonPropertyName("_ApplicationId")]
        public string? ApplicationId { get; init; }

        [JsonPropertyName("_ClientVersion")]
        public string? ClientVersion { get; init; }

        [JsonPropertyName("_InstallationId")]
        public string? InstallationId { get; init; }

        [JsonPropertyName("_SessionToken")]
        public string? SessionToken { get; init; }
    }
}
