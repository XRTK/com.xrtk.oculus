// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;

namespace XRTK.Oculus.Profiles
{
    /// <summary>
    /// Configuration profile for Oculus hand controllers.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Data Providers/Oculus Hand Controller Data Provider Profile", fileName = "OculusHandControllerDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class OculusHandControllerDataProviderProfile : BaseHandControllerDataProviderProfile
    {
        [Header("Oculus Platform Settings")]

        [SerializeField]
        [Tooltip("The minimum hand tracking confidence expected.")]
        private int minConfidenceRequired = 1;

        /// <summary>
        /// The minimum hand tracking confidence expected.
        /// </summary>
        public int MinConfidenceRequired => minConfidenceRequired;
    }
}