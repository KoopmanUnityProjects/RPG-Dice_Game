using System;
using System.Text;
using MHLab.Patch.Core.Serializing;

namespace MHLab.Patch.Core.Client
{
    [Serializable]
    public class SettingsOverride : IJsonSerializable
    {
        public bool DebugMode { get; set; }

        public bool PatcherUpdaterSafeMode { get; set; }

        public SettingsOverride()
        {
        }

        public string ToJson()
        {
            var node = ToJsonNode();

            var stringBuilder = new StringBuilder();
            node.WriteToStringBuilder(stringBuilder, 2, 2, JsonTextMode.Indent);
            return stringBuilder.ToString();
        }

        public JsonNode ToJsonNode()
        {
            var node = new JsonObject();

            node.Add("DebugMode", new JsonBool(DebugMode));
            node.Add("PatcherUpdaterSafeMode", new JsonBool(PatcherUpdaterSafeMode));

            return node;
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("DebugMode"))
            {
                DebugMode = node["DebugMode"].AsBool;
            }

            if (node.HasKey("PatcherUpdaterSafeMode"))
            {
                PatcherUpdaterSafeMode = node["PatcherUpdaterSafeMode"].AsBool;
            }
        }
    }
}