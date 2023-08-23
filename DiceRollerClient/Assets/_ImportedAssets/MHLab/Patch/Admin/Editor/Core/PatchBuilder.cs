using MHLab.Patch.Core.Admin.Exceptions;
using MHLab.Patch.Core.Compressing;
using MHLab.Patch.Core.Octodiff;
using System;
using System.Collections.Generic;
using System.Linq;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Utilities;
using MHLab.Patch.Core.Versioning;

namespace MHLab.Patch.Core.Admin
{
    public class PatchBuilder
    {
        private readonly AdminPatchContext _context;

        public PatchBuilder(AdminPatchContext context)
        {
            _context = context;
        }

        public void Build()
        {
            if (_context.VersionFrom == _context.VersionTo) throw new SameVersionsException();

            _context.LogProgress(string.Format(_context.LocalizedMessages.PatchCollectingDefinitions));
            var fromDefinition = GetBuildDefinition(_context.VersionFrom);
            var toDefinition = GetBuildDefinition(_context.VersionTo);

            _context.LogProgress(string.Format(_context.LocalizedMessages.PatchCollectingPatchData));
            var patchDefinition = BuildPatchDefinition(fromDefinition, toDefinition);

            _context.LogProgress(string.Format(_context.LocalizedMessages.PatchBuildingPatch, _context.VersionFrom, _context.VersionTo));
            BuildPatch(patchDefinition, fromDefinition, toDefinition);
            _context.FileSystem.DeleteMultipleFiles((FilePath)_context.Settings.GetPatchesTempFolderPath(), "*.signature");

            _context.LogProgress(string.Format(_context.LocalizedMessages.PatchCompressing, _context.VersionFrom, _context.VersionTo));
            CompressPatch();
            _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchCompressed, _context.VersionFrom, _context.VersionTo));

            _context.LogProgress(string.Format(_context.LocalizedMessages.PatchCleaningWorkspace));
            _context.FileSystem.DeleteDirectory((FilePath)_context.Settings.GetPatchesTempFolderPath());
            _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchCleanedWorkspace));

            _context.LogProgress(string.Format(_context.LocalizedMessages.PatchBuildingDefinition));
            BuildPatchDefinition(patchDefinition);
            _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchBuiltDefinition));

            _context.LogProgress(string.Format(_context.LocalizedMessages.PatchBuildingIndex));
            BuildPatchIndex();
            _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchBuiltIndex));
        }

        private BuildDefinition GetBuildDefinition(IVersion version)
        {
            var content = _context.FileSystem.ReadAllTextFromFile(_context.Settings.GetBuildDefinitionPath(version));
            return _context.Serializer.Deserialize<BuildDefinition>(content);
        }

        private PatchOperation GetOperation(BuildDefinitionEntry newFile, BuildDefinitionEntry oldFile)
        {
            if (newFile.Hash != oldFile.Hash) return PatchOperation.Updated;
            if (newFile.Attributes != oldFile.Attributes) return PatchOperation.ChangedAttributes;

            return PatchOperation.Unchanged;
        }

        private PatchDefinition BuildPatchDefinition(BuildDefinition fromDefinition, BuildDefinition toDefinition)
        {
            var patchDefinition = new PatchDefinition();
            patchDefinition.Entries = new List<PatchDefinitionEntry>();

            foreach (var newDefinition in toDefinition.Entries)
            {
                var oldDefinition = fromDefinition.Entries.FirstOrDefault(f => f.RelativePath == newDefinition.RelativePath);
                var alreadyExists = oldDefinition != null;

                var operation = PatchOperation.Added;

                if (alreadyExists)
                {
                    // The file exists in both versions. I have to check what type of change it got.
                    operation = GetOperation(newDefinition, oldDefinition);
                }

                if (operation != PatchOperation.Unchanged)
                {
                    patchDefinition.Entries.Add(new PatchDefinitionEntry()
                    {
                        Operation = operation,
                        RelativePath = newDefinition.RelativePath
                    });
                }

                _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchCollectedPatchData, newDefinition.RelativePath, operation.ToString()));
            }

            foreach (var oldDefinition in fromDefinition.Entries)
            {
                if (toDefinition.Entries.All(f => f.RelativePath != oldDefinition.RelativePath))
                {
                    // The old file does not exist in the new definition. This means it has been deleted.
                    patchDefinition.Entries.Add(new PatchDefinitionEntry()
                    {
                        Operation = PatchOperation.Deleted,
                        RelativePath = oldDefinition.RelativePath
                    });
                }

                _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchCollectedPatchData, oldDefinition.RelativePath, PatchOperation.Deleted.ToString()));
            }

            return patchDefinition;
        }

        private void BuildPatch(PatchDefinition definition, BuildDefinition fromDefinition, BuildDefinition toDefinition)
        {
            var skipAmount = Math.Max(fromDefinition.Entries.Length, toDefinition.Entries.Length) - definition.Entries.Count;
            for (int i = 0; i < skipAmount; i++)
            {
                _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchSkippingFile));
            }

            foreach (var entry in definition.Entries)
            {
                switch (entry.Operation)
                {
                    case PatchOperation.Added:
                        _context.LogProgress(string.Format(_context.LocalizedMessages.PatchAddingFile, entry.RelativePath));
                        HandleAddedFile(entry);
                        _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchAddedFile, entry.RelativePath));
                        break;
                    case PatchOperation.Updated:
                        _context.LogProgress(string.Format(_context.LocalizedMessages.PatchPatchingFile, entry.RelativePath));
                        HandleUpdatedFile(entry);
                        _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchPatchedFile, entry.RelativePath));
                        break;
                    case PatchOperation.ChangedAttributes:
                        _context.LogProgress(string.Format(_context.LocalizedMessages.PatchChangingAttributesFile, entry.RelativePath));
                        HandleChangedAttributesFile(entry);
                        _context.ReportProgress(string.Format(_context.LocalizedMessages.PatchChangedAttributesFile, entry.RelativePath));
                        break;
                }
            }
        }

        private void HandleAddedFile(PatchDefinitionEntry entry)
        {
            _context.FileSystem.CopyFile(
                _context.FileSystem.CombinePaths(_context.Settings.GetGameFolderPath(_context.VersionTo), entry.RelativePath),
                _context.FileSystem.CombinePaths(_context.Settings.GetPatchesTempFolderPath(), entry.RelativePath)
            );

            var path = _context.FileSystem.CombinePaths(_context.Settings.GetGameFolderPath(_context.VersionTo), entry.RelativePath);
            var info = _context.FileSystem.GetFileInfo(path);
            entry.Attributes = info.Attributes;
            entry.LastWriting = info.LastWriting;
        }

        private void HandleUpdatedFile(PatchDefinitionEntry entry)
        {
            var fromFile = _context.FileSystem.CombinePaths(_context.Settings.GetGameFolderPath(_context.VersionFrom), entry.RelativePath);
            var toFile = _context.FileSystem.CombinePaths(_context.Settings.GetGameFolderPath(_context.VersionTo), entry.RelativePath);
            var patchFile = _context.FileSystem.CombinePaths(_context.Settings.GetPatchesTempFolderPath(), entry.RelativePath + ".patch");
            var signatureFile = _context.FileSystem.CombinePaths(_context.Settings.GetPatchesTempFolderPath(), entry.RelativePath + ".signature");

            _context.FileSystem.CreateDirectory(_context.FileSystem.GetDirectoryPath(patchFile));

            DeltaFileBuilder.Build(fromFile.FullPath, toFile.FullPath, patchFile.FullPath, signatureFile.FullPath);

            var path = _context.FileSystem.CombinePaths(_context.Settings.GetGameFolderPath(_context.VersionTo), entry.RelativePath);
            var info = _context.FileSystem.GetFileInfo(path);
            entry.Attributes = info.Attributes;
            entry.LastWriting = info.LastWriting;
        }

        private void HandleChangedAttributesFile(PatchDefinitionEntry entry)
        {
            var path = _context.FileSystem.CombinePaths(_context.Settings.GetGameFolderPath(_context.VersionTo), entry.RelativePath);
            var info = _context.FileSystem.GetFileInfo(path);
            entry.Attributes = info.Attributes;
            entry.LastWriting = info.LastWriting;
        }

        private void CompressPatch()
        {
            Compressor.Compress(
                _context.Settings.GetPatchesTempFolderPath(),
                _context.FileSystem.CombinePaths(_context.Settings.GetPatchesFolderPath(), _context.PatchName).FullPath,
                null,
                _context.CompressionLevel
            );
        }

        private void BuildPatchDefinition(PatchDefinition definition)
        {
            definition.Hash = Hashing.GetFileHash(_context.FileSystem.CombinePaths(_context.Settings.GetPatchesFolderPath(), _context.PatchName).FullPath, _context.FileSystem);
            definition.From = _context.VersionFrom;
            definition.To = _context.VersionTo;
            definition.TotalSize = _context.FileSystem.GetFileInfo(_context.FileSystem.CombinePaths(_context.Settings.GetPatchesFolderPath(), _context.PatchName)).Size;

            _context.FileSystem.WriteAllTextToFile(_context.Settings.GetPatchIndexPath(_context.VersionFrom, _context.VersionTo), _context.Serializer.Serialize(definition));
        }

        private void BuildPatchIndex()
        {
            PatchIndex index;

            var patchesIndexPath = _context.Settings.GetPatchesIndexPath();
            if (_context.FileSystem.FileExists(patchesIndexPath))
            { 
                index = _context.Serializer.Deserialize<PatchIndex>(_context.FileSystem.ReadAllTextFromFile(patchesIndexPath));
            }
            else
            {
                index = new PatchIndex();
                index.Patches = new List<PatchIndexEntry>();
            }

            index.Patches.Add(new PatchIndexEntry()
            {
                From = _context.VersionFrom,
                To = _context.VersionTo
            });

            _context.FileSystem.WriteAllTextToFile(_context.Settings.GetPatchesIndexPath(), _context.Serializer.Serialize(index));
        }
    }
}
