using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace JuliaPlugin
{
    /// <summary>
    /// Class for handling the inclusion of Julia files and referencing contained functions and modules. 
    /// </summary>
    public class JuliaModelHandler : MonoBehaviour
    {
        private static JuliaModelHandler _instance;
        public static JuliaModelHandler Instance
        {
            get
            {
                return _instance;
            }
        }

        #region fields

        [Header("Debugger")]
        [SerializeField]
        private GameObject _juliaPluginDebuggerImplementation;
        private IJuliaPluginDebugger _juliaPluginDebugger = null;

        [Header("Model References")]
        [SerializeField]
        private string[] _moduleNames;
        [SerializeField]
        private string[] _functionNames;

        private Dictionary<string, JuliaBase.JuliaModule> _modelModules = new Dictionary<string, JuliaBase.JuliaModule>();
        private Dictionary<string, JuliaBase.JuliaFunction> _modelFunctions = new Dictionary<string, JuliaBase.JuliaFunction>();

        public string[] ModuleNames
        {
            get
            {
                return _moduleNames;
            }
        }

        public string[] FunctionNames
        {
            get
            {
                return _functionNames;
            }
        }

        public Dictionary<string, JuliaBase.JuliaModule> ModelModules
        {
            get
            {
                return _modelModules;
            }
        }

        public Dictionary<string, JuliaBase.JuliaFunction> ModelFunctions
        {
            get
            {
                return _modelFunctions;
            }
        }
        #endregion

        [Header("Debugging")]
        [SerializeField]
        private string[] _foundInclusions;

        #region magic methods
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }

            if (_juliaPluginDebuggerImplementation != null)
            {
                _juliaPluginDebugger = _juliaPluginDebuggerImplementation.GetComponent<IJuliaPluginDebugger>();
            }
        }

        private void Start()
        {
            GetModuleReferences();
            GetFunctionReferences();
        }
        #endregion

        /// <summary>
        /// Includes the Julia files listed in the Inclusions.txt file into the current Julia process. This allows using the contained functions and modules of those files.
        /// </summary>
        public static void LoadModelDependencies()
        {
            var juliaScriptsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "JuliaScripts");
            string[] includes = FileHandler.Instance.GetInclusions();
            Instance._foundInclusions = includes;
            if (includes == null)
            {
                var message = "\"Inclusions.txt\" file could not be found! Check the StreamingAssets folder and restart.";

                if (_instance._juliaPluginDebugger != null)
                {
                    _instance._juliaPluginDebugger.DisplayError(message);
                } 
                else
                {
                    Debug.LogError(message);
                }
                return;
            }
            foreach (string juliaFile in includes)
            {
                var baseIncludeCommand = JuliaBase.BaseInclude(Path.Combine(juliaScriptsFolderPath, juliaFile)).Replace(@"\", "/");
                JuliaBase.Instance.EvalString(baseIncludeCommand);
            }
        }

        private void GetModuleReferences()
        {
            if (_moduleNames.Length == 0)
            {
                return;
            }
            foreach (string moduleName in _moduleNames)
            {
                JuliaBase.JuliaModule moduleReference = new JuliaBase.JuliaModule(JuliaBase.Instance.EvalString(moduleName).wrapped);
                _modelModules.Add(moduleName, moduleReference);
            }
        }

        private void GetFunctionReferences()
        {
            if (_functionNames.Length == 0)
            {
                return;
            }
            foreach (string functionName in _functionNames)
            {
                JuliaBase.JuliaFunction functionReference = new JuliaBase.JuliaFunction(JuliaBase.Instance.EvalString(functionName).wrapped);
                _modelFunctions.Add(functionName, functionReference);
            }
        }

        public JuliaBase.JuliaValue InvokeReferencedFunction(string functionName, JuliaBase.JuliaValue[] parameters)
        {
            JuliaBase.JuliaValue juliaReference = new JuliaBase.JuliaValue();

            try
            {
                juliaReference = _modelFunctions[functionName].Invoke(parameters);
            }
            catch (KeyNotFoundException knfe)
            {
                Debug.LogError("The given function \"" + functionName + "\" is not part of the referenced functions. " + knfe);
            }

            return juliaReference;
        }

        public JuliaBase.JuliaValue InvokeReferencedFunction(string functionName, object[] parameters)
        {
            JuliaBase.JuliaValue[] juliaValues = new JuliaBase.JuliaValue[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                juliaValues[i] = JuliaBase.JuliaValue.Wrap(parameters[i]);
            }

            return InvokeReferencedFunction(functionName, juliaValues);
        }

        public JuliaBase.JuliaValue InvokeReferencedFunction(string functionName)
        {
            return InvokeReferencedFunction(functionName, new JuliaBase.JuliaValue[] { });
        }
    }
}