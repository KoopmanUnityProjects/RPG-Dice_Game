using System.Diagnostics;

namespace MHLab.Patch.Core.Utilities.Asserts
{
    public static partial class Assert
    {
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void Fail()
        {
            throw new AssertFailedException();
        }

        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void Fail(string message)
        {
            throw new AssertFailedException(message);
        }

        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void Fail(string message, params object[] parameters)
        {
            throw new AssertFailedException(string.Format(message, parameters));
        }
        
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void Check(bool condition)
        {
            if (condition == false) Fail();
        }
        
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void Check(bool condition, string message)
        {
            if (condition == false) Fail(message);
        }
        
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void Check(bool condition, string message, params object[] parameters)
        {
            if (condition == false) Fail(message, parameters);
        }
        
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void NotNull(object obj)
        {
            if (obj == null) Fail();
        }
        
        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void NotNull(object obj, string message)
        {
            if (obj == null) Fail(message);
        }
    }
}