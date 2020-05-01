// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        private static readonly string HiddenPath = Path.GetFullPath($"{PathFinderUtility.ResolvePath<IPathFinder>(typeof(OculusPathFinder)).ToForwardSlashes()}\\{MixedRealityPreferences.HIDDEN_PROFILES_PATH}");

        static OculusPackageInstaller()
        {
            if (!EditorPreferences.Get($"{nameof(OculusPackageInstaller)}", false))
            {
                EditorPreferences.Set($"{nameof(OculusPackageInstaller)}", PackageInstaller.TryInstallProfiles(HiddenPath, DefaultPath));
            }
        }
    }
}
