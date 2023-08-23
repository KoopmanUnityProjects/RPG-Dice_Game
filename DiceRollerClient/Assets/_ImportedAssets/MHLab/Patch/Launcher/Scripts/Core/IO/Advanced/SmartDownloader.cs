using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.Utilities.Asserts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MHLab.Patch.Core.Client.Advanced.IO
{
    public class SmartDownloader : FileDownloader
    {
        private static readonly int TasksAmount = Math.Max(1, Math.Min(8, Environment.ProcessorCount));
        private Task[] _tasks;
        private readonly Stopwatch _stopwatch;
        private readonly UpdatingContext _context;

        public SmartDownloader(UpdatingContext context) : base(context.FileSystem)
        {
            _tasks = new Task[TasksAmount];
            _stopwatch = new Stopwatch();
            _context = context;
            DownloadMetrics = new SmartDownloadMetrics(_tasks);
            DownloadSpeedMeter = new SmartDownloadSpeedMeter(DownloadMetrics);
        }

        public override void Download(List<DownloadEntry> entries, Action<DownloadEntry> onEntryStarted, Action<long> onChunkDownloaded, Action<DownloadEntry> onEntryCompleted)
        {
            _stopwatch.Restart();
            entries.Sort((entry1, entry2) =>
            {
                return entry1.Definition.Size.CompareTo(entry2.Definition.Size);
            });
            _stopwatch.Stop();

            _context.Logger.Info($"Sorted {entries.Count} files in [{_stopwatch.Elapsed.TotalSeconds}s].");

            var queue = new Queue<DownloadEntry>(entries);

            _stopwatch.Restart();
            
            for (var i = 0; i < TasksAmount; i++)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    var entryStarted = onEntryStarted;
                    var entryCompleted = onEntryCompleted;

                    while (true)
                    {
                        DownloadEntry entry;

                        lock (queue)
                        {
                            if (queue.Count == 0) break;
                            entry = queue.Dequeue();
                        }

                        if (IsCanceled) return;

                        entryStarted?.Invoke(entry);

                        int retriesCount = 0;

                        while (retriesCount < MaxDownloadRetries)
                        {
                            try
                            {
                                Download(entry.RemoteUrl, entry.DestinationFolder, onChunkDownloaded);

                                retriesCount = MaxDownloadRetries;
                            }
                            catch (Exception)
                            {
                                retriesCount++;

                                if (retriesCount >= MaxDownloadRetries)
                                {
                                    throw new WebException($"All retries have been tried for {entry.RemoteUrl}.");
                                }

                                Thread.Sleep(DelayForRetryMilliseconds + (DelayForRetryMilliseconds * retriesCount));
                            }
                        }

                        entryCompleted?.Invoke(entry);
                    }
                }, TaskCreationOptions.LongRunning);

                _tasks[i] = task;
            }

            Task.WaitAll(_tasks);
            _stopwatch.Stop();

            _context.Logger.Info($"Downloaded {entries.Count} files in [{_stopwatch.Elapsed.TotalSeconds}s].");

            if (_context.Settings.DebugMode)
            {
                Assert.AlwaysCheck(DownloaderHelper.ValidateDownloadedResult(entries, FileSystem, _context.Logger));
            }

            DownloadSpeedMeter.Reset();
        }
    }
}
