using System;
using System.IO;
using System.Text;
using MHLab.Patch.Core.Serializing;

namespace MHLab.Patch.Core
{
    [Serializable]
    public class UpdaterDefinitionEntry : IJsonSerializable
    {
        public PatchOperation Operation;
        public string RelativePath;
        public FileAttributes Attributes;
        public DateTime LastWriting;
        public long Size;
        public string Hash;

        public UpdaterDefinitionEntry()
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

            node.Add("Operation", new JsonNumber((int)Operation));
            node.Add("RelativePath", new JsonString(RelativePath));
            node.Add("Attributes", new JsonNumber((int)Attributes));
            node.Add("LastWriting", new JsonString(LastWriting.ToString("u")));
            node.Add("Size", new JsonNumber(Size));
            node.Add("Hash", new JsonString(Hash));

            return node;
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("Operation"))
            {
                Operation = (PatchOperation)node["Operation"].AsInt;
            }

            if (node.HasKey("RelativePath"))
            {
                RelativePath = node["RelativePath"].Value;
            }

            if (node.HasKey("Attributes"))
            {
                Attributes = (FileAttributes)node["Attributes"].AsInt;
            }

            if (node.HasKey("LastWriting"))
            {
                LastWriting = DateTime.Parse(node["LastWriting"].Value);
            }

            if (node.HasKey("Size"))
            {
                Size = node["Size"].AsInt;
            }

            if (node.HasKey("Hash"))
            {
                Hash = node["Hash"].Value;
            }
        }
    }

    [Serializable]
    public class UpdaterDefinition : IJsonSerializable
    {
        public UpdaterDefinitionEntry[] Entries;

        public UpdaterDefinition()
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
            
            var array = new JsonArray();
            for (var i = 0; i < Entries.Length; i++)
            {
                var entry = Entries[i];
                array.Add(entry.ToJsonNode());
            }
            node.Add("Entries", array);

            return node;
        }

        public void FromJson(JsonNode node)
        {
            var hasEntries = node.HasKey("Entries");
            if (hasEntries == false)
            {
                Entries = Array.Empty<UpdaterDefinitionEntry>();
                return;
            }

            node = node["Entries"];
            
            if (node.IsArray)
            {
                var array = node.AsArray;
                Entries = new UpdaterDefinitionEntry[array.Count];
                
                for (var i = 0; i < array.Count; i++)
                {
                    var entryNode = array[i];
                    var entry = new UpdaterDefinitionEntry();
                    entry.FromJson(entryNode);
                    Entries[i] = entry;
                }
            }
            else
            {
                Entries = Array.Empty<UpdaterDefinitionEntry>();
            }
        }
    }

    [Serializable]
    public class UpdaterSafeModeDefinition : IJsonSerializable
    {
        public string ArchiveName;
        public string ExecutableToRun;

        public UpdaterSafeModeDefinition()
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
            
            node.Add("ArchiveName", new JsonString(ArchiveName));
            node.Add("ExecutableToRun", new JsonString(ExecutableToRun));

            return node;
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("ArchiveName"))
            {
                ArchiveName = node["ArchiveName"].Value;
            }
            
            if (node.HasKey("ExecutableToRun"))
            {
                ExecutableToRun = node["ExecutableToRun"].Value;
            }
        }
    }
}