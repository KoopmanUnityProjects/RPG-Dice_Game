using System;
using System.Net;

namespace MHLab.Patch.Core.Client.IO
{
    public interface INetworkChecker
    {
        NetworkCredential Credentials { get; set; }
        IWebProxy Proxy { get; set; }

        bool IsNetworkAvailable();
        bool IsNetworkAvailable(long minimumSpeed);
        bool IsRemoteServiceAvailable(string url, out Exception exception);
    }
}
