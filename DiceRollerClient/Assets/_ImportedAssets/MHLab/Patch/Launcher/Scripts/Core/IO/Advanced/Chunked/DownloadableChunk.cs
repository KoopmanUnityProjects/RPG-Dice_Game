using MHLab.Patch.Core.Client.IO;

namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public class DownloadableChunk
    {
        public readonly long          OffsetStart;
        public readonly long          OffsetEnd;
        public readonly DownloadEntry DownloadEntry;
            
        public long Size => (OffsetEnd + 1) - OffsetStart;

        public DownloadableChunk(long offsetStart, long offsetEnd, DownloadEntry entry)
        {
            OffsetStart   = offsetStart;
            OffsetEnd     = offsetEnd;
            DownloadEntry = entry;
        }
    }
}