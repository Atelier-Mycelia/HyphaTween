using UnityEngine;

namespace AtMycelia.HyphaTween
{
    public static class HyphaTweenRuntimeBootstrap
    {
        private const string RootName = "AtMycelia";
        private const string HyphaTweenName = "HyphaTween";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureHyphaTweenHierarchy()
        {
            var atMyceliaRoot = FindOrCreateGameObject(RootName);
            Object.DontDestroyOnLoad(atMyceliaRoot);

            var hyphaTweenRoot = FindOrCreateHyphaTweenObject();
            hyphaTweenRoot.name = HyphaTweenName;

            PutUnderAtMyceliaRoot();
            void PutUnderAtMyceliaRoot()
            {
                var hyphaTrans = hyphaTweenRoot.transform;
                if (hyphaTrans.parent != atMyceliaRoot.transform)
                {
                    hyphaTrans.SetParent(atMyceliaRoot.transform, true);
                }
            }

            hyphaTweenRoot.GetOrAddComponent<TweenManager>();
        }

        private static GameObject FindOrCreateGameObject(string objectName)
        {
            var existing = GameObject.Find(objectName);
            return existing != null ? 
                existing : 
                new GameObject(objectName);
        }

        private static GameObject FindOrCreateHyphaTweenObject()
        {
            var existingManager = Object.FindFirstObjectByType<TweenManager>(FindObjectsInactive.Include);
            if (existingManager != null)
            {
                return existingManager.gameObject;
            }

            var existingByName = GameObject.Find(HyphaTweenName);
            return existingByName != null ? 
                existingByName : 
                new GameObject(HyphaTweenName);
        }

        
    }
}