using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Serializing;
using MHLab.Patch.Core.Utilities.Asserts;

namespace MHLab.Patch.Core.Client.Advanced.IO.Chunked
{
    public partial class ChunkedDownloader : IDownloader
    {
        public event EventHandler  DownloadComplete;
        public NetworkCredential   Credentials        { get; set; }
        public IWebProxy           Proxy              { get; set; }
        public IDownloadSpeedMeter DownloadSpeedMeter { get; set; }
        public IDownloadMetrics    DownloadMetrics    { get; }

        public bool IsCanceled => _isCanceled;
        public bool IsPaused   => _isPaused;

        private readonly UpdatingContext           _context;
        private readonly ChunkedDownloaderSettings _settings;

        private Task[] _runningTasks;
        private bool   _isCanceled;
        private bool   _isPaused;

        public ChunkedDownloader(UpdatingContext context, ChunkedDownloaderSettings settings)
        {
            _context  = context;
            _settings = settings;

            _runningTasks = new Task[settings.TasksAmount];

            DownloadMetrics    = new SmartDownloadMetrics(_runningTasks);
            DownloadSpeedMeter = new SmartDownloadSpeedMeter(DownloadMetrics);

            InitializeServicePointManager();
        }

        public ChunkedDownloader(UpdatingContext context) : this(context, new ChunkedDownloaderSettings())
        {
        }

        private static void InitializeServicePointManager()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                DownloaderHelper.RemoteCertificateValidationCallback;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls   |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue       = false;
            ServicePointManager.DefaultConnectionLimit  = 1000;
            ServicePointManager.MaxServicePointIdleTime = 10000;
        }

        public void Download(List<DownloadEntry>   entries,
                             Action<DownloadEntry> onEntryStarted,
                             Action<long>          onChunkDownloaded,
                             Action<DownloadEntry> onEntryCompleted)
        {
            _isCanceled = false;
            if (entries == null || entries.Count == 0) return;

            var downloaderCallbacks = new DownloaderCallbacks(onEntryStarted, onEntryCompleted, onChunkDownloaded);

            EnsureEmptyFilesOnDisk(entries, downloaderCallbacks);

            var downloaderContext = new DownloaderSharedContext()
            {
                Callbacks    = downloaderCallbacks,
                ChunkStorage = ChunkStorage.CalculateChunks(entries, _settings)
            };

            StartJobs(downloaderContext);

            if (_context.Settings.DebugMode)
            {
                Assert.AlwaysCheck(DownloaderHelper.ValidateDownloadedResult(entries, _context.FileSystem, _context.Logger));
            }
        }

        public void Download(string url, string destinationFolder)
        {
            Download(url, destinationFolder, null);
        }

        public void Download(string url, string destinationFolder, Action<long> onChunkDownloaded)
        {
            var request  = DownloaderHelper.GetRequest(url, Proxy, Credentials);
            var response = DownloaderHelper.GetResponse(request);
            var length   = response.ContentLength;

            var destinationFile = _context.FileSystem.CombinePaths(destinationFolder, _context.FileSystem.GetFilename((FilePath)url));

            var definition = new BuildDefinitionEntry()
            {
                Size = length
            };
            
            var downloadEntry = new DownloadEntry(url, null, null, destinationFile.FullPath, definition);
            var entries       = new List<DownloadEntry>(1) {downloadEntry};

            Download(entries, null, onChunkDownloaded, null);
        }

        public T DownloadJson<T>(DownloadEntry entry, ISerializer serializer) where T : IJsonSerializable
        {
            var data = DownloadString(entry);

            var obj = Activator.CreateInstance<T>();
            _context.Serializer.DeserializeOn(obj, data);

            return obj;
        }

        public string DownloadString(DownloadEntry entry)
        {
            using (var client = new WebClient())
            {
                client.Credentials = Credentials;
                client.Proxy       = Proxy;
                return client.DownloadString(entry.RemoteUrl);
            }
        }

        public void Cancel()
        {
            _isCanceled = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public void Pause()
        {
            _isPaused = true;
        }
    }
}