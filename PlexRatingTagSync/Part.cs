using System.Text.Json.Serialization;

namespace PlexRatingTagSync
{
    internal class Part
    {
        [JsonPropertyName("file")]
        public string File { get; set; } = String.Empty;
    }
}
