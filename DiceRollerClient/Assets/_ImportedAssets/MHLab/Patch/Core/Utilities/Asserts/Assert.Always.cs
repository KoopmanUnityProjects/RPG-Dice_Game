using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace MHLab.Patch.Core.Utilities.Asserts
{
    public static partial class Assert
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysFail()
        {
            throw new AssertFailedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysFail(string message)
        {
            throw new AssertFailedException(message);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysFail(string message, params object[] parameters)
        {
            throw new AssertFailedException(string.Format(message, parameters));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysCheck(bool condition)
        {
            if (condition == false) AlwaysFail();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysCheck(bool condition, string message)
        {
            if (condition == false) AlwaysFail(message);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysCheck(bool condition, string message, params object[] parameters)
        {
            if (condition == false) AlwaysFail(message, parameters);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysNotNull(object obj)
        {
            if (obj == null) AlwaysFail();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerHidden]
        public static void AlwaysNotNull(object obj, string message)
        {
            if (obj == null) AlwaysFail(message);
        }
    }
}