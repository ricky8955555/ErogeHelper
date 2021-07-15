using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Entity.Payload
{
    public class MojiSearchPayload
    {
        [JsonPropertyName("langEnv")]
        public string? LangEnv { get; init; }

        [JsonPropertyName("needWords")]
        public string? NeedWords { get; init; }

        [JsonPropertyName("searchText")]
        public string? SearchText { get; init; }

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
