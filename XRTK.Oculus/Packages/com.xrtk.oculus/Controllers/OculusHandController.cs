// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Oculus.Controllers
{
    /// <summary>
    /// The default hand controller implementation for the Oculus platform.
    /// </summary>
    public class OculusHandController : BaseHandController
    {
        private bool isInitialized = false;
        private OculusApi.Skeleton handSkeleton = new OculusApi.Skeleton();
        private OculusApi.HandState handState = new OculusApi.HandState();

        /// <summary>
        /// Controller constructor.
        /// </summary>
        /// <param name="trackingState">The controller's tracking state.</param>
        /// <param name="controllerHandedness">The controller's handedness.</param>
        /// <param name="inputSource">Optional input source of the controller.</param>
        /// <param name="interactions">Optional controller interactions mappings.</param>
        public OculusHandController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions) { }

        /// <inheritdoc />
        public override void UpdateController()
        {
            base.UpdateController();

            if (!isInitialized)
            {
                Initialize();
            }

            HandData updatedHandData = new HandData
            {
                IsTracked = true,
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
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]);
                            break;
                        case TrackedHandJoint.ThumbProximalJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2]);
                            break;
                        case TrackedHandJoint.ThumbDistalJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3]);
                            break;
                        case TrackedHandJoint.ThumbTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip]);
                            break;
                        case TrackedHandJoint.IndexKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index1]);
                            break;
                        case TrackedHandJoint.IndexMiddleJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index2]);
                            break;
                        case TrackedHandJoint.IndexTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip]);
                            break;
                        case TrackedHandJoint.MiddleKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
                            break;
                        case TrackedHandJoint.MiddleMiddleJoint:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2]);
                            break;
                        case TrackedHandJoint.MiddleTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip]);
                            break;
                        case TrackedHandJoint.RingKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1]);
                            break;
                        case TrackedHandJoint.RingTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip]);
                            break;
                        case TrackedHandJoint.PinkyKnuckle:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1]);
                            break;
                        case TrackedHandJoint.PinkyTip:
                            updatedHandData.Joints[i] = ConvertBonePoseToJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip]);
                            break;
                    }
                }
            }

            UpdateBase(updatedHandData);
        }

        private void Initialize()
        {
            OculusApi.GetSkeleton(OculusApi.SkeletonType.HandLeft, out handSkeleton);
            isInitialized = true;
        }

        private MixedRealityPose ConvertBonePoseToJointPose(OculusApi.Bone bone)
        {
            MixedRealityPose pose = bone.Pose.ToMixedRealityPose(true);
            pose.Position += handState.RootPose.Position.FromFlippedZVector3f().ToUnityVector3();
            pose.Rotation = bone.Pose.Orientation.FromFlippedZQuatf();

            return pose;
        }
    }
}