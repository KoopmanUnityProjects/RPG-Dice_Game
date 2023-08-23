using System;

namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public class ChunkedDownloaderSettings
    {
        public long ChunkSize    { get; set; }
        public long MaxChunkSize { get; set; }

        public int TasksAmount { get; set; }

        public int MaxDownloadRetries { get; set; }
        public int DelayForRetry      { get; set; }

        public ChunkedDownloaderSettings()
        {
            ChunkSize          = 1 * 1024 * 1024;
            MaxChunkSize       = 8 * ChunkSize;
            TasksAmount        = Math.Max(32, Environment.ProcessorCount * 4);
            MaxDownloadRetries = 10;
            DelayForRetry      = 50;
        }
    }
}