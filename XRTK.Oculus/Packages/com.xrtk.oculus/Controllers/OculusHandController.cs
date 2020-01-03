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
                UpdateHandJoints(updatedHandData.Joints);
                UpdateHandMesh(updatedHandData.Mesh);
            }

            UpdateBase(updatedHandData);
        }

        private void UpdateHandJoints(MixedRealityPose[] jointPoses)
        {
            for (int i = 0; i < jointPoses.Length; i++)
            {
                TrackedHandJoint trackedHandJoint = (TrackedHandJoint)i;
                switch (trackedHandJoint)
                {
                    // Wrist and Palm
                    case TrackedHandJoint.Wrist:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]);
                        break;
                    case TrackedHandJoint.Palm:
                        jointPoses[i] = ComputePalmPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot], skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
                        break;
                    // Finger: Thumb
                    case TrackedHandJoint.ThumbMetacarpalJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
                        break;
                    case TrackedHandJoint.ThumbProximalJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2]);
                        break;
                    case TrackedHandJoint.ThumbDistalJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3]);
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip]);
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexKnuckle:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index1]);
                        break;
                    case TrackedHandJoint.IndexMiddleJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index2]);
                        break;
                    case TrackedHandJoint.IndexDistalJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Index3]);
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip]);
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleKnuckle:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
                        break;
                    case TrackedHandJoint.MiddleMiddleJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2]);
                        break;
                    case TrackedHandJoint.MiddleDistalJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip]);
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingKnuckle:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1]);
                        break;
                    case TrackedHandJoint.RingMiddleJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Ring2]);
                        break;
                    case TrackedHandJoint.RingDistalJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Ring3]);
                        break;
                    case TrackedHandJoint.RingTip:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip]);
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.PinkyKnuckle:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1]);
                        break;
                    case TrackedHandJoint.PinkyMiddleJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky2]);
                        break;
                    case TrackedHandJoint.PinkyDistalJoint:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky3]);
                        break;
                    case TrackedHandJoint.PinkyTip:
                        jointPoses[i] = ComputeJointPose(skeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip]);
                        break;
                }
            }
        }

        private void UpdateHandMesh(HandMeshData handMeshData)
        {
            // TODO: Update hand mesh data.
        }

        private MixedRealityPose ComputePalmPose(OculusApi.Bone wristRoot, OculusApi.Bone middleDistal)
        {
            MixedRealityPose wristRootPose = ComputeJointPose(wristRoot);
            MixedRealityPose middleDistalPose = ComputeJointPose(middleDistal);
            Vector3 palmPosition = Vector3.Lerp(wristRootPose.Position, middleDistalPose.Position, .5f);
            Quaternion palmRotation = wristRootPose.Rotation;

            return new MixedRealityPose(palmPosition, palmRotation);
        }

        private MixedRealityPose ComputeJointPose(OculusApi.Bone bone)
        {
            Vector3 rootPosition = state.RootPose.Position.FromFlippedZVector3f();
            Quaternion rootRotation = state.RootPose.Orientation.FromFlippedZQuatf();

            Vector3 bonePosition = bone.Pose.Position.FromFlippedZVector3f();
            Quaternion boneRotation = state.BoneRotations[(int)bone.Id].FromFlippedZQuatf();

            bonePosition = rootPosition + rootRotation * bonePosition;
            boneRotation = rootRotation * boneRotation;

            return FixRotation(new MixedRealityPose(bonePosition, boneRotation));
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