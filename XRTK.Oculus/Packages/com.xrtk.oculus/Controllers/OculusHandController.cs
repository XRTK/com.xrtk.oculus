// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
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
        private OculusApi.Skeleton skeleton = new OculusApi.Skeleton();
        private OculusApi.HandState state = new OculusApi.HandState();

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
                isInitialized = OculusApi.GetSkeleton(ControllerHandedness.ToSkeletonType(), out skeleton);
                if (!isInitialized)
                {
                    Debug.LogError($"{GetType().Name} - {ControllerHandedness} failed to initialize.");
                    return;
                }
            }

            HandData updatedHandData = new HandData
            {
                IsTracked = OculusApi.GetHandState(OculusApi.Step.Render, ControllerHandedness.ToHand(), ref state),
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {
                for (int i = 0; i < updatedHandData.Joints.Length; i++)
                {
                    TrackedHandJoint trackedHandJoint = (TrackedHandJoint)i;
                    switch (trackedHandJoint)
                    {
                        // Wrist and Palm
                        case TrackedHandJoint.Wrist:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]);
                            break;
                        case TrackedHandJoint.Palm:
                            updatedHandData.Joints[i] = ComputePalmPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot], skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
                            break;
                        // Finger: Thumb
                        case TrackedHandJoint.ThumbMetacarpalJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
                            break;
                        case TrackedHandJoint.ThumbProximalJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2]);
                            break;
                        case TrackedHandJoint.ThumbDistalJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3]);
                            break;
                        case TrackedHandJoint.ThumbTip:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip]);
                            break;
                        // Finger: Index
                        case TrackedHandJoint.IndexKnuckle:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index1]);
                            break;
                        case TrackedHandJoint.IndexMiddleJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index2]);
                            break;
                        case TrackedHandJoint.IndexDistalJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index3]);
                            break;
                        case TrackedHandJoint.IndexTip:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip]);
                            break;
                        // Finger: Middle
                        case TrackedHandJoint.MiddleKnuckle:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
                            break;
                        case TrackedHandJoint.MiddleMiddleJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2]);
                            break;
                        case TrackedHandJoint.MiddleDistalJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
                            break;
                        case TrackedHandJoint.MiddleTip:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip]);
                            break;
                        // Finger: Ring
                        case TrackedHandJoint.RingKnuckle:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1]);
                            break;
                        case TrackedHandJoint.RingMiddleJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Ring2]);
                            break;
                        case TrackedHandJoint.RingDistalJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Ring3]);
                            break;
                        case TrackedHandJoint.RingTip:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip]);
                            break;
                        // Finger: Pinky
                        case TrackedHandJoint.PinkyKnuckle:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1]);
                            break;
                        case TrackedHandJoint.PinkyMiddleJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky2]);
                            break;
                        case TrackedHandJoint.PinkyDistalJoint:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky3]);
                            break;
                        case TrackedHandJoint.PinkyTip:
                            updatedHandData.Joints[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip]);
                            break;
                    }
                }
            }

            UpdateBase(updatedHandData);
        }

        private MixedRealityPose ComputePalmPose(OculusApi.Bone wristRoot, OculusApi.Bone middleDistal)
        {
            Vector3 wristRootPosition = ComputeJointPose(wristRoot).Position;
            Vector3 middleDistalPosition = ComputeJointPose(middleDistal).Position;
            Vector3 palmPosition = Vector3.Lerp(wristRootPosition, middleDistalPosition, .5f);
            Quaternion palmRotation = state.BoneRotations[(int)wristRoot.Id].FromFlippedZQuatf();

            return FixRotation(new MixedRealityPose(palmPosition, palmRotation));
        }

        private MixedRealityPose ComputeJointPose(OculusApi.Bone bone)
        {
            Vector3 jointPosition;
            if (bone.ParentBoneIndex != (int)OculusApi.BoneId.Invalid)
            {
                MixedRealityPose parentJointPose = ComputeJointPose(skeleton.Bones[bone.ParentBoneIndex]);
                Vector3 jointDirection = parentJointPose.Rotation * parentJointPose.Position.normalized;
                jointDirection.Scale(bone.Pose.Position.FromFlippedZVector3f());

                jointPosition =
                    parentJointPose.Position +
                    jointDirection;
            }
            else
            {
                jointPosition =
                    state.RootPose.Position.FromFlippedZVector3f() +
                    bone.Pose.Position.FromFlippedZVector3f();
            }

            Quaternion jointRotation = state.BoneRotations[(int)bone.Id].FromFlippedZQuatf();

            return FixRotation(new MixedRealityPose(jointPosition, jointRotation));
        }

        private MixedRealityPose FixRotation(MixedRealityPose bonePose)
        {
            // WARNING THIS CODE IS SUBJECT TO CHANGE WITH THE OCULUS SDK
            // - This fix is a hack to fix broken and inconsistent rotations for hands.
            if (ControllerHandedness == Handedness.Left)
            {
                // Rotate bone 180 degrees on X to flip up.
                bonePose.Rotation *= Quaternion.Euler(180f, 0f, 0f);

                // Rotate bone 90 degrees on Y to align X with right.
                bonePose.Rotation *= Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                // Rotate bone 90 degrees on Y to align X with left.
                bonePose.Rotation *= Quaternion.Euler(0f, -90f, 0f);
            }

            return bonePose;
        }
    }
}