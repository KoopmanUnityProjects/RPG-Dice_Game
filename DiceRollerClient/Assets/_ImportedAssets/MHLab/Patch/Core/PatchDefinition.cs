using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MHLab.Patch.Core.Serializing;
using MHLab.Patch.Core.Versioning;
using Version = MHLab.Patch.Core.Versioning.Version;

namespace MHLab.Patch.Core
{
    [Serializable]
    public class PatchIndex : IJsonSerializable
    {
        public List<PatchIndexEntry> Patches;

        public PatchIndex()
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
            foreach (var availablePatch in Patches)
            {
                array.Add(availablePatch.ToJsonNode());
            }
            node.Add("Patches", array);

            return node;
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("Patches"))
            {
                var array = node["Patches"].AsArray;
                Patches = new List<PatchIndexEntry>(array.Count);
                
                for (var i = 0; i < array.Count; i++)
                {
                    var currentNode = array[i];

                    var entry = new PatchIndexEntry();
                    entry.FromJson(currentNode);
                    Patches.Add(entry);
                }
            }
            else
            {
                Patches = new List<PatchIndexEntry>();
            }
        }
    }

    [Serializable]
    public class PatchIndexEntry : IJsonSerializable
    {
        public IVersion From;
        public IVersion To;

        public PatchIndexEntry()
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

            node.Add("From", new JsonString(From.ToString()));
            node.Add("To", new JsonString(To.ToString()));

            return node;
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("From"))
            {
                From = new Version(node["From"].Value);
            }

            if (node.HasKey("To"))
            {
                To = new Version(node["To"].Value);
            }
        }
    }

    [Serializable]
    public class PatchDefinitionEntry : IJsonSerializable
    {
        public PatchOperation Operation;
        public string RelativePath;
        public FileAttributes Attributes;
        public DateTime LastWriting;
        public long Size;

        public PatchDefinitionEntry()
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
                Size = node["Size"].AsLong;
            }
        }
    }

    [Serializable]
    public class PatchDefinition : IJsonSerializable
    {
        public IVersion From;
        public IVersion To;
        public string Hash;
        public long TotalSize;
        public List<PatchDefinitionEntry> Entries;

        public PatchDefinition()
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

            node.Add("From", new JsonString(From.ToString()));
            node.Add("To", new JsonString(To.ToString()));
            node.Add("Hash", new JsonString(Hash));
            node.Add("TotalSize", new JsonNumber(TotalSize));

            var array = new JsonArray();
            for (var i = 0; i < Entries.Count; i++)
            {
                var entry = Entries[i];
                array.Add(entry.ToJsonNode());
            }
            node.Add("Entries", array);

            return node;
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("From"))
            {
                From = new Version(node["From"].Value);
            }

            if (node.HasKey("To"))
            {
                To = new Version(node["To"].Value);
            }

            if (node.HasKey("Hash"))
            {
                Hash = node["Hash"].Value;
            }

            if (node.HasKey("TotalSize"))
            {
                TotalSize = node["TotalSize"].AsLong;
            }

            Entries = new List<PatchDefinitionEntry>();
            if (node.HasKey("Entries"))
            {
                var array = node["Entries"].AsArray;
                for (var i = 0; i < array.Count; i++)
                {
                    var entryNode = array[i];
                    var entry = new PatchDefinitionEntry();
                    entry.FromJson(entryNode);
                    Entries.Add(entry);
                }
            }
        }
    }
}