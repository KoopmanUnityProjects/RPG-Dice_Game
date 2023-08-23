using MHLab.Patch.Core.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MHLab.Patch.Core.Serializing;
using Version = MHLab.Patch.Core.Versioning.Version;

namespace MHLab.Patch.Core
{
    [Serializable]
    public class BuildsIndex : IJsonSerializable
    {
        public List<IVersion> AvailableBuilds;

        public BuildsIndex()
        {
        }

        public IVersion GetLast()
        {
            if (AvailableBuilds == null || AvailableBuilds.Count == 0)
                return null;

            return AvailableBuilds.Last();
        }

        public IVersion GetFirst()
        {
            if (AvailableBuilds == null || AvailableBuilds.Count == 0)
                return null;

            return AvailableBuilds.First();
        }

        public bool Contains(IVersion version)
        {
            for (int i = 0; i < AvailableBuilds.Count; i++)
            {
                var current = AvailableBuilds[i];
                if (current.Equals(version)) return true;
            }

            return false;
        }

        public JsonNode ToJsonNode()
        {
            var node = new JsonObject();

            var availableBuildsNode = new JsonArray();
            foreach (var availableBuild in AvailableBuilds)
            {
                availableBuildsNode.Add(new JsonString(availableBuild.ToString()));
            }
            node.Add("AvailableBuilds", availableBuildsNode);

            return node;
        }

        public string ToJson()
        {
            var node = ToJsonNode();

            var stringBuilder = new StringBuilder();
            node.WriteToStringBuilder(stringBuilder, 2, 2, JsonTextMode.Indent);
            return stringBuilder.ToString();
        }

        public void FromJson(JsonNode node)
        {
            AvailableBuilds = new List<IVersion>();

            var buildsNode = node["AvailableBuilds"];
            if (buildsNode != null && buildsNode.IsArray)
            {
                var buildsNodeArray = buildsNode.AsArray;
                foreach (var jsonNode in buildsNodeArray.Children)
                {
                    if (jsonNode != null && jsonNode.IsString)
                    {
                        try
                        {
                            var version = new Version(jsonNode.Value);
                            AvailableBuilds.Add(version);
                        } catch { }
                        
                    }
                }
            }
        }
    }

    [Serializable]
    public class BuildDefinitionEntry : IJsonSerializable
    {
        public string RelativePath;
        public long Size;
        public DateTime LastWriting;
        public string Hash;
        public FileAttributes Attributes;

        public BuildDefinitionEntry()
        {
        }

        public JsonNode ToJsonNode()
        {
            var node = new JsonObject();

            node.Add("RelativePath", new JsonString(RelativePath));
            node.Add("Size", new JsonNumber(Size));
            node.Add("LastWriting", new JsonString(LastWriting.ToString("u")));
            node.Add("Hash", new JsonString(Hash));
            node.Add("Attributes", new JsonNumber((int)Attributes));

            return node;
        }

        public string ToJson()
        {
            var node = ToJsonNode();

            var stringBuilder = new StringBuilder();
            node.WriteToStringBuilder(stringBuilder, 2, 2, JsonTextMode.Indent);
            return stringBuilder.ToString();
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("RelativePath"))
                RelativePath = node["RelativePath"].Value;
            else
                RelativePath = string.Empty;

            if (node.HasKey("Size"))
                Size = node["Size"].AsLong;

            if (node.HasKey("LastWriting"))
                LastWriting = DateTime.Parse(node["LastWriting"].Value);
            else
                LastWriting = DateTime.MinValue;

            if (node.HasKey("Hash"))
                Hash = node["Hash"].Value;
            else
                Hash = string.Empty;

            if (node.HasKey("Attributes"))
                Attributes = (FileAttributes)node["Attributes"].AsInt;
        }
    }

    [Serializable]
    public class BuildDefinition : IJsonSerializable
    {
        public BuildDefinitionEntry[] Entries;

        public BuildDefinition()
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
            var arrayNode = new JsonArray();

            foreach (var buildDefinitionEntry in Entries)
            {
                var entryNode = buildDefinitionEntry.ToJsonNode();
                arrayNode.Add(entryNode);
            }
            
            node.Add("Entries", arrayNode);

            return node;
        }

        public void FromJson(JsonNode node)
        {
            if (node.HasKey("Entries"))
            {
                var array = node["Entries"].AsArray;
                Entries = new BuildDefinitionEntry[array.Count];
                for (var i = 0; i < array.Count; i++)
                {
                    var currentNode = array[i];

                    var entry = new BuildDefinitionEntry();
                    entry.FromJson(currentNode);
                    Entries[i] = entry;
                }
            }
            else
                Entries = new BuildDefinitionEntry[0];

        }
    }
}
