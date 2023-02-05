#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace JuliaPlugin
{
    public class JuliaEditorWindow : EditorWindow
    {
        private const int _buttonsMaxWidth = 250;
        private const int _smallSpace = 5;
        private const int _mediumSpace = 8;

        private string _juliaLibraryPath;

        [MenuItem("Window/Julia Package")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(JuliaEditorWindow), false, "Julia Package");
        }

        void OnGUI()
        {
            GUILayout.Space(_smallSpace);

            #region Julia directory
            GUILayout.Label("Julia Directory", EditorStyles.boldLabel);
            _juliaLibraryPath = EditorGUILayout.TextField("Julia Directory Path", _juliaLibraryPath);

            GUILayout.Space(_smallSpace);

            if (GUILayout.Button("Update Directory Path", GUILayout.MaxWidth(_buttonsMaxWidth)))
            {
                JuliaInstallationManager.DefineJuliaInstallationPath(_juliaLibraryPath);
            }

            GUILayout.Space(_mediumSpace);
            #endregion

            #region Julia installation
            GUILayout.Label("Julia Installation", EditorStyles.boldLabel);

            if (GUILayout.Button("Check Julia installation", GUILayout.MaxWidth(_buttonsMaxWidth)))
            {
                JuliaInstallationManager.CheckJuliaInstallation();
            }

            GUILayout.Space(_smallSpace);

            if (GUILayout.Button("Setup Julia installation", GUILayout.MaxWidth(_buttonsMaxWidth)))
            {
                try
                {
                    JuliaBase.Instance.EvalString("sqrt(9.0)");
                    Debug.Log("The Julia test was successful. Julia doesn't need an additional setup.");
                }
                catch (DllNotFoundException dnfe)
                {
                    Debug.LogError("The Julia test failed. Trying to set up Julia. \nA necessary dll couldn't be found! " + dnfe);
                    JuliaInstallationManager.SetupInstallation();
                }
                catch (Exception ue)
                {
                    Debug.LogError("An unexcepted exception occured: " + ue);
                }
                finally
                {
                    JuliaBase.Instance.Exit();
                }
            }

            GUILayout.Space(_smallSpace);

            if (GUILayout.Button("Download Julia", GUILayout.MaxWidth(_buttonsMaxWidth)))
            {
                JuliaInstallationManager.DownloadJulia();
            }

            GUILayout.Space(_mediumSpace);
            #endregion

            #region documentation
            GUILayout.Label("Documenatation", EditorStyles.boldLabel);
            if (GUILayout.Button("Open Package Documenatation", GUILayout.MaxWidth(_buttonsMaxWidth)))
            {
                Application.OpenURL("https://gitlab2.informatik.uni-wuerzburg.de/GE/Teaching/gl2/projects/2021/24-gl2-manke/-/wikis/Documentation/Julia-Unity-Package-Documentation");
            }
            #endregion
        }
    }
}
#endif