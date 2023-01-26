/// <summary>
/// JuliaBase's JuliaValue is responsible for referencing and converting values from Julia into C#.
/// Author: David Schantz
/// Editor: Nico Manke
/// </summary>

using System;
using System.Runtime.InteropServices;

namespace JuliaPlugin
{
    public partial class JuliaBase
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public readonly struct JuliaValue
        {
            public readonly IntPtr wrapped;

            public JuliaValue(IntPtr wrapped)
            {
                var _ = JuliaBase.Instance;
                this.wrapped = wrapped;
            }

            /// <summary>
            /// Checks if the given JuliaValue contains a valid poinzer to a value in Julia.
            /// </summary>
            private void ThrowIfInvalid()
            {
                if (wrapped == IntPtr.Zero) throw new Exception($"Could not convert Julia Object pointer, was {wrapped}");
            }

            /// <summary>
            /// Converts a Julia bool into a C# bool.
            /// </summary>
            public bool UnboxBool
            {
                get
                {
                    ThrowIfInvalid();
                    return dll_jl_unbox_bool(this);
                }
            }

            /// <summary>
            /// Converts a Julia Float64 into a C# double.
            /// </summary>
            public double UnboxFloat64
            {
                get
                {
                    ThrowIfInvalid();
                    return dll_jl_unbox_float64(this);
                }
            }

            /// <summary>
            /// Converts a Julia Float32 into a C# float.
            /// </summary>
            public float UnboxFloat32
            {
                get
                {
                    ThrowIfInvalid();
                    return dll_jl_unbox_float32(this);
                }
            }

            /// <summary>
            /// Converts a Julia Int64 into a C# long.
            /// </summary>
            public long UnboxInt64
            {
                get
                {
                    ThrowIfInvalid();
                    return dll_jl_unbox_int64(this);
                }
            }

            /// <summary>
            /// Converts a Julia Int32 into a C# int.
            /// </summary>
            public int UnboxInt32
            {
                get
                {
                    ThrowIfInvalid();
                    return dll_jl_unbox_int32(this);
                }
            }

            /// <summary>
            /// Converts a Julia int16 into a C# short.
            /// </summary>
            public short UnboxInt16
            {
                get
                {
                    ThrowIfInvalid();
                    return dll_jl_unbox_int16(this);
                }
            }

            /// <summary>
            /// Fetches the nth item of a Julia tuple.
            /// </summary>
            /// <param name="i"></param>
            public JuliaValue this[int i]
            {
                get
                {
                    ThrowIfInvalid();
                    return getNthField(this, i);
                }
            }

            /// <summary>
            /// Converts a value of C# type T into its corresponding Julia type.
            /// </summary>
            /// <param name="toWrap"></param>
            /// <returns>Wrappped value that can be used in Julia.</returns>
            public static JuliaValue Wrap(object toWrap)
            {
                switch (toWrap)
                {
                    case bool b:
                        return dll_jl_wrap_bool(b);
                    case double d:
                        return dll_jl_wrap_float64(d);
                    case float f:
                        return dll_jl_wrap_float32(f);
                    case long l:
                        return dll_jl_wrap_int64(l);
                    case int i:
                        return dll_jl_wrap_int32(i);
                    case short s:
                        return dll_jl_wrap_int16(s);
                    case JuliaValue jv:
                        return jv;
                    case Array a:
                        return a.Wrapped;
                    case JuliaFunction f:
                        return new JuliaValue(f.wrapped);
                    default: throw new ArgumentException($"Type {toWrap.GetType()} is not supported as JuliaValue");
                }
            }

            /// <summary>
            /// Copies the data of a Julia array into a new one-dimensional C# array.
            /// Doesn't work for Julia arrays with the type Any.
            /// </summary>
            public T[] AsArray1D<T>() where T : unmanaged
            {
                ThrowIfInvalid();
                unsafe
                {
                    var size = new int[1];
                    var arrayPtr = (T*)asArray(this, dimensions: 1, size);
                    var ret = new T[size[0]];
                    for (var i = 0; i < ret.Length; i++)
                        ret[i] = arrayPtr[i];

                    return ret;
                }
            }

            /// <summary>
            /// Copies the data of a two-dimensional Julia array with one type into a new two-dimensional C# array.
            /// Doesn't work for Julia arrays with the type Any.
            /// </summary>
            public T[,] AsArray2D<T>() where T : unmanaged
            {
                ThrowIfInvalid();
                unsafe
                {
                    var sizes = new int[2];
                    var arrayPtr = (T*)asArray(this, dimensions: 2, sizes);
                    var width = sizes[0];
                    var height = sizes[1];
                    var ret = new T[width, height];
                    for (var x = 0; x < width; x++)
                        for (var y = 0; y < height; y++)
                            ret[x, y] = arrayPtr[x + y * width];
                    return ret;
                }
            }

            /// <summary>
            /// Copies the data of a three-dimensional Julia array with one type into a new three-dimensional C# array.
            /// Doesn't work for Julia arrays with the type Any.
            /// </summary>
            public T[,,] AsArray3D<T>() where T : unmanaged
            {
                ThrowIfInvalid();
                unsafe
                {
                    var sizes = new int[3];
                    var arrayPtr = (T*)asArray(this, dimensions: 3, sizes);
                    var width = sizes[0];
                    var height = sizes[1];
                    var depth = sizes[2];
                    var ret = new T[width, height, depth];
                    for (var x = 0; x < width; x++)
                        for (var y = 0; y < height; y++)
                            for (var z = 0; z < depth; z++)
                                ret[x, y, z] = arrayPtr[x + (y + z * height) * width];

                    return ret;
                }
            }
        };
    }
}
