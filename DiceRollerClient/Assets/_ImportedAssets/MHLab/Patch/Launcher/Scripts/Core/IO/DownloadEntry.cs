namespace MHLab.Patch.Core.Client.IO
{
    public class DownloadEntry
    {
        public string RemoteUrl;
        public string PartialRemoteUrl;
        public string DestinationFolder;
        public string DestinationFile;
        public BuildDefinitionEntry Definition;

        public DownloadEntry(string remoteUrl, string partialRemoteUrl, string destinationFolder, string destinationFile, BuildDefinitionEntry definition)
        {
            RemoteUrl = remoteUrl;
            PartialRemoteUrl = partialRemoteUrl;
            DestinationFolder = destinationFolder;
            DestinationFile = destinationFile;
            Definition = definition;
        }

        public override string ToString()
        {
            return PartialRemoteUrl;
        }
    }
}
