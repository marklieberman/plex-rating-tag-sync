using System.Text.Json.Serialization;

namespace PlexRatingTagSync
{
    internal class Metadata
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = String.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = String.Empty;

        [JsonPropertyName("ratingKey")] 
        public string RatingKey { get; set; } = String.Empty;

        public Media[]? Media { get; set; }

        public Media? FirstMedia => Media?[0];
    }
}
