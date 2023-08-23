using System;
using System.Runtime.CompilerServices;

namespace MHLab.Patch.Core.Logging
{
    public interface ILogger
    {
        void Debug(string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "");
        void Info(string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "");
        void Warning(string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "");
        void Error(Exception exception, string messageTemplate, [CallerFilePath] string callerPath = "", [CallerLineNumber] long callerLine = 0, [CallerMemberName] string callerMember = "");
    }
}
