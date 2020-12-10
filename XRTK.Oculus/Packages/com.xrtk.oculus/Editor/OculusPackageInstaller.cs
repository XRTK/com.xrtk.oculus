// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using XRTK.Editor;
using XRTK.Extensions;
using XRTK.Utilities.Editor;

namespace XRTK.Oculus.Editor
{
    [InitializeOnLoad]
    internal static class OculusPackageInstaller
    {
        private static readonly string DefaultPath = $"{MixedRealityPreferences.ProfileGenerationPath}Oculus";
        private static readonly string HiddenProfilePath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(OculusPathFinder)).ToForwardSlashes()}\\{MixedRealityPreferences.HIDDEN_PROFILES_PATH}");
        private static readonly string HiddenPrefabPath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(OculusPathFinder)).ToForwardSlashes()}\\{MixedRealityPreferences.HIDDEN_PREFABS_PATH}");
        private static readonly Dictionary<string, string> DefaultOculusAssets = new Dictionary<string, string>
        {
            {HiddenProfilePath,  $"{DefaultPath}\\Profiles"},
            {HiddenPrefabPath, $"{DefaultPath}\\Prefabs"}
        };

        static OculusPackageInstaller()
        {
            EditorApplication.delayCall += CheckPackage;
        }

        [MenuItem("Mixed Reality Toolkit/Packages/Install Oculus Package Assets...", true)]
        private static bool ImportPackageAssetsValidation()
        {
            return !Directory.Exists($"{DefaultPath}\\Profiles") || !Directory.Exists($"{DefaultPath}\\Prefabs");
        }

        [MenuItem("Mixed Reality Toolkit/Packages/Install Oculus Package Assets...")]
        private static void ImportPackageAssets()
        {
            EditorPreferences.Set($"{nameof(OculusPackageInstaller)}.Profiles", false);
            EditorApplication.delayCall += CheckPackage;
        }

        private static void CheckPackage()
        {
            if (!EditorPreferences.Get($"{nameof(OculusPackageInstaller)}.Profiles", false))
            {
                EditorPreferences.Set($"{nameof(OculusPackageInstaller)}.Profiles", PackageInstaller.TryInstallAssets(DefaultOculusAssets));
            }            
        }
    }
}
