using System;

namespace MHLab.Patch.Core.Utilities.Asserts
{
    public sealed class AssertFailedException : Exception
    {
        public AssertFailedException() {}
        public AssertFailedException(string message) : base(message) {}
    }
}