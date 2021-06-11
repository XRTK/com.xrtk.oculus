// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using XRTK.Attributes;
using XRTK.Interfaces;
using XRTK.Oculus;
using XRTK.Services;

namespace XRTK.Editor.BuildPipeline
{
    [RuntimePlatform(typeof(OculusPlatform))]
    public class OculusBuildInfo : AndroidBuildInfo
    {
        /// <inheritdoc />
        public override IMixedRealityPlatform BuildPlatform => new OculusPlatform();

        /// <inheritdoc />
        public override void OnPreProcessBuild(BuildReport report)
        {
            base.OnPreProcessBuild(report);

            if (!MixedRealityToolkit.ActivePlatforms.Contains(BuildPlatform) ||
                EditorUserBuildSettings.activeBuildTarget != BuildTarget)
            {
                return;
            }

            if (BuildPlatform.GetType() == typeof(OculusPlatform))
            {
                // TODO generate manifest
            }
        }
    }
}
