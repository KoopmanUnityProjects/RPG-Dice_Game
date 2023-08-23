using System;
using MHLab.Patch.Core.Client.IO;

namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public readonly struct DownloaderCallbacks
    {
        public readonly Action<DownloadEntry> OnEntryStarted;
        public readonly Action<DownloadEntry> OnEntryCompleted;
        public readonly Action<long>          OnChunkDownloaded;

        public DownloaderCallbacks(
            Action<DownloadEntry> entryStarted, 
            Action<DownloadEntry> entryCompleted, 
            Action<long>          chunkDownloaded
            )
        {
            OnChunkDownloaded = chunkDownloaded;
            OnEntryCompleted  = entryCompleted;
            OnEntryStarted    = entryStarted;
        }
    }
}