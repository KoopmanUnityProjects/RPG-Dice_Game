using System.Collections.Generic;
using System.IO;
using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.IO;

namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public class DownloaderContext
    {
        public DownloaderSharedContext SharedContext { get; private set; }
        
        public Dictionary<DownloadEntry, Stream> Streams { get; private set; }
        public byte[]                                Buffer  { get; private set; }

        public DownloaderContext(DownloaderSharedContext sharedContext, byte[] buffer)
        {
            SharedContext = sharedContext;
            Buffer        = buffer;
            Streams       = new Dictionary<DownloadEntry, Stream>();
        }

        public Stream GetStream(DownloadableChunk chunk, IFileSystem fileSystem)
        {
            EnsureStream(chunk, fileSystem);

            return Streams[chunk.DownloadEntry];
        }

        private void EnsureStream(DownloadableChunk chunk, IFileSystem fileSystem)
        {
            if (Streams.ContainsKey(chunk.DownloadEntry)) return;

            var filePath = (FilePath)chunk.DownloadEntry.DestinationFile;
            
            fileSystem.CreateDirectory(fileSystem.GetDirectoryPath(filePath));
            var fileStream = fileSystem.GetFileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            if (fileStream.Length != chunk.DownloadEntry.Definition.Size)
            {
                fileStream.SetLength(chunk.DownloadEntry.Definition.Size);
            }
            
            Streams.Add(chunk.DownloadEntry, fileStream);
        }

        public void Clear()
        {
            foreach (var stream in Streams)
            {
                var fileStream = stream.Value;
                fileStream.Flush();
                fileStream.Dispose();
            }
        }
    }
}