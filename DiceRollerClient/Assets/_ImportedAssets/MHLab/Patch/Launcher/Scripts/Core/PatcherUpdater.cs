using System;
using System.Linq;
using MHLab.Patch.Core.Client.IO;
using MHLab.Patch.Core.Compressing;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Utilities;

namespace MHLab.Patch.Core.Client
{
    public class PatcherUpdater : IUpdater
    {
        [Flags]
        private enum FileValidityDifference
        {
            None = 0,
            Size = 1,
            LastWriting = 1 << 1,
            Attributes = 1 << 2,
            Hash = 1 << 3,
        }

        private readonly UpdatingContext _context;

        public PatcherUpdater(UpdatingContext context)
        {
            _context = context;
        }

        public void Update()
        {
            if (_context.CurrentUpdaterDefinition == null)
            {
                _context.Logger.Warning("No updater definition found. The Launcher cannot be validated or updated.");
                return;
            }

            _context.Logger.Info($"Launcher update started. The update contains {_context.CurrentUpdaterDefinition.Entries.Length} operations.");

            _context.FileSystem.DeleteTemporaryDeletingFiles((FilePath)_context.Settings.RootPath);

            CheckAvailableDiskSpace();

            if (_context.Settings.PatcherUpdaterSafeMode)
            {
                _context.Logger.Info("Launcher update SAFE MODE: ENABLED");
                if (!HandleSafeModeUpdate())
                    HandleUpdate();
                    
            }
            else
            {
                HandleUpdate();
            }
            
            _context.Logger.Info("Launcher update completed.");
        }
        
        private void CheckAvailableDiskSpace()
        {
            var requiredSpace = 0L;
            
            foreach (var updaterDefinitionEntry in _context.CurrentUpdaterDefinition.Entries)
            {
                requiredSpace += updaterDefinitionEntry.Size;
            }

            if (_context.Settings.EnableDiskSpaceCheck)
            {
                var availableDiskSpace =
                    _context.FileSystem.GetAvailableDiskSpace((FilePath)_context.Settings.RootPath);

                if (requiredSpace > availableDiskSpace)
                    throw new Exception(
                        $"Not enough disk space for the Launcher update. Required space [{requiredSpace}], available space [{availableDiskSpace}]"
                    );
            }
        }

        private void HandleUpdate()
        {
            foreach (var updaterDefinitionEntry in _context.CurrentUpdaterDefinition.Entries)
            {
                switch (updaterDefinitionEntry.Operation)
                {
                    case PatchOperation.Added:
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingNewFile, updaterDefinitionEntry.RelativePath));
                        HandleAddedFile(updaterDefinitionEntry);
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessedNewFile, updaterDefinitionEntry.RelativePath));
                        continue;
                    case PatchOperation.Deleted:
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingDeletedFile, updaterDefinitionEntry.RelativePath));
                        HandleDeletedFile(updaterDefinitionEntry);
                        _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateProcessedDeletedFile, updaterDefinitionEntry.RelativePath), updaterDefinitionEntry.Size);
                        continue;
                    case PatchOperation.ChangedAttributes:
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingChangedAttributesFile, updaterDefinitionEntry.RelativePath));
                        HandleChangedAttributesFile(updaterDefinitionEntry);
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessedChangedAttributesFile, updaterDefinitionEntry.RelativePath));
                        continue;
                    case PatchOperation.Updated:
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingUpdatedFile, updaterDefinitionEntry.RelativePath));
                        HandleUpdatedFile(updaterDefinitionEntry);
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessedUpdatedFile, updaterDefinitionEntry.RelativePath));
                        continue;
                    case PatchOperation.Unchanged:
                        HandleUnchangedFile(updaterDefinitionEntry);
                        _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateUnchangedFile, updaterDefinitionEntry.RelativePath));
                        continue;
                }
            }
        }

        private bool HandleSafeModeUpdate()
        {
            if (!TryCheckForSafeModeLockFile()) return false;
            if (!TryDownloadUpdaterSafeModeIndex(out var definition)) return false;
            if (!TryDownloadUpdaterSafeModeArchive(definition)) return false;
            if (!DecompressSafeModeArchive(definition)) return false;
            if (!CreateBackupForOldLauncherFiles()) return false;
            if (!HandleDecompressedSafeModeArchive(definition))
            {
                RollbackSafeModeUpdate();
                return false;
            }
                
            CleanAfterSafeModeUpdate();
            _context.DisableSafeMode();
            _context.SetDirtyFlag(definition.ExecutableToRun, definition);
            _context.Logger.Debug("Launcher update SAFE MODE: COMPLETED");

            return true;
        }
        
        private bool TryCheckForSafeModeLockFile()
        {
            var safeModeLockFilePath = _context.Settings.GetUpdaterSafeModeLockFilePath();
            
            if (_context.FileSystem.FileExists(safeModeLockFilePath))
            {
                _context.FileSystem.DeleteFile(safeModeLockFilePath);
                _context.Logger.Debug("Safe Mode file lock found. Deleting it and exiting Safe Mode.");
                return false;
            }

            using (var fs = _context.FileSystem.CreateFile(safeModeLockFilePath))
            {
                return true;
            }
        }

        private bool TryDownloadUpdaterSafeModeIndex(out UpdaterSafeModeDefinition definition)
        {
            try
            {
                var remoteUrl = _context.Settings.GetRemoteUpdaterSafeModeIndexUrl();
                var partialRemoteUrl = _context.Settings.GetRemoteUpdaterSafeModeIndexUrl();
                if (!string.IsNullOrWhiteSpace(_context.Settings.RemoteUrl))
                    partialRemoteUrl = partialRemoteUrl.Replace(_context.Settings.RemoteUrl, string.Empty);
                var downloadEntry = new DownloadEntry(
                    remoteUrl,
                    partialRemoteUrl,
                    null,
                    null,
                    null
                );

                definition = _context.Downloader.DownloadJson<UpdaterSafeModeDefinition>(downloadEntry, _context.Serializer);
                return true;
            }
            catch
            {
                _context.Logger.Warning("Cannot download the updater safe mode INDEX. Falling back on normal self-update process.");
                definition = null;
                return false;
            }
        }

        private bool TryDownloadUpdaterSafeModeArchive(UpdaterSafeModeDefinition definition)
        {
            try
            {
                var remoteUrl = _context.Settings.GetRemoteUpdaterSafeModeArchiveUrl(definition.ArchiveName);
                var targetDirectory = _context.Settings.GetTempPath();
                var filePath = _context.FileSystem.CombinePaths(targetDirectory, definition.ArchiveName);

                _context.FileSystem.DeleteFile(filePath);
                
                _context.Downloader.Download(remoteUrl, targetDirectory);
                return true;
            }
            catch
            {
                _context.Logger.Warning("Cannot download the updater safe mode ARCHIVE. Falling back on normal self-update process.");
                return false;
            }
        }

        private bool CreateBackupForOldLauncherFiles()
        {
            var backupFolder = _context.Settings.GetUpdaterSafeModeBackupPath();
            string currentFile = string.Empty;

            try
            {
                foreach (var updaterDefinitionEntry in _context.CurrentUpdaterDefinition.Entries)
                {
                    currentFile = updaterDefinitionEntry.RelativePath;
                    var file       = _context.FileSystem.CombinePaths(_context.Settings.RootPath, currentFile);
                    var backupFile = _context.FileSystem.CombinePaths(backupFolder, currentFile);

                    if (_context.FileSystem.FileExists(file))
                    {
                        _context.FileSystem.MoveFile(file, backupFile);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                _context.Logger.Error(e, $"Safe mode: a launcher file cannot be moved [{currentFile}]");
                return false;
            }
        }

        private bool DecompressSafeModeArchive(UpdaterSafeModeDefinition definition)
        {
            var archivePath  = _context.FileSystem.CombinePaths(_context.Settings.GetTempPath(), definition.ArchiveName);
            var targetFolder = _context.Settings.GetUpdaterSafeModeTempPath();
            
            try
            {
                Compressor.Decompress(targetFolder, archivePath.FullPath, null);
                return true;
            }
            catch (Exception e)
            {
                _context.Logger.Error(e, "Cannot decompress the SAFE MODE archive.");
                return false;
            }
        }
        
        private bool HandleDecompressedSafeModeArchive(UpdaterSafeModeDefinition definition)
        {
            var targetFolder = _context.Settings.GetUpdaterSafeModeTempPath();
            var backupFolder = _context.Settings.GetUpdaterSafeModeBackupPath();

            string currentFile = string.Empty;
            
            try
            {
                var files = _context.FileSystem.GetFilesInfo((FilePath)targetFolder);

                foreach (var localFileInfo in files)
                {
                    currentFile = localFileInfo.RelativePath;
                    var file       = _context.FileSystem.CombinePaths(targetFolder, currentFile);
                    var targetFile = _context.FileSystem.CombinePaths(_context.Settings.RootPath, currentFile);

                    if (_context.FileSystem.FileExists(targetFile))
                    {
                        if (_context.FileSystem.IsFileLocked(targetFile))
                        {
                            _context.FileSystem.CopyFile(targetFile, _context.FileSystem.CombinePaths(backupFolder, currentFile));
                            _context.FileSystem.RenameFile(targetFile, (FilePath)_context.FileSystem.GetTemporaryDeletingFileName(targetFile));
                        }
                        else
                        {
                            _context.FileSystem.MoveFile(targetFile, _context.FileSystem.CombinePaths(backupFolder, currentFile));
                        }
                    }

                    _context.FileSystem.MoveFile(file, targetFile);
                }

                return true;
            }
            catch (Exception e)
            {
                _context.Logger.Error(e, $"Cannot apply the SAFE MODE update. A file [{currentFile}] cannot be moved. Maybe it's locked by another process.");
                return false;
            }
        }

        private void CleanAfterSafeModeUpdate()
        {
            try
            {
                _context.FileSystem.DeleteDirectory((FilePath)_context.Settings.GetUpdaterSafeModeTempPath());
                _context.FileSystem.DeleteDirectory((FilePath)_context.Settings.GetUpdaterSafeModeBackupPath());
                _context.FileSystem.DeleteFile(_context.Settings.GetUpdaterSafeModeLockFilePath());
            }
            catch (Exception e)
            {
                _context.Logger.Warning($"Cannot clean the SAFE MODE temporary data. {e}");
            }
        }

        private void RollbackSafeModeUpdate()
        {
            var backupFolder = (FilePath)_context.Settings.GetUpdaterSafeModeBackupPath();

            string currentFile = string.Empty;
            
            try
            {
                var files = _context.FileSystem.GetFilesInfo(backupFolder);

                foreach (var localFileInfo in files)
                {
                    currentFile = localFileInfo.RelativePath;
                    
                    var file       = _context.FileSystem.CombinePaths(backupFolder.FullPath, currentFile);
                    var targetFile = _context.FileSystem.CombinePaths(_context.Settings.RootPath, currentFile);

                    if (_context.FileSystem.FileExists(targetFile))
                    {
                        _context.FileSystem.DeleteFile(targetFile);
                    }

                    _context.FileSystem.MoveFile(file, targetFile);
                }
            }
            catch (Exception e)
            {
                _context.Logger.Error(e, $"Cannot process the SAFE MODE backup. A file [{currentFile}] cannot be moved back. There is a high chance that the Launcher is now corrupted and you need to re-download it from scratch.");
            }
        }

        public long ProgressRangeAmount()
        {
            var accumulator = 0L;

            if (_context.CurrentUpdaterDefinition != null)
            {
                foreach (var updaterDefinitionEntry in _context.CurrentUpdaterDefinition.Entries)
                {
                    accumulator += updaterDefinitionEntry.Size;
                }
            }

            return accumulator;
        }

        private bool IsValid(UpdaterDefinitionEntry entry, out FileValidityDifference difference)
        {
            var filePath = _context.FileSystem.CombinePaths(_context.Settings.RootPath, entry.RelativePath);
            
            var info = _context.FileSystem.GetFileInfo(filePath);
            difference = FileValidityDifference.None;
            
            if (info.Size != entry.Size)
            {
                difference |= FileValidityDifference.Size;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(entry.Hash))
                {
                    var hash = Hashing.GetFileHash(filePath.FullPath, _context.FileSystem);
                    if (entry.Hash != hash)
                    {
                        difference |= FileValidityDifference.Hash;
                    }
                }
            }
            
            //if (!AreLastWritingsEqual(info.LastWriting, entry.LastWriting)) difference |= FileValidityDifference.LastWriting;
            //if (info.Attributes != entry.Attributes) difference |= FileValidityDifference.Attributes;

            return difference == FileValidityDifference.None;
        }

        private bool AreLastWritingsEqual(DateTime lastWriting1, DateTime lastWriting2)
        {
            if (lastWriting1.Year != lastWriting2.Year) return false;
            if (lastWriting1.Month != lastWriting2.Month) return false;
            if (lastWriting1.Day != lastWriting2.Day) return false;
            if (lastWriting1.Hour != lastWriting2.Hour) return false;
            if (lastWriting1.Minute != lastWriting2.Minute) return false;
            if (lastWriting1.Second != lastWriting2.Second) return false;

            return true;
        }

        private void RemoveInvalidFile(FilePath filePath)
        {
            if (_context.FileSystem.IsFileLocked(filePath))
            {
                // If the file is locked by the OS, a workaround is to rename it.
                // Weirdly, Windows allows you to rename the file if it's locked.
                var newFilePath = (FilePath)_context.FileSystem.GetTemporaryDeletingFileName(filePath);

                // I also check if the target filename for renaming is already existing.
                // If so, delete it.
                if (_context.FileSystem.FileExists(newFilePath)) _context.FileSystem.DeleteFile(newFilePath);
                
                _context.FileSystem.RenameFile(filePath, newFilePath);
            }
            else
            {
                _context.FileSystem.DeleteFile(filePath);
            }
        }

        private void HandleAddedFile(UpdaterDefinitionEntry entry)
        {
            var filePath = _context.FileSystem.CombinePaths(_context.Settings.RootPath, entry.RelativePath);
            
            var difference = FileValidityDifference.None;
            var alreadyExisting = _context.FileSystem.FileExists(filePath);
            
            _context.FileSystem.UnlockFile(filePath);

            if (alreadyExisting && IsValid(entry, out difference))
            {
                EnsureDefinition(entry);
                return;
            }

            if (difference.HasFlag(FileValidityDifference.Size))
            {
                RemoveInvalidFile(filePath);

                _context.Downloader.Download(_context.Settings.GetRemoteUpdaterFileUrl(entry.RelativePath),
                                             _context.FileSystem.GetDirectoryPath(filePath).FullPath, 
                    (size) =>
                    {
                        _context.ReportProgress(size);
                    });

                EnsureDefinition(entry);

                _context.SetDirtyFlag(entry.RelativePath);
            }
            else
            {
                if (difference.HasFlag(FileValidityDifference.Hash))
                {
                    RemoveInvalidFile(filePath);

                    alreadyExisting = false;
                }
                
                if (!alreadyExisting)
                {
                    _context.Downloader.Download(_context.Settings.GetRemoteUpdaterFileUrl(entry.RelativePath),
                                                 _context.FileSystem.GetDirectoryPath(filePath).FullPath,
                        (size) =>
                        {
                            _context.ReportProgress(size);
                        });

                    _context.SetDirtyFlag(entry.RelativePath);
                }

                if (!_context.FileSystem.IsFileLocked(filePath))
                    EnsureDefinition(entry);
            }
        }

        private void HandleDeletedFile(UpdaterDefinitionEntry entry)
        {
            var filePath = _context.FileSystem.CombinePaths(_context.Settings.RootPath, entry.RelativePath);

            if (_context.FileSystem.IsFileLocked(filePath))
            {
                var newFilePath = (FilePath)_context.FileSystem.GetTemporaryDeletingFileName(filePath);
                _context.FileSystem.RenameFile(filePath, newFilePath);
            }
            else
            {
                _context.FileSystem.DeleteFile(filePath);
            }
        }

        private void HandleChangedAttributesFile(UpdaterDefinitionEntry entry)
        {
            HandleAddedFile(entry);
        }

        private void HandleUpdatedFile(UpdaterDefinitionEntry entry)
        {
            HandleAddedFile(entry);
        }

        private void HandleUnchangedFile(UpdaterDefinitionEntry entry)
        {
            HandleAddedFile(entry);
        }

        private void EnsureDefinition(UpdaterDefinitionEntry entry)
        {
            var filePath = _context.FileSystem.CombinePaths(_context.Settings.RootPath, entry.RelativePath);
            
            _context.FileSystem.SetFileAttributes(filePath, entry.Attributes);
            _context.FileSystem.SetLastWriteTime(filePath, entry.LastWriting);
        }

        public bool IsUpdateAvailable()
        {
            if (_context.CurrentUpdaterDefinition == null) return false;
            
            foreach (var entry in _context.CurrentUpdaterDefinition.Entries)
            {
                var filePath = _context.FileSystem.CombinePaths(_context.Settings.RootPath, entry.RelativePath);
                
                if (!_context.FileSystem.FileExists(filePath)) return true;
                
                if (!IsValid(entry, out _)) return true;
            }

            return false;
        }
    }
}
