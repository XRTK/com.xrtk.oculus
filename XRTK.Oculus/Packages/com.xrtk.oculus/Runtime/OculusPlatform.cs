// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Platforms;
using XRTK.Interfaces;
using XRTK.Oculus.Plugins;

namespace XRTK.Oculus
{
    public class OculusPlatform : BasePlatform
    {
        private static readonly System.Version NoVersion = new System.Version();

        /// <inheritdoc />
        public override bool IsAvailable => !UnityEngine.Application.isEditor && OculusApi.Version > NoVersion && OculusApi.Initialized;

        /// <inheritdoc />
        public override bool IsBuildTargetAvailable
        {
            get
            {
#if UNITY_EDITOR
                return (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ||
                        UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows ||
                        UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64) &&
                        OculusApi.Version > NoVersion && OculusApi.Initialized;
#else
                return false;
#endif
            }
        }

        /// <inheritdoc />
        public override IMixedRealityPlatform[] PlatformOverrides { get; } =
        {
            new WindowsStandalonePlatform(),
            new AndroidPlatform()
        };
    }
}