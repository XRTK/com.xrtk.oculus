// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using UnityEditor;
using XRTK.Editor;
using XRTK.Extensions;
using XRTK.Editor.Utilities;

namespace XRTK.Oculus.Editor
{
    [InitializeOnLoad]
    internal static class OculusPackageInstaller
    {
        private static readonly string DefaultPath = $"{MixedRealityPreferences.ProfileGenerationPath}Oculus";
        private static readonly string HiddenPath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(OculusPathFinder)).BackSlashes()}{Path.DirectorySeparatorChar}{MixedRealityPreferences.HIDDEN_PROFILES_PATH}");

        static OculusPackageInstaller()
        {
            EditorApplication.delayCall += CheckPackage;
        }

        [MenuItem("Mixed Reality Toolkit/Packages/Install Oculus Package Assets...", true)]
        private static bool ImportPackageAssetsValidation()
        {
            return !Directory.Exists($"{DefaultPath}{Path.DirectorySeparatorChar}Profiles");
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
                EditorPreferences.Set($"{nameof(OculusPackageInstaller)}.Profiles", PackageInstaller.TryInstallAssets(HiddenPath, $"{DefaultPath}{Path.DirectorySeparatorChar}Profiles"));
            }
        }
    }
}
