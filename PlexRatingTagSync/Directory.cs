using System.Text.Json.Serialization;

namespace PlexRatingTagSync
{
    internal class Directory
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = String.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = String.Empty;
    }
}
