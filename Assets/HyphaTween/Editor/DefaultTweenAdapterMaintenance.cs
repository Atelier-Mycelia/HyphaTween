using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AtMycelia.HyphaTween.Editor
{
    public class DefaultTweenAdapterMaintenance
    {
        private const string AssetName = "DefaultTweenAdapter.asset";
        private const string PreferredPackageFolderName = "com.ateliermycelia.hyphatween";
        private const string FallbackFolderPath = "Assets/Resources/AtMycelia/HyphaTween";

        [DidReloadScripts]
        private static void EnsureDefaultTweenAdapterAsset()
        {
            string[] existing = AssetDatabase.FindAssets("t:DefaultTweenAdapter");
            if (existing is { Length: > 0 })
            {
                return;
            }

            bool hasHyphaTweenPackage = TryGetHyphaTweenPackageFolderName(out string packageFolderName);

            if (hasHyphaTweenPackage)
            {
                EnsureForPackageFolder(packageFolderName);
                return;
            }

            EnsureForAssetsFolderViaSoUtils();
        }

        private static bool TryGetHyphaTweenPackageFolderName(out string packageFolderName)
        {
            packageFolderName = null;

            string projectRoot = GetProjectRootAbsolutePath();
            string packagesAbsolute = Path.Combine(projectRoot, "Packages");
            if (!Directory.Exists(packagesAbsolute))
            {
                return false;
            }

            string[] packageDirectories = Directory.GetDirectories(packagesAbsolute);
            string packageDirectory = packageDirectories
                                          .FirstOrDefault(path =>
                                              string.Equals(Path.GetFileName(path), PreferredPackageFolderName, StringComparison.OrdinalIgnoreCase))
                                      ?? packageDirectories.FirstOrDefault(path =>
                                          Path.GetFileName(path).IndexOf("hyphatween", StringComparison.OrdinalIgnoreCase) >= 0);

            if (string.IsNullOrEmpty(packageDirectory))
            {
                return false;
            }

            packageFolderName = Path.GetFileName(packageDirectory);
            return true;
        }

        private static string GetProjectRootAbsolutePath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        }

        private static void EnsureForPackageFolder(string packageFolderName)
        {
            string targetFolderPath = $"Packages/{packageFolderName}/Resources/Runtime";
            EnsureFolderExists(targetFolderPath);

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolderPath}/{AssetName}");
            DefaultTweenAdapter created = ScriptableObject.CreateInstance<DefaultTweenAdapter>();

            AssetDatabase.CreateAsset(created, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created missing DefaultTweenAdapter at '{assetPath}'.");
        }

        private static void EnsureForAssetsFolderViaSoUtils()
        {
            EnsureFolderExists(FallbackFolderPath);
            string assetPath = $"{FallbackFolderPath}/{AssetName}";

            bool invoked = TryInvokeSoUtilsEnsureSoExists(assetPath);
            if (!invoked)
            {
                // Safety fallback in case API shape differs from expected.
                string uniqueAssetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
                DefaultTweenAdapter created = ScriptableObject.CreateInstance<DefaultTweenAdapter>();
                AssetDatabase.CreateAsset(created, uniqueAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.LogWarning(
                    $"SOUtils.EnsureSOExists could not be invoked. Created DefaultTweenAdapter " +
                    $"via AssetDatabase at '{uniqueAssetPath}'.");
                return;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool TryInvokeSoUtilsEnsureSoExists(string assetPath)
        {
            MethodInfo[] methods = typeof(SOUtils)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => string.Equals(m.Name, "EnsureSOExists", StringComparison.Ordinal))
                .ToArray();

            foreach (MethodInfo method in methods)
            {
                if (!method.IsGenericMethodDefinition)
                {
                    continue;
                }

                ParameterInfo[] parameters = method.GetParameters();
                MethodInfo genericMethod = method.MakeGenericMethod(typeof(DefaultTweenAdapter));

                // Most common API shape: EnsureSOExists<T>(string assetPath)
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                {
                    genericMethod.Invoke(null, new object[] { assetPath });
                    return true;
                }

                // Alternate shape support: EnsureSOExists<T>(string folderPath, string assetName)
                if (parameters.Length != 2 ||
                    parameters[0].ParameterType != typeof(string) ||
                    parameters[1].ParameterType != typeof(string)) continue;
                genericMethod.Invoke(null, new object[] { FallbackFolderPath, AssetName });
                return true;
            }

            return false;
        }

        

        private static void EnsureFolderExists(string projectRelativeFolderPath)
        {
            string projectRoot = GetProjectRootAbsolutePath();
            string absolutePath = Path.Combine(
                projectRoot,
                projectRelativeFolderPath.Replace('/', Path.DirectorySeparatorChar));

            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }
        }

        
    }
}