// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Plugins;

namespace XRTK.Oculus.Extensions
{
    public static class MixedRealityPoseExtensions
    {
        /// <summary>
        /// Extension method to convert a Oculus Pose to an XRTK MixedRealityPose
        /// </summary>
        /// <param name="pose">Extension (this) base Oculus PoseF type</param>
        /// <param name="adjustForEyeHeight"></param>
        /// <returns>Returns an XRTK MixedRealityPose</returns>
        public static MixedRealityPose ToMixedRealityPose(this OculusApi.Posef pose, bool adjustForEyeHeight = false)
        {
            return new MixedRealityPose
            (
                position: new Vector3(pose.Position.x,
                                      adjustForEyeHeight ? pose.Position.y + OculusApi.EyeHeight : pose.Position.y,
                                      -pose.Position.z),

                rotation: new Quaternion(-pose.Orientation.x,
                                         -pose.Orientation.y,
                                         pose.Orientation.z,
                                         pose.Orientation.w)
            );
        }
    }
}