using System;

namespace MHLab.Patch.Core
{
    [Serializable]
    public enum PatchOperation
    {
        Unchanged,
        Deleted,
        Updated,
        ChangedAttributes,
        Added
    }
}
