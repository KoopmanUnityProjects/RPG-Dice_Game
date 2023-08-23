using System;
using MHLab.Patch.Core.Client.Exceptions;
using MHLab.Patch.Core.Compressing;
using MHLab.Patch.Core.Utilities;
using System.Linq;
using MHLab.Patch.Core.Client.Utilities;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Octodiff;
using MHLab.Patch.Core.Utilities.Asserts;

namespace MHLab.Patch.Core.Client
{
    public class Updater : IUpdater
    {
        private readonly UpdatingContext _context;

        public Updater(UpdatingContext context)
        {
            _context = context;
        }

        public void Update()
        {
            _context.Logger.Info("Update process started.");
            var performedOperations = 0;

            CheckAvailableDiskSpace();

            foreach (var patchDefinition in _context.PatchesPath)
            {
                _context.Logger.Info($"Applying update {patchDefinition.From + "_" + patchDefinition.To} [{patchDefinition.Hash}]");
                PerformUpdate(patchDefinition);
                performedOperations += patchDefinition.Entries.Count;
            }
            
            if (_context.Settings.DebugMode)
            {
                var currentVersion         = _context.GetLocalVersion();
                var currentBuildDefinition = _context.GetBuildDefinition(currentVersion);
                Assert.AlwaysCheck(ValidatorHelper.ValidateLocalFiles(currentBuildDefinition, _context.FileSystem, _context.Logger, _context.Settings.GetGamePath()));
            }

            _context.Logger.Info($"Update process completed. Applied {_context.PatchesPath.Count} patches with {performedOperations} operations.");
        }

        private void CheckAvailableDiskSpace()
        {
            var requiredSpace = 0L;
            
            foreach (var patchDefinition in _context.PatchesPath)
            {
                requiredSpace += patchDefinition.TotalSize;
            }

            if (_context.Settings.EnableDiskSpaceCheck)
            {
                var availableDiskSpace =
                    _context.FileSystem.GetAvailableDiskSpace((FilePath)_context.Settings.RootPath);

                if (requiredSpace > availableDiskSpace)
                    throw new Exception(
                        $"Not enough disk space for the update. Required space [{requiredSpace}], available space [{availableDiskSpace}]"
                    );
            }
        }

        public long ProgressRangeAmount()
        {
            var accumulator = 0L;

            foreach (var patchDefinition in _context.PatchesPath)
            {
                foreach (var patchDefinitionEntry in patchDefinition.Entries)
                {
                    accumulator += patchDefinitionEntry.Size;
                }

                accumulator += patchDefinition.TotalSize;
            }

            return accumulator;
        }

        private void PerformUpdate(PatchDefinition definition)
        {
            _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateDownloadingArchive, definition.From, definition.To));
            DownloadPatch(definition);
            _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateDownloadedArchive, definition.From, definition.To));

            _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateDecompressingArchive, definition.From, definition.To));
            DecompressPatch(definition);
            _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateDecompressedArchive, definition.From, definition.To));

            foreach (var definitionEntry in definition.Entries)
            {
                ProcessFile(definition, definitionEntry);
            }

            _context.FileSystem.DeleteDirectory((FilePath)_context.Settings.GetTempPath());
        }

        private void DownloadPatch(PatchDefinition definition)
        {
            _context.FileSystem.CreateDirectory((FilePath)_context.Settings.GetTempPath());
            
            var archivePath = _context.Settings.GetDownloadedPatchArchivePath(definition.From, definition.To);
            var leftAttempts = _context.Settings.PatchDownloadAttempts;
            var success = false;

            do
            {
                try
                {
                    _context.Downloader.Download(_context.Settings.GetRemotePatchArchiveUrl(definition.From, definition.To), _context.Settings.GetTempPath(), (size) => _context.ReportProgress(size));
                    var downloadedArchiveHash = Hashing.GetFileHash(archivePath, _context.FileSystem);
                    if (downloadedArchiveHash == definition.Hash)
                    {
                        success = true;
                        break;
                    }
                }
                catch
                {
                    // ignored
                }

                _context.FileSystem.DeleteFile((FilePath)archivePath);
                leftAttempts--;
                _context.Logger.Debug($"The patch {definition.From + "_" + definition.To} failed to download. Retrying...");
            } while (leftAttempts > 0);

            if (!success) throw new PatchCannotBeDownloadedException();
        }

        private void DecompressPatch(PatchDefinition definition)
        {
            var path = _context.Settings.GetUncompressedPatchArchivePath(definition.From, definition.To);
            _context.FileSystem.CreateDirectory((FilePath)path);

            Compressor.Decompress(path, _context.Settings.GetDownloadedPatchArchivePath(definition.From, definition.To), null);
        }

        private void ProcessFile(PatchDefinition definition, PatchDefinitionEntry entry)
        {
            switch (entry.Operation)
            {
                case PatchOperation.Added:
                    _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingNewFile, entry.RelativePath));
                    HandleAddedFile(definition, entry);
                    _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateProcessedNewFile, entry.RelativePath), entry.Size);
                    break;
                case PatchOperation.Deleted:
                    _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingDeletedFile, entry.RelativePath));
                    HandleDeletedFile(definition, entry);
                    _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateProcessedDeletedFile, entry.RelativePath), entry.Size);
                    break;
                case PatchOperation.Updated:
                    _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingUpdatedFile, entry.RelativePath));
                    HandleUpdatedFile(definition, entry);
                    _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateProcessedUpdatedFile, entry.RelativePath), entry.Size);
                    break;
                case PatchOperation.ChangedAttributes:
                    _context.LogProgress(string.Format(_context.LocalizedMessages.UpdateProcessingChangedAttributesFile, entry.RelativePath));
                    HandleChangedAttributesFile(definition, entry);
                    _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdateProcessedChangedAttributesFile, entry.RelativePath), entry.Size);
                    break;
            }
        }

        private void HandleAddedFile(PatchDefinition definition, PatchDefinitionEntry entry)
        {
            var sourcePath = _context.FileSystem.CombinePaths(_context.Settings.GetUncompressedPatchArchivePath(definition.From, definition.To), entry.RelativePath);
            var destinationPath = _context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath);
            _context.FileSystem.DeleteFile(destinationPath);
            _context.FileSystem.MoveFile(sourcePath, destinationPath);

            EnsureDefinition(destinationPath.FullPath, entry);
        }

        private void HandleDeletedFile(PatchDefinition definition, PatchDefinitionEntry entry)
        {
            var path = _context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath);
            _context.FileSystem.DeleteFile(path);
        }

        private void HandleUpdatedFile(PatchDefinition definition, PatchDefinitionEntry entry)
        {
            var filePath = _context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath);
            var fileBackupPath = (FilePath)(filePath + ".bak");
            var patchPath = _context.FileSystem.CombinePaths(_context.Settings.GetUncompressedPatchArchivePath(definition.From, definition.To), entry.RelativePath + ".patch");

            try
            {
                _context.FileSystem.RenameFile(filePath, fileBackupPath);

                DeltaFileApplier.Apply(fileBackupPath.FullPath, patchPath.FullPath, filePath.FullPath);

                EnsureDefinition(filePath.FullPath, entry);
            }
            catch
            {

            }
            finally
            {
                _context.FileSystem.DeleteFile(fileBackupPath);
            }
        }

        private void HandleChangedAttributesFile(PatchDefinition definition, PatchDefinitionEntry entry)
        {
            var path = _context.FileSystem.CombinePaths(_context.Settings.GetGamePath(), entry.RelativePath);

            EnsureDefinition(path.FullPath, entry);
        }

        private void EnsureDefinition(string filePath, PatchDefinitionEntry entry)
        {
            _context.FileSystem.SetFileAttributes((FilePath)filePath, entry.Attributes);
            _context.FileSystem.SetLastWriteTime((FilePath)filePath, entry.LastWriting);
        }

        public bool IsUpdateAvailable()
        {
            return _context.PatchesIndex.Patches.Any(p => p.From.Equals(_context.CurrentVersion));
        }
    }
}
