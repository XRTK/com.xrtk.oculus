// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Oculus.Plugins;

namespace XRTK.Oculus.Extensions
{
    public static class HandednessExtensions
    {
        /// <summary>
        /// Gets a <see cref="OculusApi.Hand"/> from the <see cref="Definitions.Utilities.Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>
        /// <returns><see cref="OculusApi.Hand"/></returns>
        public static OculusApi.Hand ToHand(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return OculusApi.Hand.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return OculusApi.Hand.HandRight;
                default:
                    return OculusApi.Hand.None;
            }
        }

        /// <summary>
        /// Gets a <see cref="OculusApi.Node"/> from the <see cref="Definitions.Utilities.Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>
        /// <returns><see cref="OculusApi.Node"/></returns>
        public static OculusApi.Node ToNode(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return OculusApi.Node.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return OculusApi.Node.HandRight;
                default:
                    return OculusApi.Node.None;
            }
        }

        /// <summary>
        /// Gets a <see cref="OculusApi.MeshType"/> from the <see cref="Definitions.Utilities.Handedness"/>.	
        /// </summary>	
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>	
        /// <returns><see cref="OculusApi.MeshType"/></returns>	
        public static OculusApi.MeshType ToMeshType(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return OculusApi.MeshType.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return OculusApi.MeshType.HandRight;
                default:
                    return OculusApi.MeshType.None;
            }
        }

        /// <summary>	
        /// Gets an <see cref="OculusApi.SkeletonType"/> from the <see cref="Definitions.Utilities.Handedness"/>.	
        /// </summary>	
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>	
        /// <returns><see cref="OculusApi.SkeletonType"/></returns>	
        public static OculusApi.SkeletonType ToSkeletonType(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return OculusApi.SkeletonType.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return OculusApi.SkeletonType.HandRight;
                default:
                    return OculusApi.SkeletonType.None;
            }
        }
    }
}