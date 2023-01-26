/// <summary>
/// JuliaBase functions as C#'s interface for using Julia.
/// Author: David Schantz 
/// Editor: Nico Manke
/// </summary>

using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JuliaPlugin
{
    public partial class JuliaBase
    {

        private static JuliaBase __instance = null;

        public static JuliaBase Instance
        {
            get
            {
                if (__instance == null) __instance = new JuliaBase();
                return __instance;
            }
        }

        private const string PATHTOHELPERJLFILE = @".\JuliaScripts\helper.jl";

        private static bool _hasAllHelperFunctions = false;

        private static JuliaFunction _getFromDimOne;
        private static JuliaFunction _getFromDimTwo;
        private static JuliaFunction _getFromDimThree;
        private static JuliaFunction _getFromDimFour;
        private static JuliaFunction _matrixToArray;

        private JuliaBase()
        {
            Init();
        }

        #region extern

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate void DebugFunc(string message);

        private static readonly DebugFunc log = msg => Debug.Log(msg);
        private static readonly DebugFunc warn = msg => Debug.LogWarning(msg);
        private static readonly DebugFunc error = msg => Debug.LogError(msg);

        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern void dll_jl_init(IntPtr log, IntPtr warn, IntPtr error);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern void dll_jl_atexit_hook();

        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr dll_base_include([MarshalAs(UnmanagedType.LPStr)] string moduleName, [MarshalAs(UnmanagedType.LPStr)] string funcName);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] public static extern JuliaValue dll_jl_eval_string([MarshalAs(UnmanagedType.LPStr)] string str);

        #region Boxer and Unboxer
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern bool dll_jl_unbox_bool(JuliaValue input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue dll_jl_wrap_bool(bool input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] public static extern double dll_jl_unbox_float64(JuliaValue input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue dll_jl_wrap_float64(double input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern float dll_jl_unbox_float32(JuliaValue input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue dll_jl_wrap_float32(float input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern long dll_jl_unbox_int64(JuliaValue input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue dll_jl_wrap_int64(long input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern int dll_jl_unbox_int32(JuliaValue input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue dll_jl_wrap_int32(int input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern short dll_jl_unbox_int16(JuliaValue input);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue dll_jl_wrap_int16(short input);
        #endregion Boxer and Unboxer

        #region Array Handling
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern IntPtr asArray(JuliaValue obj, int dimensions, int[] size);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue getNthField(JuliaValue container, int n);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue wrapArray1D(Type type, IntPtr ptr, int size);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue wrapArray2D(Type type, IntPtr ptr, int width, int height);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue wrapArray3D(Type type, IntPtr ptr, int width, int height, int depth);
        #endregion Array Handling

        #region Caller and Getter
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaFunction getFunction([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string moduleName);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaFunction dll_jl_get_function([MarshalAs(UnmanagedType.LPStr)] string name);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaModule dll_jl_get_module([MarshalAs(UnmanagedType.LPStr)] string str);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaFunction dll_jl_get_function_by_module(JuliaModule module, [MarshalAs(UnmanagedType.LPStr)] string str);
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern JuliaValue dll_jl_callN(JuliaFunction func, JuliaValue[] arguments, int argumentCount);
        #endregion Caller and Getter

        #region Type Handling
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern Type boolType();
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern Type floatType();
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern Type doubleType();
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern Type shortType();
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern Type intType();
        [DllImport("JuliaInterface", CallingConvention = CallingConvention.Cdecl)] private static extern Type longType();
        #endregion Type Handling
        #endregion extern

        #region public methods
        /// <summary>
        /// Starts Julia and makes Julia's logs available for feedback.
        /// </summary>
        private void Init()
        {
            var logPtr = Marshal.GetFunctionPointerForDelegate(log);
            var warnPtr = Marshal.GetFunctionPointerForDelegate(warn);
            var errorPtr = Marshal.GetFunctionPointerForDelegate(error);
            dll_jl_init(logPtr, warnPtr, errorPtr);
        }

        /// <summary>
        /// Sends Julia the command to finish.
        /// </summary>
        public void Exit()
        {
            dll_jl_atexit_hook();
        }

        /// The functions below were added by Nico Manke

        /// <summary>
        /// Loads and references the JuliaFunctions needed for extracting Values and one-dimensional Arrays from multi-dimensional Julia Arrays
        /// </summary>
        public static void InitHelperFunctions()
        {
            if (_hasAllHelperFunctions)
            {
                return;
            }

            string helperPath = Path.GetFullPath(PATHTOHELPERJLFILE);

            string slashHelperPath = helperPath.Replace("\\", "/");

            string baseIncludeSlashHelperPath = BaseInclude(slashHelperPath);

            dll_jl_eval_string(baseIncludeSlashHelperPath);

            _getFromDimOne = new JuliaFunction(dll_jl_eval_string("getFromDimOne").wrapped);
            _getFromDimTwo = new JuliaFunction(dll_jl_eval_string("getFromDimTwo").wrapped);
            _getFromDimThree = new JuliaFunction(dll_jl_eval_string("getFromDimThree").wrapped);
            _getFromDimFour = new JuliaFunction(dll_jl_eval_string("getFromDimFour").wrapped);
            _matrixToArray = new JuliaFunction(dll_jl_eval_string("matrixToArray").wrapped);

            _hasAllHelperFunctions = _getFromDimOne != null
                && _getFromDimTwo != null
                && _getFromDimThree != null
                && _getFromDimFour != null
                && _matrixToArray != null;
        }

        /// <summary>
        /// Evaluates an instruction for Julia saved as a String.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Reference to the determined or calculated data in Julia.</returns>
        public JuliaValue EvalString(string message)
        {
            return dll_jl_eval_string(message);
        }

        /// <summary>
        /// Evaluates multiple instructions for Julia saved as a String array at once.
        /// </summary>
        /// <param name="stringArray"></param>
        public void EvalStringArray(string[] stringArray)
        {
            for (int i = 0; i < stringArray.Length; i++)
            {
                EvalString(stringArray[i]);
            }
        }

        /// <summary>
        /// Gets a specific function from a given JuliaModule.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="funcName"></param>
        /// <returns>A JuliaFunction from a module.</returns>
        public JuliaFunction GetJuliaFunctionByModule(JuliaModule module, string funcName)
        {
            return dll_jl_get_function_by_module(module, funcName);
        }

        #region helper methods

        /// <summary>
        /// Calls the Julia function getFromDimOne and extracts a JuliaValue from 1st dimension of a n-dimensional array.
        /// </summary>
        /// <returns>JuliaValue of a value or a sub-array</returns>
        public JuliaValue GetFromDimOne(JuliaValue array, int indexDimOne)
        {
            JuliaValue arrayPart;

            if (_getFromDimOne == null)
            {
                Debug.LogError("GetFromDimOne was not initialized.");
                InitHelperFunctions();
            }

            arrayPart = _getFromDimOne.Invoke(new JuliaValue[] { array, JuliaValue.Wrap(indexDimOne) });

            return arrayPart;
        }

        /// <summary>
        /// Calls the Julia Function getFromDimTwo and extracts a JuliaValue from 2nd dimension of a n-dimensional array.
        /// </summary>
        /// <returns>JuliaValue of a value or a sub-array</returns>
        public JuliaValue GetFromDimTwo(JuliaValue array, int indexDimOne, int indexDimTwo)
        {
            JuliaValue arrayPart;

            if (_getFromDimTwo == null)
            {
                Debug.LogError("GetFromDimTwo was not initialized.");
                InitHelperFunctions();
            }

            arrayPart = _getFromDimTwo.Invoke(new JuliaValue[] { array, JuliaValue.Wrap(indexDimOne), JuliaValue.Wrap(indexDimTwo) });

            return arrayPart;
        }

        /// <summary>
        /// Calls the Julia Function getFromDimThree and extracts a JuliaValue from 3rd dimension of a n-dimensional array.
        /// </summary>
        /// <returns>JuliaValue of a value or a sub-array</returns>
        public JuliaValue GetFromDimThree(JuliaValue array, int indexDimOne, int indexDimTwo, int indexDimThree)
        {
            JuliaValue arrayPart;

            if (_getFromDimThree == null)
            {
                Debug.LogError("GetFromDimThree was not initialized.");
                InitHelperFunctions();
            }

            arrayPart = _getFromDimThree.Invoke(new JuliaValue[] { array,
            JuliaValue.Wrap(indexDimOne),
            JuliaValue.Wrap(indexDimTwo),
            JuliaValue.Wrap(indexDimThree) });

            return arrayPart;
        }

        /// <summary>
        /// Calls the Julia Function getFromDimFour and extracts a JuliaValue from 4th dimension of a n-dimensional array.
        /// </summary>
        /// <returns>JuliaValue of a value or a sub-array</returns>
        public JuliaValue GetFromDimFour(JuliaValue array, int indexDimOne, int indexDimTwo, int indexDimThree, int indexDimFour)
        {
            JuliaValue arrayPart;

            if (_getFromDimFour == null)
            {
                Debug.LogError("GetFromDimFour was not initialized.");
                InitHelperFunctions();
            }

            arrayPart = _getFromDimFour.Invoke(new JuliaValue[] { array,
            JuliaValue.Wrap(indexDimOne),
            JuliaValue.Wrap(indexDimTwo),
            JuliaValue.Wrap(indexDimThree),
            JuliaValue.Wrap(indexDimFour) });

            return arrayPart;
        }

        /// <summary>
        /// Converts a Julia matrix into an one dimensional array with the same length.
        /// </summary>
        public JuliaValue MatrixToArray(JuliaValue matrix)
        {
            JuliaValue array;

            if (_matrixToArray == null)
            {
                Debug.LogError("MatrixToArray was not initialized.");
                InitHelperFunctions();
            }

            array = _matrixToArray.Invoke(new JuliaValue[] { matrix });

            return array;
        }

        #endregion helper methods
        #region additional methods

        /// <summary>
        /// generates the include() command of Julia's Base module for a specific module and a specific function
        /// </summary>
        public static string BaseInclude(string moduleName, string funcName)
        {
            return "Base.include(" + moduleName + ", \"" + funcName + "\")";
        }

        /// <summary>
        /// generates the include() command of Julia's Base Module for the Main Module and a specific Function
        /// </summary>
        /// <param name="funcName"></param>
        public static string BaseInclude(string funcName)
        {
            return BaseInclude("Main", funcName);
        }

        #endregion additional methods
        #endregion public methods
    }
}
