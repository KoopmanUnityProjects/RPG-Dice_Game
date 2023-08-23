namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public class DownloaderSharedContext
    {
        public ChunkStorage        ChunkStorage { get; set; }
        public DownloaderCallbacks Callbacks    { get; set; }
    }
}