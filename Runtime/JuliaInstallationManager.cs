using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace JuliaPlugin
{
    /// <summary>
    /// Class for checking and setting up the Julia environment if necessary.
    /// </summary>
    public class JuliaInstallationManager : MonoBehaviour
    {
        private readonly static string JULIAENVIRONMENTVARIABLENAME = "JULIA_DIR";
        private static string JULIA_DIR = null;
        private readonly static string JULIAVERSIONINDICATOR = @"\Julia-1.6.3";

        private static bool _juliaIsCheckedAndValid = false;

        private static JuliaInstallationManager _instance = null;
        public static JuliaInstallationManager Instance
        {
            get
            {
                return _instance;
            }
        }

        [Header("Debugger")]
        [SerializeField]
        private GameObject _juliaPluginDebuggerImplementation;
        private IJuliaPluginDebugger _juliaPluginDebugger = null;

        #region magic methods
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
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
            if (_juliaIsCheckedAndValid)
            {
                return;
            }

            try
            {
                JuliaBase.Instance.EvalString("sqrt(9.0)");

                _juliaIsCheckedAndValid = true;
            }
            catch (DllNotFoundException)
            {
                var message = "A necessary dll could not be found!";
                DebugError(message);
                SetupInstallation();
            }
            catch (Exception ue)
            {
                var message = "An unexcepted exception occured: " + ue;
                DebugError(message);
            }
        }

        #endregion magic methods

        public static UnityEvent RequestPathOrDownload = new UnityEvent();
        public static UnityEvent RequestRestart = new UnityEvent();

        public static void SetupInstallation()
        {
            bool juliaDirUser = IsJuliaEnvironmentVariableDefined();
            bool juliaOnPathUser = false;

            var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (userPath != null)
            {
                juliaOnPathUser = userPath.Contains(JULIAVERSIONINDICATOR);
            } 
            else
            {
                Debug.LogError("Couldn't get the user's PATH!");
            }

            if (!juliaDirUser && !juliaOnPathUser)
            {
                var juliaDirectory = SearchForJuliaInstallation();
                if (juliaDirectory != null)
                {
                    DefineJuliaEnvironmentVariable(juliaDirectory);
                    PutJuliaOnPath(juliaDirectory);

                    RequestRestart?.Invoke();
                }
                else
                {
                    RequestPathOrDownload?.Invoke();
                }
            }
            else if (!juliaDirUser && juliaOnPathUser)
            {
                DefineJuliaEnvironmentVariable(GetJuliaInstallationFromPath());
            }
            else if (juliaDirUser && !juliaOnPathUser)
            {
                PutJuliaOnPath(Environment.GetEnvironmentVariable(JULIAENVIRONMENTVARIABLENAME, EnvironmentVariableTarget.User));
            } 
            else
            {
                var message = "A necessary dll or header file could not be found.\n" +
                    "Make sure Julia and Microsoft Visual C++ Redistributable are installed properly.\n" +
                    "Also check the presents of the JuliaPlugin.dll in the Plugins folder of the package.";
                DebugError(message);
            }

            if (Application.isEditor)
            {
                JuliaBase.Instance.Exit();
            }
        }

        #region private methods
        private static void DefineJuliaEnvironmentVariable(string juliaDirectory)
        {
            Environment.SetEnvironmentVariable(JULIAENVIRONMENTVARIABLENAME, juliaDirectory, EnvironmentVariableTarget.User);
        }

        private static void PutJuliaOnPath(string juliaDirectory)
        {
            var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
            if (userPath != null)
            {
                if (!userPath.Contains(juliaDirectory))
                {
                    var newUserPath = AppendToPath(userPath, juliaDirectory);
                    Environment.SetEnvironmentVariable("PATH", newUserPath, EnvironmentVariableTarget.User);
                }
            }
        }

        private static string AppendToPath(string path, string juliaDirectory)
        {
            if (!path.EndsWith(";"))
            {
                path += ";";
            }

            if (!juliaDirectory.EndsWith(@"\bin"))
            {
                juliaDirectory += @"\bin";
            }

            var newPath = path + juliaDirectory;

            return newPath;
        }

        #region boolean methods
        private static bool IsJuliaEnvironmentVariableDefined()
        {
            var juliaEnvironmentVariable = Environment.GetEnvironmentVariable(JULIAENVIRONMENTVARIABLENAME, EnvironmentVariableTarget.User);
            if (juliaEnvironmentVariable != null)
            {
                if (IsJuliaEnvironmentVariableValid(juliaEnvironmentVariable))
                {
                    JULIA_DIR = juliaEnvironmentVariable;
                    return true;
                }
            }

            return false;
        }

        private static bool IsJuliaEnvironmentVariableValid(string juliaEnvironmentVariable)
        {
            try
            {
                return File.Exists(Path.Combine(juliaEnvironmentVariable, @"bin\julia.exe"));
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        #endregion boolean methods

        private static string SearchForJuliaInstallation()
        {
            var appDataPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
            string[] potentialPaths = {
            Path.Combine(appDataPath, @"Local\Programs\Julia\Julia-1.6.3"),
            Path.Combine(appDataPath, @"Local\Programs\Julia-1.6.3")
            };

            foreach (string potentialPath in potentialPaths)
            {
                if (File.Exists(Path.Combine(potentialPath, @"bin\julia.exe")))
                {
                    return potentialPath;
                }
            }

            return null;
        }

        private static string GetJuliaInstallationFromPath()
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);

            var pathSegments = path.Split(';');

            foreach (string segment in pathSegments)
            {
                if (segment.Contains(JULIAVERSIONINDICATOR))
                {
                    return segment.Replace(@"\bin", "");
                }
            }
            return null;
        }

        private static void DebugMessage(string message)
        {
            if (Application.isPlaying)
            {
                if (_instance._juliaPluginDebugger != null)
                {
                    _instance._juliaPluginDebugger.DisplayMessage(message);
                }
                else
                {
                    Debug.Log(message);
                }
            }
            else
            {
                Debug.Log(message);
            }
        }

        private static void DebugWarning(string message)
        {
            if (Application.isPlaying)
            {
                if (_instance._juliaPluginDebugger != null)
                {
                    _instance._juliaPluginDebugger.DisplayWarning(message);
                }
                else
                {
                    Debug.LogWarning(message);
                }
            }
            else
            {
                Debug.LogWarning(message);
            }
        }

        private static void DebugError(string message)
        {
            if (Application.isPlaying)
            {
                if (_instance._juliaPluginDebugger != null)
                {
                    _instance._juliaPluginDebugger.DisplayError(message);
                }
                else
                {
                    Debug.LogError(message);
                }
            }
            else
            {
                Debug.LogError(message);
            }
        }

        #endregion private methods

        #region public methods
        public static void DownloadJulia()
        {
            if (Environment.Is64BitProcess)
            {
                Application.OpenURL("https://julialang-s3.julialang.org/bin/winnt/x64/1.6/julia-1.6.3-win64.exe");
            }
            else
            {
                Application.OpenURL("https://julialang-s3.julialang.org/bin/winnt/x86/1.6/julia-1.6.3-win32.exe");
            }
        }

        public static void DefineJuliaInstallationPath(string juliaDirectory)
        {
            if (IsJuliaEnvironmentVariableValid(juliaDirectory))
            {
                DefineJuliaEnvironmentVariable(juliaDirectory);
                PutJuliaOnPath(juliaDirectory);
            }
            else
            {
                var message = "The given path is invalid. Please make sure to pass the correct path of the Julia directory.";
                DebugWarning(message);
            }
        }

        public static void CheckJuliaInstallation()
        {
            DebugMessage("JULIA_DIR: " + Environment.GetEnvironmentVariable("JULIA_DIR", EnvironmentVariableTarget.User) + "\n" +
                @"\bin on PATH: " + GetJuliaInstallationFromPath() + "\n" +
                "Current Installation: " + SearchForJuliaInstallation());
        }
        #endregion public methods
    }
}