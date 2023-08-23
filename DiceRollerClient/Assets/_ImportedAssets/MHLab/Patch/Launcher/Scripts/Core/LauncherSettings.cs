using System;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Versioning;

namespace MHLab.Patch.Core.Client
{
    public interface ILauncherSettings : ISettings
    {
        int      PatchDownloadAttempts { get; set; }
        string   GetVersionFilePath();
        string   GetGamePath();
        string   GetTempPath();
        FilePath GetSettingsOverridePath();
        string   GetDownloadedPatchArchivePath(IVersion   from, IVersion to);
        string   GetUncompressedPatchArchivePath(IVersion from, IVersion to);
        string   GetUpdaterSafeModeTempPath();
        string   GetUpdaterSafeModeBackupPath();
        FilePath GetUpdaterSafeModeLockFilePath();

        string RemoteUrl { get; set; }
        string GetRemoteBuildsIndexUrl();
        string GetRemoteBuildDefinitionUrl(IVersion version);
        string GetRemoteBuildUrl(IVersion version);
        string GetPartialRemoteBuildUrl(IVersion version);
        string GetRemotePatchesIndexUrl();
        string GetRemotePatchDefinitionUrl(IVersion from, IVersion to);
        string GetRemotePatchArchiveUrl(IVersion from, IVersion to);
        string GetRemoteUpdaterIndexUrl();
        string GetRemoteUpdaterSafeModeIndexUrl();
        string GetRemoteUpdaterSafeModeArchiveUrl(string archiveName);
        string GetRemoteUpdaterFileUrl(string relativePath);
        
        bool PatcherUpdaterSafeMode { get; set; }
        bool EnableDiskSpaceCheck   { get; set; }
    }

    [Serializable]
    public class LauncherSettings : Settings, ILauncherSettings
    {
        public int PatchDownloadAttempts { get; set; } = 3;
        public virtual string GetVersionFilePath() => PathsManager.Combine(RootPath, GameFolderName, VersionFileName);
        public virtual string GetGamePath() => PathsManager.Combine(RootPath, GameFolderName);
        public virtual string GetTempPath() => PathsManager.Combine(AppDataPath, TempFolderName);
        public virtual FilePath GetSettingsOverridePath() => new FilePath(RootPath, PathsManager.Combine(RootPath, "settings.json"));
        public virtual string GetDownloadedPatchArchivePath(IVersion from, IVersion to) => PathsManager.Combine(GetTempPath(), string.Format(PatchFileName, from, to));
        public virtual string GetUncompressedPatchArchivePath(IVersion from, IVersion to) => PathsManager.Combine(GetTempPath(), string.Format("{0}_{1}_uncompressed", from, to));
        public virtual string GetUpdaterSafeModeTempPath() => PathsManager.Combine(GetTempPath(), "UpdaterSafeMode");
        public virtual string GetUpdaterSafeModeBackupPath() => PathsManager.Combine(GetTempPath(), "UpdaterSafeModeBackup");
        public virtual FilePath GetUpdaterSafeModeLockFilePath() => new FilePath(GetTempPath(), PathsManager.Combine(GetTempPath(), "patch_tmp_safemode_lock"));

        public string RemoteUrl { get; set; } = "http://localhost/patch";
        public virtual string GetRemoteBuildsIndexUrl() => PathsManager.UriCombine(RemoteUrl, BuildsFolderName, BuildsIndexFileName);
        public virtual string GetRemoteBuildDefinitionUrl(IVersion version) => PathsManager.UriCombine(RemoteUrl, BuildsFolderName, string.Format(BuildDefinitionFileName, version));
        public virtual string GetRemoteBuildUrl(IVersion version) => PathsManager.UriCombine(RemoteUrl, BuildsFolderName, version.ToString());
        public virtual string GetPartialRemoteBuildUrl(IVersion version) => PathsManager.UriCombine(BuildsFolderName, version.ToString());
        public virtual string GetRemotePatchesIndexUrl() => PathsManager.UriCombine(RemoteUrl, PatchesFolderName, PatchesIndexFileName);
        public virtual string GetRemotePatchDefinitionUrl(IVersion from, IVersion to) => PathsManager.UriCombine(RemoteUrl, PatchesFolderName, string.Format(PatchDefinitionFileName, from, to));
        public virtual string GetRemotePatchArchiveUrl(IVersion from, IVersion to) => PathsManager.UriCombine(RemoteUrl, PatchesFolderName, string.Format(PatchFileName, from, to));
        public virtual string GetRemoteUpdaterIndexUrl() => PathsManager.UriCombine(RemoteUrl, UpdaterFolderName, UpdaterIndexFileName);
        public virtual string GetRemoteUpdaterSafeModeIndexUrl() => PathsManager.UriCombine(RemoteUrl, UpdaterFolderName, UpdaterSafeModeIndexFileName);
        public virtual string GetRemoteUpdaterSafeModeArchiveUrl(string archiveName) => PathsManager.UriCombine(RemoteUrl, UpdaterFolderName, archiveName);
        public virtual string GetRemoteUpdaterFileUrl(string relativePath) => PathsManager.UriCombine(RemoteUrl, UpdaterFolderName, relativePath);

        public bool PatcherUpdaterSafeMode { get; set; } = false;

        public bool EnableDiskSpaceCheck { get; set; } = true;
        
        public LauncherSettings() : base()
        {

        }

        public override string ToDebugString()
        {
            var result = base.ToDebugString();

            result += $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n" +
                      $"Remote URL => {RemoteUrl}\n";
            
            return result + $"GetVersionFilePath() => {GetVersionFilePath()}\n" +
                   $"GetGamePath() => {GetGamePath()}\n" +
                   $"GetTempPath() => {GetTempPath()}\n" +
                   $"GetRemoteBuildsIndexUrl() => {GetRemoteBuildsIndexUrl()}\n" +
                   $"GetRemotePatchesIndexUrl() => {GetRemotePatchesIndexUrl()}\n" +
                   $"GetRemoteUpdaterIndexUrl() => {GetRemoteUpdaterIndexUrl()}\n" +
                   $"GetRemoteUpdaterSafeModeIndexUrl() => {GetRemoteUpdaterSafeModeIndexUrl()}\n" +
                   $"PatcherUpdaterSafeMode => {PatcherUpdaterSafeMode}\n";
        }
    }
}
