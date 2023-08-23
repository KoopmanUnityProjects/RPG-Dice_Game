namespace MHLab.Patch.Core.Client.IO
{
    public interface IDownloadSpeedMeter
    {
        string FormattedDownloadSpeed { get; }
        long DownloadSpeed { get; }
        void UpdateDownloadSpeed(long additionalDownloadedSize);
        void Tick();
        void Reset();
    }
}
