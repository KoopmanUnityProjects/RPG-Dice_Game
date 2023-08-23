using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Serializing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace MHLab.Patch.Core.Client.IO
{
    public class FileDownloader : IDownloader
    {
        public const int DownloadBlockSize         = 1024 * 32;
        public const int MaxDownloadRetries        = 10;
        public const int DelayForRetryMilliseconds = 50;

        public NetworkCredential   Credentials        { get; set; }
        public IDownloadSpeedMeter DownloadSpeedMeter { get; set; }
        public IDownloadMetrics    DownloadMetrics    { get; set; }
        public IWebProxy           Proxy              { get; set; }

        public bool IsCanceled { get; private set; }
        public bool IsPaused   { get; private set; }

        public event EventHandler DownloadComplete;

        protected readonly IFileSystem FileSystem;

        public FileDownloader(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;

            DownloadSpeedMeter = new DownloadSpeedMeter();
            DownloadMetrics = new DownloadMetrics()
            {
                RunningThreads = 1
            };
        }

        private void OnDownloadComplete()
        {
            if (this.DownloadComplete != null)
                this.DownloadComplete(this, new EventArgs());
        }

        public virtual void Download(List<DownloadEntry> entries,           Action<DownloadEntry> onEntryStarted,
                                     Action<long>        onChunkDownloaded, Action<DownloadEntry> onEntryCompleted)
        {
            IsCanceled = false;

            foreach (var downloadEntry in entries)
            {
                onEntryStarted?.Invoke(downloadEntry);
                Download(downloadEntry.RemoteUrl, downloadEntry.DestinationFolder, onChunkDownloaded);
                onEntryCompleted?.Invoke(downloadEntry);
            }

            DownloadSpeedMeter.Reset();
        }

        public virtual void Download(string url, string destFolder)
        {
            Download(url, destFolder, null);
        }

        public virtual void Download(string url, string destFolder, Action<long> onChunkDownloaded)
        {
            var destFileName = FileSystem.GetFilename((FilePath)url);

            destFolder = destFolder.Replace("file:///", "").Replace("file://", "");

            var downloadingTo = new FilePath(destFolder, FileSystem.CombinePaths(destFolder, destFileName).FullPath);

            FileSystem.CreateDirectory((FilePath)destFolder);

            if (!FileSystem.FileExists(downloadingTo))
            {
                // This allows the downloader to create 0-byte sized files.
                using (var fs = FileSystem.GetFileStream(downloadingTo, FileMode.OpenOrCreate, FileAccess.Read, FileShare.ReadWrite))
                {
                    fs.Flush();
                    fs.Dispose();
                    fs.Close();
                }
            }

            var buffer = new byte[DownloadBlockSize];

            var gotCanceled = false;

            using (var fs = FileSystem.GetFileStream(downloadingTo, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                var currentRetries = 0;

                while (currentRetries < MaxDownloadRetries)
                {
                    DownloadData data = null;

                    try
                    {
                        long currentRetryTotalReadCount = 0;

                        try
                        {
                            data = DownloadData.Create(url, fs, Credentials, Proxy);

                            if (data.Start > 0)
                                onChunkDownloaded?.Invoke(data.Start);

                            int readCount;
                            while ((int) (readCount = data.DownloadStream.Read(buffer, 0, DownloadBlockSize)) > 0)
                            {
                                if (IsCanceled)
                                {
                                    gotCanceled = true;
                                    data.Close();
                                    break;
                                }

                                currentRetryTotalReadCount += readCount;

                                SaveToFile(buffer, readCount, fs, downloadingTo.FullPath);

                                DownloadSpeedMeter.UpdateDownloadSpeed(readCount);
                                onChunkDownloaded?.Invoke(readCount);

                                if (IsCanceled)
                                {
                                    gotCanceled = true;
                                    data.Close();
                                    break;
                                }

                                while (IsPaused)
                                {
                                    Thread.Sleep(150);
                                }
                            }

                            currentRetries = MaxDownloadRetries;
                        }
                        catch
                        {
                            currentRetries++;

                            onChunkDownloaded?.Invoke(-currentRetryTotalReadCount);

                            if (currentRetries >= MaxDownloadRetries)
                            {
                                fs.Dispose();
                                fs.Close();

                                throw new WebException($"All retries have been tried for {url}.");
                            }

                            Thread.Sleep(DelayForRetryMilliseconds + (DelayForRetryMilliseconds * currentRetries));
                        }
                    }
                    catch (WebException webException)
                    {
                        throw new WebException($"The URL {url} generated an exception.", webException);
                    }
                    catch (UriFormatException e)
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Could not parse the URL \"{0}\" - it's either malformed or is an unknown protocol.",
                                url), e);
                    }
                    finally
                    {
                        if (data != null)
                            data.Close();
                    }
                }

                fs.Flush();
                fs.Dispose();
                fs.Close();
            }

            if (!gotCanceled)
                OnDownloadComplete();
        }

        public virtual T DownloadJson<T>(DownloadEntry entry, ISerializer serializer) where T : IJsonSerializable
        {
            var content = DownloadString(entry);
            return serializer.Deserialize<T>(content);
        }

        public virtual string DownloadString(DownloadEntry entry)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                DownloaderHelper.RemoteCertificateValidationCallback;
            using (WebClient client = new WebClient())
            {
                client.Credentials = Credentials;
                try
                {
                    return Encoding.UTF8.GetString(client.DownloadData(entry.RemoteUrl));
                }
                catch (WebException webException)
                {
                    throw new WebException($"The URL {entry.RemoteUrl} generated an exception.", webException);
                }
            }
        }

        public virtual void Cancel()
        {
            IsCanceled = true;
        }

        public virtual void Pause()
        {
            IsPaused = true;
        }

        public virtual void Resume()
        {
            IsPaused = false;
        }

        private void SaveToFile(byte[] buffer, int count, Stream file, string fileName)
        {
            try
            {
                file.Write(buffer, 0, count);
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("Error trying to save file \"{0}\": {1}", fileName, e.Message), e);
            }
        }
    }

    class DownloadData
    {
        public long Start => _start;
        public long Size  => _size;

        private HttpWebResponse _response;

        private Stream _stream;
        private long   _size;
        private long   _start;

        public static DownloadData Create(string url, Stream stream, NetworkCredential credentials, IWebProxy proxy)
        {
            const int MaxRetries   = 3;
            var       downloadData = new DownloadData();

            var            currentRetries = 0;
            HttpWebRequest request;

            while (currentRetries < MaxRetries)
            {
                try
                {
                    request = downloadData.GetRequest(url, credentials, proxy);

                    if (stream.Length > 0)
                    {
                        downloadData._start = stream.Length;
                        request.AddRange((int) downloadData._start);
                    }

                    downloadData._response = (HttpWebResponse) request.GetResponse();
                    downloadData._size     = downloadData._response.ContentLength;

                    if (downloadData.Response.StatusCode != HttpStatusCode.PartialContent)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        downloadData._start = 0;
                    }

                    currentRetries = MaxRetries;
                    break;
                }
                catch (WebException)
                {
                    currentRetries++;

                    if (currentRetries >= MaxRetries)
                    {
                        throw;
                    }
                }
                catch (Exception e)
                {
                    throw new ArgumentException(String.Format(
                                                    "Error downloading \"{0}\": {1}", url, e.Message), e);
                }
            }

            ValidateResponse(downloadData._response, url);

            return downloadData;
        }

        private DownloadData()
        {
        }

        private static void ValidateResponse(HttpWebResponse response, string url)
        {
            var status = (int) response.StatusCode;
            if (status >= 400)
            {
                /*throw new ArgumentException(
                    String.Format("Could not download \"{0}\" - an error has been returned from the web server.",
                    url));*/
                return;
            }
        }

        private long GetFileSize(string url, NetworkCredential credentials, IWebProxy proxy)
        {
            HttpWebResponse httpWebResponse = null;
            long            contentLength   = -1;
            try
            {
                var request = GetRequest(url, credentials, proxy);

                httpWebResponse = (HttpWebResponse) request.GetResponse();
                contentLength   = httpWebResponse.ContentLength;
            }
            finally
            {
                if (httpWebResponse != null)
                    httpWebResponse.Close();
            }

            return contentLength;
        }

        private HttpWebRequest GetRequest(string url, NetworkCredential credentials, IWebProxy proxy)
        {
            if (url == null)
                throw new ArgumentException("The URL parameter for this WebRequest is empty!", nameof(url));
            ServicePointManager.ServerCertificateValidationCallback =
                DownloaderHelper.RemoteCertificateValidationCallback;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null) throw new ArgumentException("The URL parameter is not an HTTP endpoint!", nameof(url));

            request.Credentials = credentials;
            request.Proxy       = proxy;
            request.KeepAlive   = true;

            return request;
        }

        public void Close()
        {
            this._response.Close();
        }

        public HttpWebResponse Response
        {
            get { return _response; }
            set { _response = value; }
        }

        public Stream DownloadStream
        {
            get
            {
                if (this._start == this._size)
                    return Stream.Null;
                if (this._stream == null)
                    this._stream = this._response.GetResponseStream();
                return this._stream;
            }
        }
    }
}