using MHLab.Patch.Core.Client.Exceptions;
using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.Client.Localization;
using MHLab.Patch.Core.Client.Progresses;
using MHLab.Patch.Core.Client.Runners;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Logging;
using MHLab.Patch.Core.Serializing;
using MHLab.Patch.Core.Utilities;
using MHLab.Patch.Core.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MHLab.Patch.Core.Client
{
    public class UpdatingContext
    {
        public BuildsIndex BuildsIndex  { get; set; }
        public PatchIndex  PatchesIndex { get; set; }

        public LocalFileInfo[]                   ExistingFiles    { get; set; }
        public Dictionary<string, LocalFileInfo> ExistingFilesMap { get; set; }

        public  IVersion        CurrentVersion { get; set; }
        private IVersionFactory VersionFactory { get; set; }

        public BuildDefinition       CurrentBuildDefinition   { get; set; }
        public List<PatchDefinition> PatchesPath              { get; set; }
        public UpdaterDefinition     CurrentUpdaterDefinition { get; set; }

        public IUpdateRunner Runner { get; set; }

        public readonly ILauncherSettings Settings;

        public IUpdaterLocalizedMessages LocalizedMessages { get; set; }

        public ILogger     Logger     { get; set; }
        public ISerializer Serializer { get; set; }

        public IDownloader Downloader { get; set; }
        
        public IFileSystem FileSystem { get; set; }

        private readonly IProgress<UpdateProgress> _progressReporter;
        private          UpdateProgress            _progress;

        private bool             _isDirty;
        private List<string>     _dirtyReasons;
        private List<object>     _dirtyData;
        private bool             _isRepairNeeded;
        private SettingsOverride _currentSettingsOverride;

        public UpdatingContext(ILauncherSettings settings, IProgress<UpdateProgress> progress)
        {
            _isDirty          = false;
            _dirtyReasons     = new List<string>();
            _dirtyData        = new List<object>();
            _isRepairNeeded   = false;
            Settings          = settings;
            _progressReporter = progress;
            PatchesPath       = new List<PatchDefinition>();

            VersionFactory = new VersionFactory();

            FileSystem = new FileSystem();
            Runner     = new UpdateRunner();
            Downloader = new FileDownloader(FileSystem);
        }

        public void Initialize()
        {
            Logger.Info("Update context initializing...");
            Logger.Info($"Software version: {Settings.SoftwareVersion}");
            Logger.Info($"Update context points to {Settings.RemoteUrl}");
            Logger.Debug("===> Debug Mode: ENABLED <===");

            _progress = new UpdateProgress();

            SetCurrentVersion();

            var cleanedFiles = CleanWorkspace();
            Logger.Info($"Workspace cleaned. Removed {cleanedFiles} files");

            FileSystem.CreateDirectory((FilePath)Settings.GetTempPath());

            FetchIndexes();

            _progress.TotalSteps = Runner.GetProgressAmount();
            Logger.Info("Update context completed initialization.");
        }

        public void FetchIndexes()
        {
            Task.WaitAll(
                GetUpdaterDefinition(),
                GetBuildsIndex(),
                GetPatchesIndex()
            );

            Task.WaitAll(
                GetLocalFiles(),
                GetBuildDefinition()
            );

            Task.WaitAll(GetPatchesShortestPath());
        }

        public IVersion GetLocalVersion()
        {
            if (LocalVersionExists())
            {
                try
                {
                    var encryptedVersion = FileSystem.ReadAllTextFromFile((FilePath)Settings.GetVersionFilePath());
                    var decryptedVersion = Rijndael.Decrypt(encryptedVersion, Settings.EncryptionKeyphrase);

                    var version = VersionFactory.Create();
                        
                    return Serializer.DeserializeOn(version, decryptedVersion);
                }
                catch (Exception e)
                {
                    Logger.Debug($"Cannot retrieve local version: {e.Message} - {e.StackTrace}");
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public bool LocalVersionExists()
        {
            return FileSystem.FileExists((FilePath)Settings.GetVersionFilePath());
        }
        
        public void Update()
        {
            Runner.Update();
        }

        public void RegisterUpdateStep(IUpdater updater)
        {
            Runner.RegisterStep(updater);
        }

        private void SetCurrentVersion()
        {
            if (LocalVersionExists())
            {
                try
                {
                    var encryptedVersion = FileSystem.ReadAllTextFromFile((FilePath)Settings.GetVersionFilePath());
                    var decryptedVersion = Rijndael.Decrypt(encryptedVersion, Settings.EncryptionKeyphrase);
                    
                    CurrentVersion = VersionFactory.Create();
                    Serializer.DeserializeOn(CurrentVersion, decryptedVersion);
                    Logger.Info($"Retrieved current version: {CurrentVersion}");
                }
                catch (Exception e)
                {
                    CurrentVersion = null;
                    Logger.Warning($"Current version file cannot be read. Error: {e.Message}");
                }
            }
            else
            {
                CurrentVersion = null;
                Logger.Warning("No current version found. A full repair may be required.");
            }
        }

        private int CleanWorkspace()
        {
            return FileSystem.DeleteMultipleFiles((FilePath)Settings.RootPath, "*.delete_tmp");
        }

        private Task GetUpdaterDefinition()
        {
            return Task.Run(() =>
            {
                try
                {
                    var remoteUrl        = Settings.GetRemoteUpdaterIndexUrl();
                    var partialRemoteUrl = Settings.GetRemoteUpdaterIndexUrl();
                    if (!string.IsNullOrWhiteSpace(Settings.RemoteUrl))
                        partialRemoteUrl = partialRemoteUrl.Replace(Settings.RemoteUrl, string.Empty);
                    var downloadEntry = new DownloadEntry(
                        remoteUrl,
                        partialRemoteUrl,
                        null,
                        null,
                        null
                    );
                    CurrentUpdaterDefinition =
                        Downloader.DownloadJson<UpdaterDefinition>(downloadEntry, Serializer);
                }
                catch (Exception e)
                {
                    CurrentUpdaterDefinition = null;
                    Logger.Warning(
                        $"No updater definition found on remote server. The Launcher update will be skipped. Problem reference: {e.Message}");
                }
            });
        }

        private Task GetBuildsIndex()
        {
            return Task.Run(() =>
            {
                try
                {
                    var remoteUrl        = Settings.GetRemoteBuildsIndexUrl();
                    var partialRemoteUrl = Settings.GetRemoteBuildsIndexUrl();
                    if (!string.IsNullOrWhiteSpace(Settings.RemoteUrl))
                        partialRemoteUrl = partialRemoteUrl.Replace(Settings.RemoteUrl, string.Empty);

                    var downloadEntry = new DownloadEntry(
                        remoteUrl,
                        partialRemoteUrl,
                        null,
                        null,
                        null
                    );
                    BuildsIndex = Downloader.DownloadJson<BuildsIndex>(downloadEntry, Serializer);
                }
                catch (Exception e)
                {
                    BuildsIndex = new BuildsIndex()
                    {
                        AvailableBuilds = new List<IVersion>()
                    };
                    Logger.Warning(
                        $"No builds index found on remote server. The Repair process will be skipped. Problem reference: {e}");
                }
            });
        }

        private Task GetPatchesIndex()
        {
            return Task.Run(() =>
            {
                try
                {
                    var remoteUrl        = Settings.GetRemotePatchesIndexUrl();
                    var partialRemoteUrl = Settings.GetRemotePatchesIndexUrl();
                    if (!string.IsNullOrWhiteSpace(Settings.RemoteUrl))
                        partialRemoteUrl = partialRemoteUrl.Replace(Settings.RemoteUrl, string.Empty);

                    var downloadEntry = new DownloadEntry(
                        remoteUrl,
                        partialRemoteUrl,
                        null,
                        null,
                        null
                    );
                    PatchesIndex = Downloader.DownloadJson<PatchIndex>(downloadEntry, Serializer);
                }
                catch
                {
                    PatchesIndex = new PatchIndex()
                    {
                        Patches = new List<PatchIndexEntry>()
                    };
                    Logger.Warning("No patches index found on the remote server. The Update process will be skipped.");
                }
            });
        }

        private Task GetLocalFiles()
        {
            return Task.Run(() =>
            {
                FileSystem.GetFilesInfo((FilePath)Settings.RootPath, out var existingFiles, out var existingFilesMap);
                ExistingFiles    = existingFiles;
                ExistingFilesMap = existingFilesMap;
                Logger.Info($"Collected information on {ExistingFiles.Length} local files.");
            });
        }

        private Task GetBuildDefinition()
        {
            return Task.Run(() =>
            {
                if (CurrentVersion == null)
                {
                    CurrentVersion = BuildsIndex.GetLast();
                }

                if (CurrentVersion == null)
                {
                    Logger.Error(null, "Cannot retrieve any new version...");
                    throw new NoAvailableBuildsException();
                }

                if (!BuildsIndex.Contains(CurrentVersion) && CurrentVersion.IsLower(BuildsIndex.GetFirst()))
                {
                    CurrentVersion = BuildsIndex.GetLast();
                    SetRepairNeeded();
                }

                try
                {
                    CurrentBuildDefinition = GetBuildDefinition(CurrentVersion);
                    Logger.Info($"Retrieved definition for {CurrentVersion}");
                }
                catch
                {
                    CurrentBuildDefinition = new BuildDefinition()
                    {
                        Entries = new BuildDefinitionEntry[0]
                    };
                    Logger.Warning($"Cannot retrieve the build definition for {CurrentVersion} on the remote server.");
                }
            });
        }

        public BuildDefinition GetBuildDefinition(IVersion version)
        {
            var remoteUrl        = Settings.GetRemoteBuildDefinitionUrl(version);
            var partialRemoteUrl = Settings.GetRemoteBuildDefinitionUrl(version);
            if (!string.IsNullOrWhiteSpace(Settings.RemoteUrl))
                partialRemoteUrl = partialRemoteUrl.Replace(Settings.RemoteUrl, string.Empty);
            var downloadEntry = new DownloadEntry(
                remoteUrl,
                partialRemoteUrl,
                null,
                null,
                null
            );
            
            return Downloader.DownloadJson<BuildDefinition>(downloadEntry, Serializer);
        }

        private Task GetPatchesShortestPath()
        {
            return Task.Run(() =>
            {
                var                   currentVersion    = CurrentVersion;
                List<PatchIndexEntry> compatiblePatches = null;
                do
                {
                    var version = currentVersion;
                    compatiblePatches = PatchesIndex.Patches.Where(p => p.From.Equals(version)).ToList();
                    if (compatiblePatches.Count == 0) continue;

                    var longestJumpPatch = compatiblePatches.OrderBy(p => p.To).Last();
                    var remoteUrl = Settings.GetRemotePatchDefinitionUrl(longestJumpPatch.From, longestJumpPatch.To);
                    var partialRemoteUrl =
                        Settings.GetRemotePatchDefinitionUrl(longestJumpPatch.From, longestJumpPatch.To);
                    if (!string.IsNullOrWhiteSpace(Settings.RemoteUrl))
                        partialRemoteUrl = partialRemoteUrl.Replace(Settings.RemoteUrl, string.Empty);
                    var downloadEntry = new DownloadEntry(
                        remoteUrl,
                        partialRemoteUrl,
                        null,
                        null,
                        null
                    );
                    PatchesPath.Add(Downloader.DownloadJson<PatchDefinition>(downloadEntry, Serializer));
                    currentVersion = longestJumpPatch.To;
                } while (compatiblePatches.Count > 0);

                Logger.Info($"Found {PatchesPath.Count} applicable updates.");
            });
        }

        public void ReportProgress(string log, long increment)
        {
            _progress.IncrementStep(increment);
            _progress.StepMessage = log;

            _progressReporter.Report(new UpdateProgress()
            {
                CurrentSteps = _progress.CurrentSteps,
                StepMessage  = log,
                TotalSteps   = _progress.TotalSteps
            });
        }

        public void ReportProgress(long increment)
        {
            _progress.IncrementStep(increment);

            _progressReporter.Report(new UpdateProgress()
            {
                CurrentSteps = _progress.CurrentSteps,
                StepMessage  = _progress.StepMessage,
                TotalSteps   = _progress.TotalSteps
            });
        }

        public void LogProgress(string log)
        {
            _progress.StepMessage = log;

            _progressReporter.Report(new UpdateProgress()
            {
                CurrentSteps = _progress.CurrentSteps,
                StepMessage  = log,
                TotalSteps   = _progress.TotalSteps
            });
        }

        public void SetDirtyFlag(string reason, object data = null)
        {
            _dirtyReasons.Add(reason);
            if (data != null) _dirtyData.Add(data);
            _isDirty = true;
        }

        public bool IsDirty(out List<string> reasons, out List<object> data)
        {
            reasons = _dirtyReasons;
            data    = _dirtyData;
            return _isDirty;
        }

        public void SetRepairNeeded()
        {
            _isRepairNeeded = true;
        }

        public bool IsRepairNeeded()
        {
            return _isRepairNeeded;
        }

        public void OverrideSettings<TSettings>(Action<ILauncherSettings, TSettings> settingsOverrider)
            where TSettings : SettingsOverride
        {
            var filePath = Settings.GetSettingsOverridePath();
            if (!FileSystem.FileExists(filePath)) return;

            var content          = FileSystem.ReadAllTextFromFile(filePath);
            var settingsOverride = Serializer.Deserialize<TSettings>(content);

            _currentSettingsOverride = settingsOverride;

            settingsOverrider.Invoke(Settings, settingsOverride);
        }

        public void DisableSafeMode()
        {
            var settingsOverride = _currentSettingsOverride;

            settingsOverride.PatcherUpdaterSafeMode = false;

            var filePath = Settings.GetSettingsOverridePath();
            FileSystem.DeleteFile(filePath);
            FileSystem.WriteAllTextToFile(filePath, Serializer.Serialize(settingsOverride));
        }
    }
}