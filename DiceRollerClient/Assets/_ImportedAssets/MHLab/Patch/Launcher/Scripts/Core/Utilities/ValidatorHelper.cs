using System.Text;
using MHLab.Patch.Core.IO;
using MHLab.Patch.Core.Logging;
using MHLab.Patch.Core.Utilities;

namespace MHLab.Patch.Core.Client.Utilities
{
    public static class ValidatorHelper
    {
        public static bool ValidateLocalFiles(BuildDefinition buildDefinition, IFileSystem fileSystem, ILogger logger, string rootPath)
        {
            var errorRecognized = false;

            var builder = new StringBuilder();
            builder.AppendLine("The following files are not valid after debug validation: ");
            
            foreach (var entry in buildDefinition.Entries)
            {
                var filePath = fileSystem.CombinePaths(rootPath, entry.RelativePath);
                var info     = fileSystem.GetFileInfo(filePath);

                if (info.Size != entry.Size)
                {
                    builder.AppendLine(
                        $"[{entry.RelativePath}] with expected size of [{entry.Size}]. Found [{info.Size}]");
                    errorRecognized = true;
                }
                else
                {
                    if (entry.Hash != null)
                    {
                        var hash = Hashing.GetFileHash(filePath.FullPath, fileSystem);
                        if (hash != entry.Hash)
                        {
                            builder.AppendLine(
                                $"[{entry.RelativePath}] with expected hash of [{entry.Hash}]. Found [{hash}]");
                            errorRecognized = true;
                        }
                    }
                }

                if (entry.Attributes != info.Attributes)
                {
                    builder.AppendLine(
                        $"[{entry.RelativePath}] with expected attributes of [{entry.Attributes}]. Found [{info.Attributes}]");
                    errorRecognized = true;
                }
            }

            if (errorRecognized)
            {
                logger.Debug(builder.ToString());
                return false;
            }

            logger.Debug($"Verified {buildDefinition.Entries.Length} files. All good.");
            return true;
        }
    }
}