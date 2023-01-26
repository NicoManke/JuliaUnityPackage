/// <summary>
/// JuliaBase's JuliaModule is responsible for referencing and using modules from in Julia.
/// Author: Nico Manke
/// </summary>

using System;
using System.Runtime.InteropServices;

namespace JuliaPlugin
{
    public partial class JuliaBase
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public readonly struct JuliaModule
        {
            public readonly IntPtr wrapped;

            public JuliaModule(IntPtr wrapped)
            {
                var _ = JuliaBase.Instance;
                this.wrapped = wrapped;
            }
            public JuliaModule(JuliaValue juliaValue)
            {
                var _ = JuliaBase.Instance;
                wrapped = juliaValue.wrapped;
            }
            public static explicit operator JuliaModule(JuliaValue JuliaValue)
            {
                return new JuliaModule(JuliaValue);
            }
            public static JuliaModule GetJuliaModule(string moduleName)
            {
                return dll_jl_get_module(moduleName);
            }
        }
    }
}
