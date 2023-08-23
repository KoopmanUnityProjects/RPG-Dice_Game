namespace MHLab.Patch.Core.Serializing
{
    public interface ISerializer
    {
        string Serialize<TObject>(TObject obj) where TObject : IJsonSerializable;
        TObject Deserialize<TObject>(string data) where TObject : IJsonSerializable;
        TObject DeserializeOn<TObject>(TObject obj, string data) where TObject : IJsonSerializable;
    }
}
