/// <summary>
/// Author: David Schantz
/// </summary>

using System;
using System.Runtime.InteropServices;

namespace JuliaPlugin
{
    public partial class JuliaBase
    {
        [StructLayout(LayoutKind.Sequential)]
        public readonly struct Type
        {
            private static Type? _cachedBoolType;
            private static Type? _cachedFloatType;
            private static Type? _cachedDoubleType;
            private static Type? _cachedShortType;
            private static Type? _cachedIntType;
            private static Type? _cachedLongType;

            public static Type Bool => _cachedBoolType ?? (_cachedBoolType = JuliaBase.boolType()).Value;
            public static Type Float => _cachedFloatType ?? (_cachedFloatType = JuliaBase.floatType()).Value;
            public static Type Double => _cachedDoubleType ?? (_cachedDoubleType = JuliaBase.doubleType()).Value;
            public static Type Short => _cachedShortType ?? (_cachedShortType = JuliaBase.shortType()).Value;
            public static Type Int => _cachedIntType ?? (_cachedIntType = JuliaBase.intType()).Value;
            public static Type Long => _cachedLongType ?? (_cachedLongType = JuliaBase.longType()).Value;

            private readonly IntPtr wrapped;

        }
    }
}
