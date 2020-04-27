// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Oculus.Extensions
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Gets a <see cref="UnityEngine.Vector3"/> position from the <see cref="OculusApi.Posef"/>.
        /// </summary>
        /// <param name="pose"></param>
        public static Vector3 GetPosePosition(this OculusApi.Posef pose)
        {
            return new Vector3(pose.Position.x, pose.Position.y, -pose.Position.z);
        }

        /// <summary>
        /// Flips the z-axis of the vector.
        /// </summary>
        /// <param name="v">Input vector.</param>
        /// <returns>Vector with flipped z axis.</returns>
        public static Vector3 FromFlippedZVector3f(this OculusApi.Vector3f v)
        {
            return new Vector3() { x = v.x, y = v.y, z = -v.z };
        }
    }
}