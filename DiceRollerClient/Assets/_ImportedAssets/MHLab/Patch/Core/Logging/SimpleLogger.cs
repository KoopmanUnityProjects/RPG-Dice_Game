using System;
using System.IO;
using System.Runtime.CompilerServices;
using MHLab.Patch.Core.IO;

namespace MHLab.Patch.Core.Logging
{
    public class SimpleLogger : ILogger, IDisposable
    {
        private bool         _isDebug;
        private StreamWriter _streamWriter;
        private IFileSystem  _fileSystem;

        public SimpleLogger(IFileSystem fileSystem, string logfilePath, bool isDebug)
        {
            _isDebug    = isDebug;
            _fileSystem = fileSystem;

            var filePath = new FilePath(_fileSystem.GetDirectoryPath((FilePath)logfilePath).FullPath, logfilePath);
            _fileSystem.CreateDirectory((FilePath)filePath.BasePath);
            
            _streamWriter           = new StreamWriter(_fileSystem.GetFileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            _streamWriter.AutoFlush = true;
        }
        
        public void Debug(string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "")
        {
            if (!_isDebug) return;
            
            _streamWriter.WriteLine(BuildMessage(messageTemplate, "DBG", callerPath, callerLine, callerMember));
        }

        public void Info(string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "")
        {
            _streamWriter.WriteLine(BuildMessage(messageTemplate, "INF", callerPath, callerLine, callerMember));
        }

        public void Warning(string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "")
        {
            _streamWriter.WriteLine(BuildMessage(messageTemplate, "WRN", callerPath, callerLine, callerMember));
        }

        public void Error(Exception exception, string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "")
        {
            _streamWriter.WriteLine(BuildMessage(messageTemplate, "ERR", callerPath, callerLine, callerMember));

            if (exception != null)
            {
                _streamWriter.WriteLine(exception.Message);
                _streamWriter.WriteLine(exception.StackTrace);
            }
        }

        private string BuildMessage(string messageTemplate, string level, string callerPath, long callerLine, string callerMember)
        {
            if (_isDebug)
            {
                var caller = Path.GetFileName(callerPath);
                messageTemplate = $"[{DateTime.UtcNow}] [{level}] [{caller}::{callerMember}@{callerLine}] {messageTemplate}";
            }
            return messageTemplate;
        }

        public void Dispose()
        {
            _streamWriter?.Flush();
            _streamWriter?.Dispose();
        }
    }
}