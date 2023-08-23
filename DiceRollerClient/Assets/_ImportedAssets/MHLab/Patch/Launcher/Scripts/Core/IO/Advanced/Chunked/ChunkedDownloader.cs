using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Utilities.Asserts;

namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public partial class ChunkedDownloader
    {
        private static void ValidatePartialContentResponse(HttpWebResponse response)
        {
            Assert.AlwaysCheck(
                response.StatusCode == HttpStatusCode.PartialContent,
                "The server does not support chunked downloading. Consider using a different downloader."
            );
        }

        private static void ValidateResponse(HttpWebResponse response, DownloadableChunk chunk)
        {
            ValidatePartialContentResponse(response);

            Assert.AlwaysCheck(
                response.ContentLength == chunk.Size,
                $"The response from {chunk.DownloadEntry.RemoteUrl} has an unexpected size. Expected: {chunk.Size} - Actual: {response.ContentLength}"
            );
        }

        private void StartJobs(DownloaderSharedContext sharedContext)
        {
            var chunksAmount = sharedContext.ChunkStorage.ChunksAmount;

            var tasksAmount = Math.Min(_settings.TasksAmount, chunksAmount);
            
            var stopwatch = new Stopwatch();
            stopwatch.Restart();

            for (var i = 0; i < tasksAmount; i++)
            {
                _runningTasks[i] =
                    Task.Factory.StartNew(
                        ProcessJob,
                        sharedContext,
                        TaskCreationOptions.LongRunning
                    );
            }

            // Filter out null tasks (the array is pre-allocated, so if tasksAmount is small some of the values
            // in the array may be un-initialized).
            var tasksToWaitOn = _runningTasks.Where(t => t != null).ToArray();

            Task.WaitAll(tasksToWaitOn);

            stopwatch.Stop();
            _context.Logger.Info(
                $"Downloaded and stored on disk [{chunksAmount - sharedContext.ChunkStorage.ChunksAmount}/{chunksAmount}] chunks in [{stopwatch.Elapsed.TotalSeconds}s] on [{tasksAmount}] threads.");

            if (_isCanceled)
            {
                _context.Logger.Info($"Download canceled.");
            }
            else
            {
                DownloadComplete?.Invoke(this, null);
            }
            
            DownloadSpeedMeter.Reset();
        }

        private void ProcessJob(object state)
        {
            var sharedContext = (DownloaderSharedContext) state;
            var context       = new DownloaderContext(sharedContext, new byte[_settings.MaxChunkSize]);

            while (true)
            {
                if (_isCanceled || sharedContext.ChunkStorage.IsEmpty)
                    break;

                if (_isPaused)
                {
                    Thread.Sleep(150);
                    continue;
                }

                if (sharedContext.ChunkStorage.FetchNext(out var chunkInfo))
                {
                    if (chunkInfo.IsStarted)
                    {
                        sharedContext.Callbacks.OnEntryStarted?.Invoke(chunkInfo.Chunk.DownloadEntry);
                    }
                    
                    var downloadedSize = DownloadChunk(context, chunkInfo.Chunk);
                    WriteToDisk(context, chunkInfo.Chunk, downloadedSize);

                    if (chunkInfo.IsLast)
                    {
                        sharedContext.Callbacks.OnEntryCompleted?.Invoke(chunkInfo.Chunk.DownloadEntry);
                    }
                }
            }
            
            context.Clear();
        }

        private long DownloadChunk(DownloaderContext context, DownloadableChunk chunk)
        {
            var sharedContext  = context.SharedContext;
            var buffer         = context.Buffer;
            var currentRetries = 0;
            
            while (currentRetries < _settings.MaxDownloadRetries)
            {
                if (_isCanceled) return -1;
                
                HttpWebRequest  request  = null;
                HttpWebResponse response = null;

                try
                {
                    request  = DownloaderHelper.GetRequest(chunk.DownloadEntry.RemoteUrl, chunk.OffsetStart, chunk.OffsetEnd, Proxy, Credentials);
                    response = DownloaderHelper.GetResponse(request);
                    
                    if (_context.Settings.DebugMode)
                        ValidateResponse(response, chunk);

                    var stream = response.GetResponseStream();

                    Assert.AlwaysNotNull(
                        stream, $"The response stream for {chunk.DownloadEntry.RemoteUrl} is not valid.");

                    var expectedSize = chunk.Size;

                    int remainingSize   = (int) expectedSize;
                    int totalDownloaded = 0;
                    int readCount;

                    while ((readCount = stream.Read(buffer, totalDownloaded, (int) remainingSize)) > 0)
                    {
                        if (_isCanceled) break;

                        remainingSize   -= readCount;
                        totalDownloaded += readCount;
                        
                        DownloadSpeedMeter.UpdateDownloadSpeed(readCount);
                        sharedContext.Callbacks.OnChunkDownloaded?.Invoke(readCount);
                    }

                    Assert.AlwaysCheck(
                        totalDownloaded == expectedSize,
                        $"The read operation from the resource {chunk.DownloadEntry.RemoteUrl} has an unexpected size. Expected: {expectedSize} - Actual: {totalDownloaded}"
                    );

                    return totalDownloaded;
                }
                catch (Exception e)
                {
                    currentRetries++;

                    if (currentRetries >= _settings.MaxDownloadRetries)
                    {
                        _isCanceled = true;
                        throw new WebException(
                            $"Tried to download [{chunk.DownloadEntry.RemoteUrl}] for [{currentRetries}] times, but failed. Reason: {e.Message} - {e.StackTrace}");
                    }

                    Thread.Sleep(_settings.DelayForRetry + (_settings.DelayForRetry * currentRetries));
                }
                finally
                {
                    if (response != null)
                        response.Close();
                }
            }

            throw new WebException(
                $"Tried to download [{chunk.DownloadEntry.RemoteUrl}] for [{currentRetries}] times, but failed.");
        }

        private void WriteToDisk(DownloaderContext context, DownloadableChunk chunk, long downloadedSize)
        {
            var stream = context.GetStream(chunk, _context.FileSystem);
            
            stream.Seek(chunk.OffsetStart, SeekOrigin.Begin);
            stream.Write(context.Buffer, 0, (int) downloadedSize);
        }

        private void EnsureEmptyFilesOnDisk(List<DownloadEntry> entries, DownloaderCallbacks downloaderCallbacks)
        {
            foreach (var downloadEntry in entries)
            {
                if (downloadEntry.Definition.Size == 0)
                {
                    downloaderCallbacks.OnEntryStarted?.Invoke(downloadEntry);
                    EnsureEmptyFileOnDisk(downloadEntry);
                    downloaderCallbacks.OnEntryCompleted?.Invoke(downloadEntry);
                }
            }
        }

        private void EnsureEmptyFileOnDisk(DownloadEntry entry)
        {
            _context.FileSystem.CreateDirectory((FilePath)entry.DestinationFolder);
            using (var fs = _context.FileSystem.CreateFile((FilePath)entry.DestinationFile))
            {
            }
        }
    }
}