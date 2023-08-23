namespace MHLab.Patch.Core.Client.Localization
{
    public interface IUpdaterLocalizedMessages
    {
        string UpdateDownloadingArchive   { get; }
        string UpdateDownloadedArchive    { get; }
        string UpdateDecompressingArchive { get; }
        string UpdateDecompressedArchive  { get; }


        string UpdateUnchangedFile                   { get; }
        string UpdateProcessingNewFile               { get; }
        string UpdateProcessedNewFile                { get; }
        string UpdateProcessingDeletedFile           { get; }
        string UpdateProcessedDeletedFile            { get; }
        string UpdateProcessingUpdatedFile           { get; }
        string UpdateProcessedUpdatedFile            { get; }
        string UpdateProcessingChangedAttributesFile { get; }
        string UpdateProcessedChangedAttributesFile  { get; }

        string UpdateCheckedFile     { get; }
        string UpdateFixedAttributes { get; }
        string UpdateFixedMetadata   { get; }
        string UpdateDownloadingFile { get; }
        string UpdateRepairedFile    { get; }

        string NotAvailableNetwork { get; }
        string NotAvailableServers { get; }
        string LogsFileNotWritable { get; }

        string UpdateProcessProgressing { get; }
        string UpdateProcessCompleted   { get; }
        string UpdateProcessFailed      { get; }
        string UpdateRestartNeeded      { get; }
    }
}