using System;
using System.Collections.Generic;
using MHLab.Patch.Core.Admin.Localization;
using MHLab.Patch.Core.Admin.Progresses;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Logging;
using MHLab.Patch.Core.Serializing;
using MHLab.Patch.Core.Versioning;

namespace MHLab.Patch.Core.Admin
{
    public class AdminPatchContext
    {
        public IVersion VersionFrom { get; set; }
        public IVersion VersionTo { get; set; }

        public string PatchName { get; set; }

        public readonly IAdminSettings Settings;

        private int _compressionLevel;
        public int CompressionLevel
        {
            get => _compressionLevel;

            set
            {
                if (value < 0) _compressionLevel = 0;
                else if (value > 9) _compressionLevel = 9;
                else _compressionLevel = value;
            }
        }

        public IAdminLocalizedMessages LocalizedMessages { get; set; }

        public ILogger Logger { get; set; }
        public ISerializer Serializer { get; set; }
        public IVersionFactory VersionFactory { get; set; }
        
        public IFileSystem FileSystem { get; set; }

        private readonly IProgress<BuilderProgress> _progressReporter;
        private BuilderProgress _progress;

        public AdminPatchContext(IAdminSettings settings, IProgress<BuilderProgress> progress)
        {
            Settings          = settings;
            _progressReporter = progress;
            VersionFactory    = new VersionFactory();
            FileSystem        = new FileSystem();
        }

        public void Initialize()
        {
            _progress = new BuilderProgress();

            InitializeDirectories();

            PatchName = string.Format(Settings.PatchFileName, VersionFrom, VersionTo);

            var fromDefinition = GetBuildDefinition(VersionFrom);
            var toDefinition = GetBuildDefinition(VersionTo);

            _progress.TotalSteps = 4 + fromDefinition.Entries.Length + toDefinition.Entries.Length + Math.Max(fromDefinition.Entries.Length, toDefinition.Entries.Length);
        }

        private void InitializeDirectories()
        {
            FileSystem.CreateDirectory((FilePath)Settings.GetApplicationFolderPath());
            FileSystem.CreateDirectory((FilePath)Settings.GetBuildsFolderPath());
            FileSystem.CreateDirectory((FilePath)Settings.GetPatchesFolderPath());
        }

        private BuildDefinition GetBuildDefinition(IVersion version)
        {
            var content = FileSystem.ReadAllTextFromFile(Settings.GetBuildDefinitionPath(version));
            return Serializer.Deserialize<BuildDefinition>(content);
        }

        public List<IVersion> GetVersions()
        {
            var buildsIndexPath = Settings.GetBuildsIndexPath();
            
            if (FileSystem.FileExists(buildsIndexPath))
            {
                var readData = FileSystem.ReadAllTextFromFile(buildsIndexPath);
                var index    = Serializer.Deserialize<BuildsIndex>(readData);
                return index.AvailableBuilds;
            }

            return new List<IVersion>();
        }

        public void ReportProgress(string log)
        {
            _progress.CurrentSteps++;

            _progressReporter.Report(new BuilderProgress()
            {
                CurrentSteps = _progress.CurrentSteps,
                StepMessage = log,
                TotalSteps = _progress.TotalSteps
            });
        }

        public void LogProgress(string log)
        {
            _progressReporter.Report(new BuilderProgress()
            {
                CurrentSteps = _progress.CurrentSteps,
                StepMessage = log,
                TotalSteps = _progress.TotalSteps
            });
        }
    }
}
