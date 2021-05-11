// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Plugins;

namespace XRTK.Oculus.Extensions
{
    public static class HandednessExtensions
    {
        /// <summary>
        /// Converts an <see cref="OculusApi.Controller"/> mask to a XRTK <see cref="Handedness"/>.
        /// </summary>
        /// <param name="controller">Controller mask.</param>
        /// <returns>Handedness.</returns>
        public static Handedness ToHandedness(this OculusApi.Controller controller)
        {
            switch (controller)
            {
                case OculusApi.Controller.LTouch:
                    return Handedness.Left;
                case OculusApi.Controller.RTouch:
                    return Handedness.Right;
                case OculusApi.Controller.Gamepad:
                case OculusApi.Controller.Remote:
                    return Handedness.Both;
                case OculusApi.Controller.LHand:
                    return Handedness.Left;
                case OculusApi.Controller.RHand:
                    return Handedness.Right;
                case OculusApi.Controller.Hands:
                    return Handedness.Both;
            }

            Debug.LogWarning($"{controller} does not have a defined controller handedness, falling back to {Handedness.None}");
            return Handedness.None;
        }

        /// <summary>
        /// Gets a <see cref="OculusApi.Hand"/> from the <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Handedness"/> to convert.</param>
        /// <returns><see cref="OculusApi.Hand"/></returns>
        public static OculusApi.Hand ToHand(this Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    return OculusApi.Hand.HandLeft;
                case Handedness.Right:
                    return OculusApi.Hand.HandRight;
                default:
                    return OculusApi.Hand.None;
            }
        }

        /// <summary>
        /// Gets a <see cref="OculusApi.Node"/> from the <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Handedness"/> to convert.</param>
        /// <returns><see cref="OculusApi.Node"/></returns>
        public static OculusApi.Node ToNode(this Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    return OculusApi.Node.HandLeft;
                case Handedness.Right:
                    return OculusApi.Node.HandRight;
                default:
                    return OculusApi.Node.None;
            }
        }

        /// <summary>
        /// Gets a <see cref="OculusApi.MeshType"/> from the <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Handedness"/> to convert.</param>
        /// <returns><see cref="OculusApi.MeshType"/></returns>
        public static OculusApi.MeshType ToMeshType(this Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    return OculusApi.MeshType.HandLeft;
                case Handedness.Right:
                    return OculusApi.MeshType.HandRight;
                default:
                    return OculusApi.MeshType.None;
            }
        }

        /// <summary>
        /// Gets an <see cref="OculusApi.SkeletonType"/> from the <see cref="Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Handedness"/> to convert.</param>
        /// <returns><see cref="OculusApi.SkeletonType"/></returns>
        public static OculusApi.SkeletonType ToSkeletonType(this Handedness handedness)
        {
            switch (handedness)
            {
                case Handedness.Left:
                    return OculusApi.SkeletonType.HandLeft;
                case Handedness.Right:
                    return OculusApi.SkeletonType.HandRight;
                default:
                    return OculusApi.SkeletonType.None;
            }
        }
    }
}