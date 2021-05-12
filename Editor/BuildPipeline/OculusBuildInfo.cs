// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor.Build.Reporting;
using XRTK.Attributes;
using XRTK.Interfaces;
using XRTK.Oculus;

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

            // TODO Generate manifest
        }
    }
}
