namespace PlexRatingTagSync
{
    internal class MediaContainer
    {
        public Metadata[] Metadata { get; set; } = Array.Empty<Metadata>();
        public Directory[] Directory { get; set; } = Array.Empty<Directory>();
    }
}
