/// <summary>
/// JuliaBase's JuliaFunction is responsible for referencing and calling functions written in Julia.
/// Author: David Schantz
/// Editor: Nico Manke
/// </summary>

using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace JuliaPlugin
{
    public partial class JuliaBase
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public readonly struct JuliaFunction
        {
            public delegate JuliaValue Delegate(object first, params object[] args);

            public readonly IntPtr wrapped;
            public JuliaFunction(IntPtr wrapped)
            {
                var _ = JuliaBase.Instance;
                this.wrapped = wrapped;
            }
            public JuliaFunction(JuliaValue juliaValue)
            {
                var _ = JuliaBase.Instance;
                this.wrapped = juliaValue.wrapped;
            }

            public JuliaValue Invoke(params JuliaValue[] args)
            {
                return dll_jl_callN(this, args, args.Length);
            }
            public JuliaValue Invoke(object first, params object[] args)
            {
                return Invoke(args.Prepend(first).Select(JuliaValue.Wrap).ToArray());
            }
            public JuliaValue Invoke()
            {
                return Invoke(new JuliaValue[] { });
            }

            public static explicit operator JuliaFunction(JuliaValue juliaValue)
            {
                return new JuliaFunction(juliaValue);
            }

            public static implicit operator Delegate(JuliaFunction f)
            {
                return f.Invoke;
            }

            public static JuliaFunction GetJuliaFunctionWithModule(string name, string moduleName)
            {
                return getFunction(name, moduleName);
            }
            public static JuliaFunction GetJuliaFunction(string name)
            {
                return dll_jl_get_function(name);
            }
        }
    }
}
