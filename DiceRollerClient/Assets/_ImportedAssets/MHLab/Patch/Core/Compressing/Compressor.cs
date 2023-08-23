using System;

namespace MHLab.Patch.Core.Compressing
{
    public static class Compressor
    {
        public static void Compress(string folderToCompress, string outputFile, string password, int compressionLevel, Func<string, bool> filesFilter = null)
        {
            ZipCompressor.ZipFolder(outputFile, password, folderToCompress, compressionLevel, filesFilter);
        }

        public static void Decompress(string folderWhereDecompress, string inputFile, string password)
        {
            ZipCompressor.ExtractZipFile(inputFile, password, folderWhereDecompress);
        }
    }
}