using System.Text.Json.Serialization;

namespace ErogeHelper.Model.Entity.Response
{
    public class GameSettingResponse
    {
        [JsonPropertyName("Id")]
        public int GameId { get; init; }

        [JsonPropertyName("TextSettingJson")]
        public string? GameSettingJson { get; init; }

        [JsonIgnore]
        public string? Regex { get; init; }

        public override string ToString()
        {
            return $"GameId={GameId} TextractorSettingJson={GameSettingJson} RegExp={Regex}";
        }
    }
}