using MHLab.Patch.Core.IO;
using System;

namespace MHLab.Patch.Core
{
    public interface ISettings
    {
        string RootPath { get; set; }
        string AppDataPath { get; set; }
        
        string SoftwareVersion { get; }

        string GameFolderName { get; set; }
        string EncryptionKeyphrase { get; set; }

        string BuildsFolderName { get; set; }
        string PatchesFolderName { get; set; }
        string UpdaterFolderName { get; set; }
        string LogsFolderName { get; set; }

        string TempFolderName { get; set; }

        string VersionFileName { get; set; }
        string BuildsIndexFileName { get; set; }
        string BuildDefinitionFileName { get; set; }
        string PatchesIndexFileName { get; set; }
        string PatchFileName { get; set; }
        string PatchDefinitionFileName { get; set; }
        string UpdaterIndexFileName { get; set; }
        string UpdaterSafeModeIndexFileName { get; set; }

        string LogsFileName { get; set; }

        string LaunchArgumentParameter { get; set; }
        string LaunchArgumentValue { get; set; }

        string GetLogsFilePath();
        string GetLogsDirectoryPath();

        bool DebugMode { get; set; }
        string ToDebugString();
    }

    [Serializable]
    public class Settings : ISettings
    {
        public string RootPath { get; set; }
        public string AppDataPath { get; set; }

        public string SoftwareVersion => "2.6.2";

        public string GameFolderName { get; set; } = "Game";
        public string EncryptionKeyphrase { get; set; } = "dwqqe2231ffe32";

        public string BuildsFolderName { get; set; } = "Builds";
        public string PatchesFolderName { get; set; } = "Patches";
        public string UpdaterFolderName { get; set; } = "Updater";
        public string LogsFolderName { get; set; } = "Logs";

        public string TempFolderName { get; set; } = "Temp";

        public string VersionFileName { get; set; } = "version.data";
        public string BuildsIndexFileName { get; set; } = "builds_index.json";
        public string BuildDefinitionFileName { get; set; } = "build_{0}.json";
        public string PatchesIndexFileName { get; set; } = "patches_index.json";
        public string PatchFileName { get; set; } = "{0}_{1}.zip";
        public string PatchDefinitionFileName { get; set; } = "{0}_{1}.json";
        public string UpdaterIndexFileName { get; set; } = "updater_index.json";
        public string UpdaterSafeModeIndexFileName { get; set; } = "updater_safemode_index.json";

        public string LogsFileName { get; set; } = "logs-{0}.txt";

        public string LaunchArgumentParameter { get; set; } = "--launchArgument";
        public string LaunchArgumentValue { get; set; } = "Qjshn1k!ncS_298";

        public virtual string GetLogsFilePath() => PathsManager.Combine(RootPath, LogsFolderName, string.Format(LogsFileName, $"{DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}"));
        public virtual string GetLogsDirectoryPath() => PathsManager.Combine(RootPath, LogsFolderName);
        
        public bool DebugMode { get; set; }

        public Settings()
        {
            
        }

        public virtual string ToDebugString()
        {
            var result = $"Debug Mode => {DebugMode}\n" +
                         $"RootPath => {RootPath}\n" +
                         $"AppDataPath => {AppDataPath}" +
                         $"SoftwareVersion => {SoftwareVersion}";
            
            result += $"GetLogsFilePath() => {GetLogsFilePath()}\n";

            return result;
        }
    }
}
