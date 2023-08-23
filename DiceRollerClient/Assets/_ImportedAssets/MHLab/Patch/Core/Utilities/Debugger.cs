using MHLab.Patch.Core.Serializing;

namespace MHLab.Patch.Core.Utilities
{
    public static class Debugger
    {
        public static string GenerateDebugReport<TSettings>(TSettings settings, string additionalInfo, ISerializer serializer) where TSettings : ISettings
        {
            var content = "===================== START DEBUG REPORT =====================\n";
            content += "==============================================================\n\n";

            content += additionalInfo + "\n\n";
            content += settings.ToDebugString() + "\n\n";
            
            content += "====================== END DEBUG REPORT ======================\n";
            content += "==============================================================\n";

            return content;
        }
    }
}
