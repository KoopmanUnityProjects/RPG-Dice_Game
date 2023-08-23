using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using MHLab.Patch.Core.Client.Utilities;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Utilities.Asserts;

namespace MHLab.Patch.Core.Client
{
    public class Repairer : IUpdater
    {
        [Flags]
        private enum FileIntegrity
        {
            None = 0,
            Valid = 1,
            NotExisting = 1 << 1,
            InvalidSize = 1 << 2,
            InvalidLastWriting = 1 << 3,
            InvalidAttributes = 1 << 4
        }

        private readonly UpdatingContext _context;

        public Repairer(UpdatingContext context)
        {
            _context = context;
        }

        public void Update()
        {            
            _context.Logger.Info("Repairing process started.");
            var repairedFiles = 0;
            var downloadEntries = new List<DownloadEntry>();

            var sizeToRepair = 0L;

            foreach (var currentEntry in _context.CurrentBuildDefinition.Entries)
            {
                var canSkip = false;
                var integrity = GetFileIntegrity(currentEntry);
                var filePath = _context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), currentEntry.RelativePath);

                if (integrity == FileIntegrity.Valid)
                {
                    canSkip = true;
                    _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateCheckedFile, currentEntry.RelativePath), currentEntry.Size);
                }
                else if (integrity == FileIntegrity.InvalidAttributes)
                {
                    HandleInvalidAttributes(currentEntry);
                    canSkip = true;
                    _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateFixedAttributes, currentEntry.RelativePath), currentEntry.Size);
                }
                else if (integrity == FileIntegrity.InvalidLastWriting || integrity == (FileIntegrity.InvalidLastWriting | FileIntegrity.InvalidAttributes))
                {
                    var isNowValid = HandleInvalidLastWriting(currentEntry);
                    if (isNowValid)
                    {
                        canSkip = true;
                        _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateFixedMetadata, currentEntry.RelativePath), currentEntry.Size);
                    }
                }
                else if (integrity.HasFlag(FileIntegrity.InvalidSize))
                {
                    _context.FileSystem.DeleteFile(filePath);
                }

                if (!canSkip)
                {
                    // If I am here, the file cannot be fixed and it does not exist anymore (or never existed)
                    _context.FileSystem.CreateDirectory(_context.FileSystem.GetDirectoryPath(filePath));

                    var remoteFile = _context.FileSystem.CombineUri(
                        _context.Settings.GetRemoteBuildUrl(_context.CurrentVersion), 
                        _context.Settings.GameFolderName, 
                        currentEntry.RelativePath
                    );
                    var partialRemoteFile = _context.FileSystem.CombineUri(
                        _context.Settings.GetPartialRemoteBuildUrl(_context.CurrentVersion),
                        _context.Settings.GameFolderName,
                        currentEntry.RelativePath
                    );
                    downloadEntries.Add(new DownloadEntry(
                        remoteFile.FullPath,
                        partialRemoteFile.FullPath,
                        _context.FileSystem.GetDirectoryPath(filePath).FullPath, 
                        filePath.FullPath, 
                        currentEntry)
                    );

                    sizeToRepair += currentEntry.Size;
                    repairedFiles++;
                }
            }

            if (_context.Settings.EnableDiskSpaceCheck)
            {
                var availableDiskSpace = _context.FileSystem.GetAvailableDiskSpace((FilePath)_context.Settings.RootPath);

                if (sizeToRepair > availableDiskSpace)
                    throw new Exception(
                        $"Not enough disk space for repairing. Required space [{sizeToRepair}], available space [{availableDiskSpace}]"
                        );
            }
            
            _context.Downloader.Download(downloadEntries,
                (entry) =>
                {
                    _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateDownloadingFile, entry.Definition.RelativePath));
                },
                (size) =>
                {
                    _context.ReportProgress(size);
                },
                (entry) =>
                {
                    SetDefinition((FilePath)entry.DestinationFile, entry.Definition);
                    _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateRepairedFile, entry.Definition.RelativePath));
                });

            if (_context.Settings.DebugMode)
            {
                Assert.AlwaysCheck(ValidatorHelper.ValidateLocalFiles(_context.CurrentBuildDefinition, _context.FileSystem, _context.Logger, _context.Settings.GetGamePath()));
            }

            _context.Logger.Info($"Repairing process completed. Checked {_context.CurrentBuildDefinition.Entries.Length} files, repaired {repairedFiles} files, skipped {_context.CurrentBuildDefinition.Entries.Length - repairedFiles} files.");
        }

        public long ProgressRangeAmount()
        {
            var accumulator = 0L;
            foreach (var buildDefinitionEntry in _context.CurrentBuildDefinition.Entries)
            {
                accumulator += buildDefinitionEntry.Size;
            }
            return accumulator;
        }

        private FileIntegrity GetFileIntegrity(BuildDefinitionEntry entry)
        {
            var entryFilePath = _context.FileSystem.SanitizePath(_context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath)).FullPath;
            entryFilePath = entryFilePath.Replace(_context.FileSystem.SanitizePath((FilePath)_context.Settings.RootPath).FullPath, "");

            if (entryFilePath.StartsWith("/"))
                entryFilePath = entryFilePath.Substring(1);

            if (_context.ExistingFilesMap.TryGetValue(entryFilePath, out var existingFile))
            {
                var integrity = FileIntegrity.None;

                if (existingFile.Size != entry.Size) integrity |= FileIntegrity.InvalidSize;
                if (!AreLastWritingsEqual(existingFile.LastWriting, entry.LastWriting)) integrity |= FileIntegrity.InvalidLastWriting;
                if (existingFile.Attributes != entry.Attributes) integrity |= FileIntegrity.InvalidAttributes;

                if (integrity == FileIntegrity.None) return FileIntegrity.Valid;
                return integrity;
            }

            return FileIntegrity.NotExisting;
        }

        private FileIntegrity GetRelaxedFileIntegrity(BuildDefinitionEntry entry)
        {
            var entryFilePath = _context.FileSystem.SanitizePath(_context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath)).FullPath;
            entryFilePath = entryFilePath.Replace(_context.Settings.RootPath, "");

            if (entryFilePath.StartsWith("/"))
                entryFilePath = entryFilePath.Substring(1);

            if (_context.ExistingFilesMap.TryGetValue(entryFilePath, out var existingFile))
            {
                var integrity = FileIntegrity.None;

                if (existingFile.Size != entry.Size) integrity |= FileIntegrity.InvalidSize;

                if (integrity == FileIntegrity.None) return FileIntegrity.Valid;
                return integrity;
            }

            return FileIntegrity.NotExisting;
        }

        private bool AreLastWritingsEqual(DateTime lastWriting1, DateTime lastWriting2)
        {
            if (lastWriting1.Year != lastWriting2.Year) return false;
            if (lastWriting1.Month != lastWriting2.Month) return false;
            if (lastWriting1.Day != lastWriting2.Day) return false;
            if (lastWriting1.Hour != lastWriting2.Hour) return false;
            if (lastWriting1.Minute != lastWriting2.Minute) return false;

            return true;
        }

        private void HandleInvalidAttributes(BuildDefinitionEntry entry)
        {
            var filePath = _context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath);
            SetDefinition(filePath, entry);
        }

        private bool HandleInvalidLastWriting(BuildDefinitionEntry entry)
        {
            var filePath = _context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath);
            var hash     = Hashing.GetFileHash(filePath.FullPath, _context.FileSystem);

            if (entry.Hash != hash)
            {
                _context.FileSystem.DeleteFile(filePath);
                return false;
            }
            
            SetDefinition(filePath, entry);
            return true;
        }

        private void SetDefinition(FilePath filePath, BuildDefinitionEntry currentEntry)
        {
            File.SetAttributes(filePath.FullPath, currentEntry.Attributes);
            File.SetLastWriteTimeUtc(filePath.FullPath, currentEntry.LastWriting);
            File.SetLastWriteTime(filePath.FullPath, currentEntry.LastWriting);
        }

        public bool IsRepairNeeded()
        {
            if (_context.IsRepairNeeded()) return true;

            foreach (var currentEntry in _context.CurrentBuildDefinition.Entries)
            {
                var integrity = GetRelaxedFileIntegrity(currentEntry);
                if (integrity != FileIntegrity.Valid)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
