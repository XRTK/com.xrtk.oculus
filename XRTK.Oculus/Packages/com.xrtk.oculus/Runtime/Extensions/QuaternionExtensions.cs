// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace XRTK.Oculus.Extensions
{
    public static class QuaternionExtensions
    {
        /// <summary>
        /// Gets a Unity quaternion from a flipped Z oculus orientation.
        /// </summary>
        /// <param name="q">Input orientation.</param>
        /// <returns>Unity orientation.</returns>
        public static Quaternion FromFlippedZQuatf(this OculusApi.Quatf q)
        {
            return new Quaternion() { x = -q.x, y = -q.y, z = q.z, w = q.w };
        }
    }
}