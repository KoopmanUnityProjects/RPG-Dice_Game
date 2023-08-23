using System.Collections.Generic;
using System.Linq;
using MHLab.Patch.Core.Admin.Exceptions;
using MHLab.Patch.Core.Compressing;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Utilities;

namespace MHLab.Patch.Core.Admin
{
    public class UpdaterBuilder
    {
        private readonly AdminPatcherUpdateContext _context;

        public UpdaterBuilder(AdminPatcherUpdateContext context)
        {
            _context = context;
        }

        public void Build()
        {
            if (_context.FileSystem.IsDirectoryEmpty((FilePath)_context.Settings.GetUpdaterFolderPath())) throw new UpdaterFolderIsEmptyException();

            _context.LogProgress(string.Format(_context.LocalizedMessages.UpdaterCollectingOldDefinition));
            var oldDefinition = GetCurrentDefinition();

            _context.LogProgress(string.Format(_context.LocalizedMessages.UpdaterCollectingFiles));
            var files = GetFiles();

            var definition       = BuildDefinition(files, oldDefinition);
            var updaterIndexPath = _context.Settings.GetUpdaterIndexPath();

            _context.FileSystem.DeleteFile(updaterIndexPath);

            _context.FileSystem.DeleteFile((FilePath)_context.Settings.GetUpdaterDeployPath(_context.LauncherArchiveName));

            _context.LogProgress(string.Format(_context.LocalizedMessages.UpdaterCompressingArchive));
            Compressor.Compress(_context.Settings.GetUpdaterFolderPath(), _context.Settings.GetUpdaterDeployPath(_context.LauncherArchiveName), null, _context.CompressionLevel);
            _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdaterCompressedArchive));

            _context.FileSystem.WriteAllTextToFile(updaterIndexPath, _context.Serializer.Serialize(definition));
            _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdaterSavedDefinition));
        }

        private UpdaterDefinition BuildDefinition(LocalFileInfo[] files, UpdaterDefinition oldDefinition)
        {
            var definition = new UpdaterDefinition();
            var entries = new List<UpdaterDefinitionEntry>();

            for (var i = 0; i < files.Length; i++)
            {
                var currentInfo = files[i];

                var entry = new UpdaterDefinitionEntry();
                entry.RelativePath = currentInfo.RelativePath;
                entry.Attributes = currentInfo.Attributes;
                entry.LastWriting = currentInfo.LastWriting;
                entry.Size = currentInfo.Size;
                entry.Operation = GetOperation(entry, oldDefinition);
                entry.Hash = Hashing.GetFileHash(_context.FileSystem.CombinePaths(_context.Settings.GetUpdaterFolderPath(), currentInfo.RelativePath).FullPath, _context.FileSystem);

                entries.Add(entry);

                _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdaterProcessedFile, currentInfo.RelativePath));
            }

            foreach (var oldDefinitionEntry in oldDefinition.Entries)
            {
                if (entries.All(e => e.RelativePath != oldDefinitionEntry.RelativePath))
                {
                    entries.Add(new UpdaterDefinitionEntry()
                    {
                        RelativePath = oldDefinitionEntry.RelativePath,
                        Operation = PatchOperation.Deleted
                    });
                }

                _context.ReportProgress(string.Format(_context.LocalizedMessages.UpdaterProcessedFile, oldDefinitionEntry.RelativePath));
            }

            definition.Entries = entries.ToArray();
            return definition;
        }

        public int GetCurrentFilesToProcessAmount()
        {
            return _context.FileSystem.GetFilesList((FilePath)_context.Settings.GetUpdaterFolderPath()).Count(f => !f.FullPath.EndsWith(_context.Settings.UpdaterIndexFileName));
        }

        public string GetCurrentFilesToProcessSize()
        {
            var files = _context.FileSystem.GetFilesInfo((FilePath)_context.Settings.GetUpdaterFolderPath()).Where(f => !f.RelativePath.EndsWith(_context.Settings.UpdaterIndexFileName));
            long size = 0;

            foreach (var fileInfo in files)
            {
                size += fileInfo.Size;
            }

            return FormatUtility.FormatSizeDecimal(size, 2);
        }

        private LocalFileInfo[] GetFiles()
        {
            var files = _context.FileSystem.GetFilesInfo((FilePath)_context.Settings.GetUpdaterFolderPath());
            return files.Where(f => !f.RelativePath.EndsWith(_context.Settings.UpdaterIndexFileName)).ToArray();
        }

        private UpdaterDefinition GetCurrentDefinition()
        {
            var updaterIndexPath = _context.Settings.GetUpdaterIndexPath();
            
            if (_context.FileSystem.FileExists(updaterIndexPath)) 
                return _context.Serializer.Deserialize<UpdaterDefinition>(_context.FileSystem.ReadAllTextFromFile(updaterIndexPath));

            return new UpdaterDefinition()
            {
                Entries = new UpdaterDefinitionEntry[0]
            };
        }

        private PatchOperation GetOperation(UpdaterDefinitionEntry current, UpdaterDefinition oldDefinition)
        {
            if (oldDefinition.Entries.All(e => e.RelativePath != current.RelativePath)) return PatchOperation.Added;

            var oldEntry = oldDefinition.Entries.FirstOrDefault(e => e.RelativePath == current.RelativePath);

            if (oldEntry.Size == current.Size)
            {
                if (oldEntry.Attributes == current.Attributes)
                {
                    return PatchOperation.Unchanged;
                }

                return PatchOperation.ChangedAttributes;
            }

            return PatchOperation.Updated;
        }
    }
}
