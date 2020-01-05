// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;

namespace XRTK.Oculus.Profiles
{
    /// <summary>
    /// Configuration profile for Oculus controllers.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Data Providers/Oculus Controller Data Provider Profile", fileName = "OculusControllerDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class OculusControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        [Header("Hand Tracking")]
        [SerializeField]
        [Tooltip("Enable hand tracking")]
        private bool handTrackingEnabled = true;

        /// <summary>
        /// Is hand tracking enabled?
        /// </summary>
        public bool HandTrackingEnabled => handTrackingEnabled;

        [SerializeField]
        [Tooltip("The minimum hand tracking confidence expected.")]
        private OculusApi.TrackingConfidence minConfidenceRequired = OculusApi.TrackingConfidence.High;

        /// <summary>
        /// The minimum hand tracking confidence expected.
        /// </summary>
        public OculusApi.TrackingConfidence MinConfidenceRequired => minConfidenceRequired;
    }
}