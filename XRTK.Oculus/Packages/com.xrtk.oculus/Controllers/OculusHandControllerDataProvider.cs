// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Profiles;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Oculus.Controllers
{
    public class OculusHandControllerDataProvider : BaseHandControllerDataProvider
    {
        private readonly OculusHandControllerDataProviderProfile profile;
        private OculusApi.Skeleton leftHandSkeleton = new OculusApi.Skeleton();
        private OculusApi.HandState leftHandState = new OculusApi.HandState();
        private OculusApi.Skeleton rightHandSkeleton = new OculusApi.Skeleton();
        private OculusApi.HandState rightHandState = new OculusApi.HandState();

        /// <summary>
        /// Creates a new instance of the data provider.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Hand controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public OculusHandControllerDataProvider(string name, uint priority, OculusHandControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
            this.profile = profile;
        }

#if UNITY_ANDROID

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (profile.HandTrackingEnabled && OculusApi.GetHandTrackingEnabled())
            {
                OculusApi.GetSkeleton(OculusApi.SkeletonType.HandLeft, out leftHandSkeleton);
                OculusApi.GetSkeleton(OculusApi.SkeletonType.HandRight, out rightHandSkeleton);
            }
        }

        /// <inheritdoc />
        public override void LateUpdate()
        {
            base.LateUpdate();

            if (profile.HandTrackingEnabled && OculusApi.GetHandTrackingEnabled())
            {
                UpdateHandController(OculusApi.Hand.HandLeft, ref leftHandState);
                UpdateHandController(OculusApi.Hand.HandRight, ref rightHandState);
            }
        }

        private void UpdateHandController(OculusApi.Hand hand, ref OculusApi.HandState state)
        {
            OculusApi.GetHandState(OculusApi.Step.Render, hand, ref state);
            HandData updatedHandData = new HandData
            {
                IsTracked = state.Status == OculusApi.HandStatus.HandTracked,
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {

            }

            // Update provider base implementation
            UpdateHandData(ConvertHandedness(hand), updatedHandData);
        }

        private Handedness ConvertHandedness(OculusApi.Hand hand)
        {
            switch (hand)
            {
                case OculusApi.Hand.HandLeft:
                    return Handedness.Left;
                case OculusApi.Hand.HandRight:
                    return Handedness.Right;
                case OculusApi.Hand.None:
                default:
                    return Handedness.None;
            }
        }

#endif
    }
}