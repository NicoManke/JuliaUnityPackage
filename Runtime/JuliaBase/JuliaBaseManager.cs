/// <summary>
/// A MonoBehaviour expanding the JuliaBase class.
/// Makes sure the JuliaBase instance and the required helper functions for working with multidimensional arrays are initialized.
/// Author: Nico Manke
/// </summary>

using UnityEngine;

namespace JuliaPlugin
{
    public class JuliaBaseManager : MonoBehaviour
    {
        private static JuliaBaseManager _instance;

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

            InitJuliaBase();
        }

        private void OnDestroy()
        {
            JuliaBase.Instance.Exit();
        }

        private void InitJuliaBase()
        {
            var _ = JuliaBase.Instance;
            JuliaBase.InitHelperFunctions();
        }
    }
}
