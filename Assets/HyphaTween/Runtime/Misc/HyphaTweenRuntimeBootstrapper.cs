using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.HyphaTween
{
    public static class HyphaTweenRuntimeBootstrapper
    {
        /// <summary>
        /// Called by HyphaTweenMaintenanceRunner. MaintenanceRunner runs the Common lib's
        /// core startup first (priority-wise), so the AtMycelia root already exists here.
        /// </summary>
        internal static void EnsureHyphaTweenHierarchy()
        {
            if (OurRoot != null)
            {
                return;
            }

            OurRoot = CreateOurRoot();

            ValidateRoot(out bool success);
            if (!success)
            {
                return;
            }

            var atMyceliaRoot = GameObjectRootBootstrapper.Root;
            OurRoot.transform.SetParent(atMyceliaRoot.transform);
        }

        private static GameObject OurRoot { get; set; }

        private static GameObject CreateOurRoot()
        {
            GameObject result = null;
            var prefab = Resources.Load<GameObject>(OurRootPrefabPath);

            if (prefab == null)
            {
                string errorMessage = $"Could not find HyphaTween root prefab at " +
                    $"{OurRootPrefabPath}. The prefab is either missing or at " +
                    $"a different path.";
                Debug.LogError(errorMessage);
            }
            else
            {
                result = UnityObj.Instantiate(prefab);
                result.name = prefab.name;
                UnityObj.DontDestroyOnLoad(result);
            }

            return result;
        }

        private const string OurRootPrefabPath = "Runtime/HyphaTween";

        private static void ValidateRoot(out bool success)
        {
            OurRoot.TryGetComponent<TweenManager>(out var manager);
            success = manager != null;
            if (manager == null)
            {
                string errorMessage = $"HyphaTweenRoot is missing its TweenManager component!";
                Debug.LogError(errorMessage);
            }
        }
    }
}