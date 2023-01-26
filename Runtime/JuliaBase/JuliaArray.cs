/// <summary>
/// JuliaBase's Array is used for translating Julia arrays into C# arrays
/// Author: David Schantz
/// </summary>

using System;
using System.Runtime.InteropServices;

namespace JuliaPlugin
{
    public partial class JuliaBase
    {
        public interface Array
        {
            JuliaValue Wrapped { get; }
        }

        public abstract class Array<T, ArrayType> : Array, IDisposable where T : unmanaged
        {

            protected readonly ArrayType data;
            public JuliaValue Wrapped { get; }
            private GCHandle gcHandle;

            protected Array(ArrayType data)
            {
                var _ = JuliaBase.Instance;
                this.data = data;
                gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var ptr = gcHandle.AddrOfPinnedObject();
                Wrapped = wrap(ptr, fetchType());
            }

            protected abstract JuliaValue wrap(IntPtr ptr, Type juliaType);

            private static Type fetchType()
            {
                switch (default(T))
                {
                    case bool _: return Type.Bool;
                    case float _: return Type.Float;
                    case double _: return Type.Double;
                    case short _: return Type.Short;
                    case int _: return Type.Int;
                    case long _: return Type.Long;
                    default: throw new ArgumentException($"No julia type exists for {nameof(T)}");
                }
            }

            public static implicit operator JuliaValue(Array<T, ArrayType> arr) => arr.Wrapped;

            public void Dispose() => gcHandle.Free();
        }

        public class Array1D<T> : Array<T, T[]> where T : unmanaged
        {

            public T this[int i] => data[i];

            protected override JuliaValue wrap(IntPtr ptr, Type juliaType) =>
                wrapArray1D(juliaType, ptr, data.Length);
            public Array1D(T[] data) : base(data) { }
            public Array1D(int size) : this(new T[size]) { }
        }

        public class Array2D<T> : Array<T, T[,]> where T : unmanaged
        {

            public T this[int x, int y] => data[x, y];

            protected override JuliaValue wrap(IntPtr ptr, Type juliaType) =>
                wrapArray2D(juliaType, ptr, data.GetLength(0), data.GetLength(1));
            public Array2D(T[,] data) : base(data) { }
            public Array2D(int width, int height) : this(new T[width, height]) { }
        }

        public class Array3D<T> : Array<T, T[,,]> where T : unmanaged
        {

            public T this[int x, int y, int z] => data[x, y, z];

            protected override JuliaValue wrap(IntPtr ptr, Type juliaType) =>
                wrapArray3D(juliaType, ptr, data.GetLength(0), data.GetLength(1), data.GetLength(2));
            public Array3D(T[,,] data) : base(data) { }
            public Array3D(int width, int height, int depth) : this(new T[width, height, depth]) { }
        }

    }
}
