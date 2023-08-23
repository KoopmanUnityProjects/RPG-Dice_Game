namespace MHLab.Patch.Core.Versioning
{
    public interface IVersionFactory
    {
        IVersion Create(int major, int minor, int patch);
        IVersion Create(string version);
        IVersion Create(IVersion version);
        IVersion Create();

        IVersion Parse(string text);
    }

    public sealed class VersionFactory : IVersionFactory
    {
        public IVersion Create(int major, int minor, int patch)
        {
            return new Version(major, minor, patch);
        }

        public IVersion Create(string version)
        {
            return new Version(version);
        }

        public IVersion Create(IVersion version)
        {
            return new Version(version);
        }

        public IVersion Create()
        {
            return new Version();
        }

        public IVersion Parse(string text)
        {
            return Version.Parse(text);
        }
    }
}
