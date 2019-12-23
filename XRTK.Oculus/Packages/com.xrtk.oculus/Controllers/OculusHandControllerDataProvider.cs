// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
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
                UpdateHandController(OculusApi.Hand.HandLeft, ref leftHandState, ref leftHandSkeleton);
                UpdateHandController(OculusApi.Hand.HandRight, ref rightHandState, ref rightHandSkeleton);
            }
        }

        private void UpdateHandController(OculusApi.Hand hand, ref OculusApi.HandState state, ref OculusApi.Skeleton skeleton)
        {
            bool isTracked = OculusApi.GetHandState(OculusApi.Step.Render, hand, ref state)
                && (state.Status & OculusApi.HandStatus.HandTracked) != 0
                && state.HandConfidence == profile.MinConfidenceRequired;

            HandData updatedHandData = new HandData
            {
                IsTracked = isTracked,
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {
                for (int i = 0; i < updatedHandData.Joints.Length; i++)
                {
                    TrackedHandJoint trackedHandJoint = (TrackedHandJoint)i;
                    switch (trackedHandJoint)
                    {
                        case TrackedHandJoint.Wrist:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot], state);
                            break;
                        case TrackedHandJoint.ThumbProximalJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2], state);
                            break;
                        case TrackedHandJoint.ThumbDistalJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3], state);
                            break;
                        case TrackedHandJoint.ThumbTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip], state);
                            break;
                        case TrackedHandJoint.IndexKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index1], state);
                            break;
                        case TrackedHandJoint.IndexMiddleJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index2], state);
                            break;
                        case TrackedHandJoint.IndexTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip], state);
                            break;
                        case TrackedHandJoint.MiddleKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1], state);
                            break;
                        case TrackedHandJoint.MiddleMiddleJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2], state);
                            break;
                        case TrackedHandJoint.MiddleTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip], state);
                            break;
                        case TrackedHandJoint.RingKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1], state);
                            break;
                        case TrackedHandJoint.RingTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip], state);
                            break;
                        case TrackedHandJoint.PinkyKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1], state);
                            break;
                        case TrackedHandJoint.PinkyTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip], state);
                            break;
                    }
                }
            }

            // Update provider base implementation
            UpdateHandData(ConvertHandedness(hand), updatedHandData);
        }

        private MixedRealityPose ConvertBonePoseToJointPose(OculusApi.Bone bone, OculusApi.HandState handState)
        {
            MixedRealityPose pose = bone.Pose.ToMixedRealityPose(true);
            pose.Position += handState.RootPose.Position.FlipZVector3f().ToUnityVector3();

            return pose;
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