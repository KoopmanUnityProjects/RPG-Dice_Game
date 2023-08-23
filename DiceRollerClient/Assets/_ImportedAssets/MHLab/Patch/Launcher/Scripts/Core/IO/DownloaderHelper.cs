using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Logging;
using MHLab.Patch.Core.Utilities;
using MHLab.Patch.Core.Utilities.Asserts;

namespace MHLab.Patch.Core.Client.IO
{
    public static class DownloaderHelper
    {
        public static bool RemoteCertificateValidationCallback(object    sender, X509Certificate certificate,
                                                               X509Chain chain,  SslPolicyErrors sslPolicyErrors)
        {
            bool isValid = true;
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag      = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode      = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags   = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2) certificate);
                        if (!chainIsValid)
                        {
                            isValid = false;
                        }
                    }
                }
            }

            return isValid;
        }

        public static bool ValidateDownloadedResult(List<DownloadEntry> entries, IFileSystem fileSystem, ILogger logger)
        {
            var errorRecognized = false;

            var builder = new StringBuilder();
            builder.AppendLine("The following files are not valid after debug validation: ");
            
            foreach (var entry in entries)
            {
                var info = fileSystem.GetFileInfo((FilePath)entry.DestinationFile);

                if (info.Size != entry.Definition.Size)
                {
                    builder.AppendLine(
                        $"[{entry.DestinationFile}] with expected size of [{entry.Definition.Size}]. Found [{info.Size}]");
                    errorRecognized = true;
                }
                else
                {
                    if (entry.Definition.Hash != null)
                    {
                        var hash = Hashing.GetFileHash(entry.DestinationFile, fileSystem);
                        if (hash != entry.Definition.Hash)
                        {
                            builder.AppendLine(
                                $"[{entry.DestinationFile}] with expected hash of [{entry.Definition.Hash}]. Found [{hash}]");
                            errorRecognized = true;
                        }
                    }
                }
            }

            if (errorRecognized)
            {
                logger.Debug(builder.ToString());
                return false;
            }

            return true;
        }
        
        public static HttpWebRequest GetRequest(string remoteUrl, IWebProxy proxy, NetworkCredential credentials)
        {
            Assert.NotNull(remoteUrl, $"Cannot create a request from an invalid RemoteUrl.");

            var request = WebRequest.CreateHttp(remoteUrl);
            request.Credentials            = credentials;
            request.Proxy                  = proxy;
            request.KeepAlive              = true;
            request.Accept                 = "*/*";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            Assert.AlwaysNotNull(request, 
                                 $"Cannot create the request: the RemoteUrl ({remoteUrl}) is an invalid HTTP resource.");

            return request;
        }
        
        public static HttpWebRequest GetRequest(string remoteUrl, long offsetStart, long offsetEnd, IWebProxy proxy, NetworkCredential credentials)
        {
            var request = GetRequest(remoteUrl, proxy, credentials);

            request.AddRange(offsetStart, offsetEnd);

            return request;
        }

        public static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            var response = (HttpWebResponse) request.GetResponse();

            return response;
        }
    }
}