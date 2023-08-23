using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.Utilities.Asserts;

namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public class ChunkStorage
    {
        public readonly struct DownloadableChunkInfo
        {
            public readonly DownloadableChunk Chunk;
            public readonly bool              IsStarted;
            public readonly bool              IsLast;

            public DownloadableChunkInfo(DownloadableChunk chunk, bool isStarted, bool isLast)
            {
                Chunk     = chunk;
                IsLast    = isLast;
                IsStarted = isStarted;
            }
        }

        private readonly ConcurrentQueue<DownloadableChunk> _downloadQueue;
        private readonly Dictionary<DownloadEntry, int>     _remainingChunks;
        private readonly HashSet<DownloadEntry>             _startedEntries;

        public bool IsEmpty      => _downloadQueue.IsEmpty;
        public int  ChunksAmount => _downloadQueue.Count;

        public ChunkStorage()
        {
            _downloadQueue   = new ConcurrentQueue<DownloadableChunk>();
            _remainingChunks = new Dictionary<DownloadEntry, int>();
            _startedEntries  = new HashSet<DownloadEntry>();
        }

        public void EnqueueDownloadableChunk(DownloadableChunk chunk)
        {
            lock (_remainingChunks)
            {
                if (_remainingChunks.TryGetValue(chunk.DownloadEntry, out var remainingChunks))
                {
                    _remainingChunks[chunk.DownloadEntry] = remainingChunks + 1;
                }
                else
                {
                    _remainingChunks.Add(chunk.DownloadEntry, 1);
                }
            }

            _downloadQueue.Enqueue(chunk);
        }

        public DownloadableChunk[] GetDownloadableChunks()
        {
            return _downloadQueue.ToArray();
        }

        public bool FetchNext(out DownloadableChunkInfo chunkInfo)
        {
            if (_downloadQueue.TryDequeue(out var chunk))
            {
                var isStarted = false;
                var isLast    = false;

                lock (_startedEntries)
                {
                    if (!_startedEntries.Contains(chunk.DownloadEntry))
                    {
                        _startedEntries.Add(chunk.DownloadEntry);
                        isStarted = true;
                    }
                }

                lock (_remainingChunks)
                {
                    if (_remainingChunks.ContainsKey(chunk.DownloadEntry))
                    {
                        var remainingChunks = _remainingChunks[chunk.DownloadEntry];

                        _remainingChunks[chunk.DownloadEntry] = remainingChunks - 1;
                        
                        if (remainingChunks <= 1)
                            isLast = true;
                    }
                    else
                        isLast = true;
                }

                chunkInfo = new DownloadableChunkInfo(chunk, isStarted, isLast);
                return true;
            }

            chunkInfo = default;
            return false;
        }

        public static ChunkStorage CalculateChunks(List<DownloadEntry> entries, ChunkedDownloaderSettings settings)
        {
            var storage = new ChunkStorage();

            foreach (var downloadEntry in entries)
            {
                var targetSize = downloadEntry.Definition.Size;
                var chunkSize  = targetSize / settings.TasksAmount;

#if DEBUG
                var processedSize = 0L;
#endif

                chunkSize = Math.Max(chunkSize, settings.ChunkSize);
                chunkSize = Math.Min(chunkSize, settings.MaxChunkSize);

                for (long size = 0; size < targetSize; size += chunkSize)
                {
                    long offsetStart = size;
                    long offsetEnd   = size + chunkSize - 1;

                    offsetEnd = Math.Min(targetSize - 1, offsetEnd);

                    var chunk = new DownloadableChunk(offsetStart, offsetEnd, downloadEntry);

#if DEBUG
                    processedSize += chunk.Size;
#endif

                    storage.EnqueueDownloadableChunk(chunk);
                }

#if DEBUG
                Assert.Check(processedSize == targetSize);
#endif
            }

            return storage;
        }
    }
}