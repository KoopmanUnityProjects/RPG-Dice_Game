using System;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Versioning;

namespace MHLab.Patch.Core.Admin
{
    public interface IAdminSettings : ISettings
    {
        string ApplicationFolderName { get; set; }

        string   GetApplicationFolderPath();
        string   GetBuildsFolderPath();
        FilePath GetBuildsIndexPath();
        string   GetPatchesFolderPath();
        string   GetPatchesTempFolderPath();
        FilePath GetPatchIndexPath(IVersion from, IVersion to);
        FilePath GetPatchesIndexPath();
        FilePath GetBuildDefinitionPath(IVersion version);
        string   GetGameFolderPath(IVersion      version);
        FilePath GetVersionFilePath(IVersion     version);
        string   GetUpdaterFolderPath();
        FilePath GetUpdaterIndexPath();
        string   GetUpdaterDeployPath(string fileName);

        string GetDebugReportFilePath();
    }

    [Serializable]
    public class AdminSettings : Settings, IAdminSettings
    {
        public string ApplicationFolderName { get; set; } = "App";

        public virtual string GetApplicationFolderPath() => PathsManager.Combine(RootPath, ApplicationFolderName);
        public virtual string GetBuildsFolderPath()      => PathsManager.Combine(RootPath, BuildsFolderName);

        public virtual FilePath GetBuildsIndexPath() =>
            new FilePath(RootPath, PathsManager.Combine(RootPath, BuildsFolderName, BuildsIndexFileName));

        public virtual string GetPatchesFolderPath() => PathsManager.Combine(RootPath, PatchesFolderName);

        public virtual string GetPatchesTempFolderPath() =>
            PathsManager.Combine(RootPath, PatchesFolderName, TempFolderName);

        public virtual FilePath GetPatchIndexPath(IVersion from, IVersion to) => new FilePath(
            RootPath,
            PathsManager.Combine(RootPath, PatchesFolderName, string.Format(PatchDefinitionFileName, from, to)));

        public virtual FilePath GetPatchesIndexPath() =>
            new FilePath(RootPath, PathsManager.Combine(RootPath, PatchesFolderName, PatchesIndexFileName));

        public virtual FilePath GetBuildDefinitionPath(IVersion version) => new FilePath(
            RootPath,
            PathsManager.Combine(RootPath, BuildsFolderName,
                                 string.Format(BuildDefinitionFileName, version.ToString())));

        public virtual string GetGameFolderPath(IVersion version) =>
            PathsManager.Combine(RootPath, BuildsFolderName, version.ToString(), GameFolderName);

        public virtual FilePath GetVersionFilePath(IVersion version) => new FilePath(
            GetGameFolderPath(version), PathsManager.Combine(GetGameFolderPath(version), VersionFileName));

        public virtual string GetUpdaterFolderPath() => PathsManager.Combine(RootPath, UpdaterFolderName);

        public virtual FilePath GetUpdaterIndexPath() => new FilePath(RootPath, PathsManager.Combine(RootPath, UpdaterFolderName, UpdaterIndexFileName));

        public virtual string GetUpdaterDeployPath(string fileName) =>
            PathsManager.Combine(RootPath, fileName + ".zip");

        public virtual string GetDebugReportFilePath() => PathsManager.Combine(RootPath, "debug_report.txt");

        public AdminSettings() : base()
        {
        }

        public override string ToDebugString()
        {
            var result = base.ToDebugString();
            return result + $"GetApplicationFolderPath() => {GetApplicationFolderPath()}\n" +
                   $"GetBuildsFolderPath() => {GetBuildsFolderPath()}\n" +
                   $"GetPatchesFolderPath() => {GetPatchesFolderPath()}\n" +
                   $"GetPatchesTempFolderPath() => {GetPatchesTempFolderPath()}\n" +
                   $"GetUpdaterFolderPath() => {GetUpdaterFolderPath()}\n" +
                   $"GetDebugReportFilePath() => {GetDebugReportFilePath()}\n";
        }
    }
}