// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Extensions;
using XRTK.Oculus.Plugins;
using XRTK.Providers.Controllers.Hands;
using XRTK.Services;
using XRTK.Utilities;

namespace XRTK.Oculus.Utilities
{
    /// <summary>
    /// Converts oculus hand data to <see cref="HandData"/>.
    /// </summary>
    public sealed class OculusHandDataConverter : BaseHandDataConverter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handedness">Handedness of the hand this converter is created for.</param>
        /// <param name="trackedPoses">The tracked poses collection to use for pose recognition.</param>
        public OculusHandDataConverter(Handedness handedness, IReadOnlyList<HandControllerPoseDefinition> trackedPoses) : base(handedness, trackedPoses)
        { }

        private readonly Dictionary<int, Transform> boneProxyTransforms = new Dictionary<int, Transform>();

        private bool isInitialized = false;
        private OculusApi.Skeleton handSkeleton = new OculusApi.Skeleton();
        private OculusApi.HandState handState = new OculusApi.HandState();
        private OculusApi.Mesh handMesh = new OculusApi.Mesh();

        /// <inheritdoc />
        protected override bool PlatformProvidesPointerPose => true;

        /// <inheritdoc />
        protected override bool PlatformProvidesIsPinching => false;

        /// <inheritdoc />
        protected override bool PlatformProvidesPinchStrength => false;

        /// <inheritdoc />
        protected override bool PlatformProvidesIsPointing => false;

        /// <summary>
        /// Gets or sets whether hand mesh data should be read and converted.
        /// </summary>
        public static bool HandMeshingEnabled { get; set; }

        /// <summary>
        /// Reads hand data for the current frame and converts it to agnostic hand data.
        /// </summary>
        /// <returns>Updated hand data.</returns>
        public HandData GetHandData(OculusApi.HandState hand)
        {
            if (!isInitialized)
            {
                isInitialized = OculusApi.GetSkeleton(Handedness.ToSkeletonType(), out handSkeleton);
                if (!isInitialized)
                {
                    Debug.LogError($"{GetType().Name} - {Handedness} failed to initialize.");
                    return null;
                }
            }

            HandData updatedHandData = new HandData
            {
                IsTracked = OculusApi.GetHandState(OculusApi.Step.Render, Handedness.ToHand(), ref handState),
                TimeStamp = DateTimeOffset.UtcNow.Ticks
            };

            if (updatedHandData.IsTracked)
            {
                UpdateHandJoints(updatedHandData.Joints);

                if (HandMeshingEnabled && TryGetUpdatedHandMeshData(out HandMeshData data))
                {
                    updatedHandData.Mesh = data;
                }
                else
                {
                    updatedHandData.Mesh = new HandMeshData();
                }

                updatedHandData.PointerPose = ComputePointerPose(hand);
            }

            Finalize(updatedHandData);
            return updatedHandData;
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
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]);
                        break;
                    case TrackedHandJoint.Palm:
                        jointPoses[i] = EstimatePalmPose();
                        break;
                    // Finger: Thumb
                    case TrackedHandJoint.ThumbMetacarpalJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
                        break;
                    case TrackedHandJoint.ThumbProximalJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2]);
                        break;
                    case TrackedHandJoint.ThumbDistalJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3]);
                        break;
                    case TrackedHandJoint.ThumbTip:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip]);
                        break;
                    // Finger: Index
                    case TrackedHandJoint.IndexMetacarpal:
                        jointPoses[i] = EstimateIndexMetacarpal();
                        break;
                    case TrackedHandJoint.IndexKnuckle:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index1]);
                        break;
                    case TrackedHandJoint.IndexMiddleJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index2]);
                        break;
                    case TrackedHandJoint.IndexDistalJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index3]);
                        break;
                    case TrackedHandJoint.IndexTip:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip]);
                        break;
                    // Finger: Middle
                    case TrackedHandJoint.MiddleMetacarpal:
                        jointPoses[i] = EstimateMiddleMetacarpal();
                        break;
                    case TrackedHandJoint.MiddleKnuckle:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
                        break;
                    case TrackedHandJoint.MiddleMiddleJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2]);
                        break;
                    case TrackedHandJoint.MiddleDistalJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
                        break;
                    case TrackedHandJoint.MiddleTip:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip]);
                        break;
                    // Finger: Ring
                    case TrackedHandJoint.RingMetacarpal:
                        jointPoses[i] = EstimateRingMetacarpal();
                        break;
                    case TrackedHandJoint.RingKnuckle:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1]);
                        break;
                    case TrackedHandJoint.RingMiddleJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring2]);
                        break;
                    case TrackedHandJoint.RingDistalJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring3]);
                        break;
                    case TrackedHandJoint.RingTip:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip]);
                        break;
                    // Finger: Pinky
                    case TrackedHandJoint.PinkyMetacarpal:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
                        break;
                    case TrackedHandJoint.PinkyKnuckle:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1]);
                        break;
                    case TrackedHandJoint.PinkyMiddleJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky2]);
                        break;
                    case TrackedHandJoint.PinkyDistalJoint:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky3]);
                        break;
                    case TrackedHandJoint.PinkyTip:
                        jointPoses[i] = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip]);
                        break;
                }
            }
        }

        private bool TryGetUpdatedHandMeshData(out HandMeshData data)
        {
            if (OculusApi.GetMesh(Handedness.ToMeshType(), out handMesh))
            {
                Vector3[] vertices = new Vector3[handMesh.NumVertices];

                for (int i = 0; i < handMesh.NumVertices; ++i)
                {
                    vertices[i] = handMesh.VertexPositions[i].FromFlippedZVector3f();
                }

                Vector2[] uvs = new Vector2[handMesh.NumVertices];

                for (int i = 0; i < handMesh.NumVertices; ++i)
                {
                    uvs[i] = new Vector2(handMesh.VertexUV0[i].x, -handMesh.VertexUV0[i].y);
                }

                int[] triangles = new int[handMesh.NumIndices];

                for (int i = 0; i < handMesh.NumIndices; ++i)
                {
                    triangles[i] = handMesh.Indices[handMesh.NumIndices - i - 1];
                }

                Vector3[] normals = new Vector3[handMesh.NumVertices];

                for (int i = 0; i < handMesh.NumVertices; ++i)
                {
                    normals[i] = handMesh.VertexNormals[i].FromFlippedZVector3f();
                }

                data = new HandMeshData(vertices, triangles, normals, uvs, handState.RootPose.Position.FromFlippedZVector3f(), handState.RootPose.Orientation.FromFlippedZQuatf());

                return true;
            }

            data = default;
            return false;
        }

        private MixedRealityPose EstimateIndexMetacarpal()
        {
            MixedRealityPose thumbMetacarpalPose = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
            MixedRealityPose pinkyMetacarpalPose = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            Vector3 indexMetacarpalPosition = Vector3.Lerp(thumbMetacarpalPose.Position, pinkyMetacarpalPose.Position, .2f);
            Quaternion indexMetacarpalRotation = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]).Rotation;

            return new MixedRealityPose(indexMetacarpalPosition, indexMetacarpalRotation);
        }

        private MixedRealityPose EstimateRingMetacarpal()
        {
            MixedRealityPose thumbMetacarpalPose = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
            MixedRealityPose pinkyMetacarpalPose = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            Vector3 ringMetacarpalPosition = Vector3.Lerp(thumbMetacarpalPose.Position, pinkyMetacarpalPose.Position, .8f);
            Quaternion ringMetacarpalRotation = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]).Rotation;

            return new MixedRealityPose(ringMetacarpalPosition, ringMetacarpalRotation);
        }

        private MixedRealityPose EstimateMiddleMetacarpal()
        {
            MixedRealityPose thumbMetacarpalPose = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
            MixedRealityPose pinkyMetacarpalPose = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            Vector3 middleMetacarpalPosition = Vector3.Lerp(thumbMetacarpalPose.Position, pinkyMetacarpalPose.Position, .5f);
            Quaternion middleMetacarpalRotation = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]).Rotation;

            return new MixedRealityPose(middleMetacarpalPosition, middleMetacarpalRotation);
        }

        private MixedRealityPose EstimatePalmPose()
        {
            MixedRealityPose middleMetacarpalPose = EstimateMiddleMetacarpal();
            MixedRealityPose middleKnucklePose = ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
            Vector3 palmPosition = Vector3.Lerp(middleMetacarpalPose.Position, middleKnucklePose.Position, .5f);
            Quaternion palmRotation = middleMetacarpalPose.Rotation;

            return new MixedRealityPose(palmPosition, palmRotation);
        }

        private MixedRealityPose ComputeJointPose(OculusApi.Bone bone)
        {
            // The Pinky/Thumb 1+ bones depend on the Pinky/Thumb 0 bone
            // to be availble, which the XRTK hand tracking does not use. We still gotta update them to
            // be able to resolve pose dependencies.
            if (bone.Id == OculusApi.BoneId.Hand_Thumb1)
            {
                ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb0]);
            }
            else if (bone.Id == OculusApi.BoneId.Hand_Pinky1)
            {
                ComputeJointPose(handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            }

            Transform proxyTransform = GetProxyTransform(bone.Id);
            Transform parentProxyTransform = GetProxyTransform((OculusApi.BoneId)bone.ParentBoneIndex);

            if (parentProxyTransform == null)
            {
                Vector3 rootPosition = handState.RootPose.Position.FromFlippedZVector3f();
                proxyTransform.position = rootPosition;
                proxyTransform.rotation = handState.RootPose.Orientation.FromFlippedZQuatf();
            }
            else
            {
                proxyTransform.parent = parentProxyTransform;
                proxyTransform.localPosition = bone.Pose.Position.FromFlippedZVector3f();
                proxyTransform.localRotation = handState.BoneRotations[(int)bone.Id].FromFlippedZQuatf();
            }

            // Compute final bone pose.
            return TranslateToCameraSpace(FixRotation(new MixedRealityPose(proxyTransform.position, proxyTransform.rotation)));
        }

        private MixedRealityPose FixRotation(MixedRealityPose bonePose)
        {
            // WARNING THIS CODE IS SUBJECT TO CHANGE WITH THE OCULUS SDK
            // - This fix is a hack to fix broken and inconsistent rotations for hands.
            if (Handedness == Handedness.Left)
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

        private Transform GetProxyTransform(OculusApi.BoneId boneId)
        {
            if (boneId == OculusApi.BoneId.Invalid)
            {
                return null;
            }

            if (boneProxyTransforms.ContainsKey((int)boneId))
            {
                return boneProxyTransforms[(int)boneId];
            }

            var transform = new GameObject($"Oculus Hand {Handedness} {boneId} Proxy").transform;
            boneProxyTransforms.Add((int)boneId, transform);

            return transform;
        }

        /// <summary>
        /// Gets the hand's pointer pose mixed reality playspace coordinates.
        /// </summary>
        /// <param name="hand">The hand state provided by native APIs.</param>
        /// <returns>Pointer pose in playspace tracking space.</returns>
        private MixedRealityPose ComputePointerPose(OculusApi.HandState hand)
        {
            var platformPointerPose = hand.PointerPose.ToMixedRealityPose();
            var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
            var pointerForward = playspaceTransform.TransformDirection(platformPointerPose.Forward);
            var pointerUp = playspaceTransform.TransformDirection(platformPointerPose.Up);

            var pointerPosition = playspaceTransform.TransformPoint(platformPointerPose.Position);
            var pointerRotation = Quaternion.LookRotation(pointerForward, pointerUp);

            return TranslateToCameraSpace(new MixedRealityPose(pointerPosition, pointerRotation));
        }

        private MixedRealityPose TranslateToCameraSpace(MixedRealityPose pose)
        {
            var cameraTransform = MixedRealityToolkit.CameraSystem != null
                ? MixedRealityToolkit.CameraSystem.MainCameraRig.PlayerCamera.transform
                : CameraCache.Main.transform;

            var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
            pose.Position = playspaceTransform.TransformPoint(pose.Position) + new Vector3(0, cameraTransform.position.y, 0f);

            return pose;
        }
    }
}