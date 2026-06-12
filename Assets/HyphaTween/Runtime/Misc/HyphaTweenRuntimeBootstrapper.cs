using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.HyphaTween
{
    public static class HyphaTweenRuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureHyphaTweenHierarchy()
        {
            if (OurRoot != null)
            {
                return;
            }

            RootBootstrapper.EnsureRoot();
            var atMyceliaRoot = RootBootstrapper.Root;

            OurRoot = CreateOurRoot();

            ValidateRoot(out bool success);
            if (!success)
            {
                return;
            }

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