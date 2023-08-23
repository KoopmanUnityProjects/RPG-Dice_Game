using System;
using System.Collections.Generic;
using MHLab.Patch.Core.Admin.Exceptions;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Utilities;
using MHLab.Patch.Core.Versioning;

namespace MHLab.Patch.Core.Admin
{
    public class BuildBuilder
    {
        private readonly AdminBuildContext _context;

        public BuildBuilder(AdminBuildContext context)
        {
            _context = context;
        }

        public void Build()
        {
            if (_context.BuildVersion == null) throw new ArgumentNullException(nameof(_context.BuildVersion));
            if (BuildExists()) throw new BuildAlreadyExistsException();
            if (ApplicationFolderIsEmpty()) throw new ApplicationFolderIsEmptyException();

            _context.LogProgress(string.Format(_context.LocalizedMessages.NewVersionBuilding, _context.BuildVersion));
            CopyFiles(_context.Settings.GetApplicationFolderPath(),
                      _context.Settings.GetGameFolderPath(_context.BuildVersion));
            _context.ReportProgress(string.Format(_context.LocalizedMessages.NewVersionBuilt, _context.BuildVersion));

            _context.LogProgress(string.Format(_context.LocalizedMessages.VersionFileBuilding));
            BuildVersionFile();
            _context.ReportProgress(string.Format(_context.LocalizedMessages.VersionFileBuilt));

            _context.LogProgress(string.Format(_context.LocalizedMessages.BuildDefinitionBuilding));
            BuildDefinition();
            _context.ReportProgress(string.Format(_context.LocalizedMessages.BuildDefinitionBuilt));

            _context.LogProgress(string.Format(_context.LocalizedMessages.BuildIndexBuilding));
            UpdateBuildIndex();
            _context.ReportProgress(string.Format(_context.LocalizedMessages.BuildCompletedSuccessfully,
                                                  _context.BuildVersion));
        }

        public int GetCurrentFilesToProcessAmount()
        {
            return _context.FileSystem.GetFilesList((FilePath)_context.Settings.GetApplicationFolderPath()).Length;
        }

        public string GetCurrentFilesToProcessSize()
        {
            var  files = _context.FileSystem.GetFilesInfo((FilePath)_context.Settings.GetApplicationFolderPath());
            long size  = 0;

            foreach (var fileInfo in files)
            {
                size += fileInfo.Size;
            }

            return FormatUtility.FormatSizeDecimal(size, 2);
        }

        private bool BuildExists()
        {
            var buildDefinitionPath = _context.Settings.GetBuildDefinitionPath(_context.BuildVersion);
            return _context.FileSystem.FileExists(buildDefinitionPath);
        }

        private bool ApplicationFolderIsEmpty()
        {
            return _context.FileSystem.IsDirectoryEmpty((FilePath)_context.Settings.GetApplicationFolderPath());
        }

        private void CopyFiles(string sourceFolder, string destinationFolder)
        {
            var files = _context.FileSystem.GetFilesList((FilePath)sourceFolder);

            foreach (var file in files)
            {
                var newFile = file.FullPath.Replace(sourceFolder, destinationFolder);

                _context.FileSystem.CopyFile(file, (FilePath)newFile);

                _context.ReportProgress(string.Format(_context.LocalizedMessages.BuildFileProcessed,
                                                      _context.FileSystem.GetFilename(file)));
            }
        }

        private void BuildVersionFile()
        {
            var encoded   = _context.Serializer.Serialize(_context.BuildVersion);
            var encrypted = Rijndael.Encrypt(encoded, _context.Settings.EncryptionKeyphrase);

            var versionFilePath = _context.Settings.GetVersionFilePath(_context.BuildVersion);
            _context.FileSystem.WriteAllTextToFile(versionFilePath, encrypted);
        }

        private void UpdateBuildIndex()
        {
            BuildsIndex index;

            var buildsIndexPath = _context.Settings.GetBuildsIndexPath();
            
            if (_context.FileSystem.FileExists(buildsIndexPath))
            {
                index = _context.Serializer.Deserialize<BuildsIndex>(
                    _context.FileSystem.ReadAllTextFromFile(buildsIndexPath));
            }
            else
            {
                index                 = new BuildsIndex();
                index.AvailableBuilds = new List<IVersion>();
            }

            index.AvailableBuilds.Add(_context.BuildVersion);

            _context.FileSystem.WriteAllTextToFile(buildsIndexPath, _context.Serializer.Serialize(index));
        }

        private void BuildDefinition()
        {
            var files       = _context.FileSystem.GetFilesInfo((FilePath)_context.Settings.GetApplicationFolderPath());
            var definitions = new BuildDefinition();
            definitions.Entries = new BuildDefinitionEntry[files.Length + 1];

            for (var i = 0; i < files.Length; i++)
            {
                var file = files[i];
                definitions.Entries[i] = new BuildDefinitionEntry()
                {
                    Attributes = file.Attributes,
                    Hash = Hashing.GetFileHash(
                        _context.FileSystem.CombinePaths(_context.Settings.GetApplicationFolderPath(), file.RelativePath).FullPath,
                        _context.FileSystem
                        ),
                    LastWriting  = file.LastWriting,
                    RelativePath = file.RelativePath,
                    Size         = file.Size
                };

                _context.ReportProgress(string.Format(_context.LocalizedMessages.BuildDefinitionProcessed,
                                                      _context.FileSystem.GetFilename((FilePath)file.RelativePath)));
            }

            var versionFilePath = _context.Settings.GetVersionFilePath(_context.BuildVersion);
            var versionFile = _context.FileSystem.GetFileInfo(versionFilePath);
            definitions.Entries[files.Length] = new BuildDefinitionEntry()
            {
                Attributes   = versionFile.Attributes,
                Hash         = Hashing.GetFileHash(versionFilePath.FullPath, _context.FileSystem),
                LastWriting  = versionFile.LastWriting,
                RelativePath = versionFile.RelativePath,
                Size         = versionFile.Size
            };
            _context.ReportProgress(string.Format(_context.LocalizedMessages.BuildDefinitionProcessed,
                                                  _context.FileSystem.GetFilename((FilePath)versionFile.RelativePath)));

            var buildDefinitionPath = _context.Settings.GetBuildDefinitionPath(_context.BuildVersion);
            _context.FileSystem.WriteAllTextToFile(buildDefinitionPath, _context.Serializer.Serialize(definitions));
        }
    }
}