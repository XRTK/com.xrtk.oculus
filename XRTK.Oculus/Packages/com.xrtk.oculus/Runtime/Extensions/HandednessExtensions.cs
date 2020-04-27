// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using static XRTK.Oculus.OculusApi;

namespace XRTK.Oculus.Extensions
{
    public static class HandednessExtensions
    {
        /// <summary>
        /// Gets a <see cref="Hand"/> from the <see cref="Definitions.Utilities.Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>
        /// <returns><see cref="Hand"/></returns>
        public static Hand ToHand(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return Hand.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return Hand.HandRight;
                default:
                    return Hand.None;
            }
        }

        /// <summary>
        /// Gets a <see cref="Node"/> from the <see cref="Definitions.Utilities.Handedness"/>.
        /// </summary>
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>
        /// <returns><see cref="Node"/></returns>
        public static Node ToNode(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return Node.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return Node.HandRight;
                default:
                    return Node.None;
            }
        }

        /// <summary>
        /// Gets a <see cref="MeshType"/> from the <see cref="Definitions.Utilities.Handedness"/>.	
        /// </summary>	
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>	
        /// <returns><see cref="MeshType"/></returns>	
        public static MeshType ToMeshType(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return MeshType.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return MeshType.HandRight;
                default:
                    return MeshType.None;
            }
        }

        /// <summary>	
        /// Gets an <see cref="SkeletonType"/> from the <see cref="Definitions.Utilities.Handedness"/>.	
        /// </summary>	
        /// <param name="handedness"><see cref="Definitions.Utilities.Handedness"/> to convert.</param>	
        /// <returns><see cref="SkeletonType"/></returns>	
        public static SkeletonType ToSkeletonType(this Definitions.Utilities.Handedness handedness)
        {
            switch (handedness)
            {
                case Definitions.Utilities.Handedness.Left:
                    return SkeletonType.HandLeft;
                case Definitions.Utilities.Handedness.Right:
                    return SkeletonType.HandRight;
                default:
                    return SkeletonType.None;
            }
        }
    }
}