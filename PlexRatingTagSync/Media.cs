using System.Text.Json.Serialization;

namespace PlexRatingTagSync
{
    internal class Media
    {
        [JsonPropertyName("key")]
        public string? Key { get; set; }

        public Part[]? Part { get; set; }

        public Part? FirstPart => Part?[0];

        public string? FirstFile => FirstPart?.File;
    }
}
