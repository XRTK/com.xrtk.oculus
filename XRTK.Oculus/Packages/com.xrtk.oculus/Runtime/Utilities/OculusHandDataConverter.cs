// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Extensions;
using XRTK.Oculus.Plugins;
using XRTK.Services;
using XRTK.Utilities;

namespace XRTK.Oculus.Utilities
{
    /// <summary>
    /// Converts oculus hand data to <see cref="HandData"/>.
    /// </summary>
    public sealed class OculusHandDataConverter
    {
        private readonly Dictionary<OculusApi.BoneId, Transform> boneProxyTransforms = new Dictionary<OculusApi.BoneId, Transform>();

        private OculusApi.Skeleton handSkeleton = new OculusApi.Skeleton();
        private OculusApi.HandState handState = new OculusApi.HandState();
        private OculusApi.Mesh handMesh = new OculusApi.Mesh();

        /// <summary>
        /// Reads hand APIs for the current frame and converts it to agnostic <see cref="HandData"/>.
        /// </summary>
        /// <param name="handedness">The handedness of the hand to get <see cref="HandData"/> for.</param>
        /// <param name="includeMeshData">If set, hand mesh information will be included in <see cref="HandData.Mesh"/>.</param>
        /// <param name="minTrackingConfidence">The minimum <see cref="OculusApi.TrackingConfidence"/> required to consider hands tracked.</param>
        /// <param name="handData">The output <see cref="HandData"/>.</param>
        /// <returns>True, if data conversion was a success.</returns>
        public bool TryGetHandData(Handedness handedness, bool includeMeshData, OculusApi.TrackingConfidence minTrackingConfidence, out HandData handData)
        {
            // Here we check whether the hand is being tracked at all by the Oculus system.
            if (!(OculusApi.GetHandState(OculusApi.Step.Render, handedness.ToHand(), ref handState) &&
                OculusApi.GetSkeleton(handedness.ToSkeletonType(), out handSkeleton)))
            {
                handData = default;
                return false;
            }

            // The hand is being tracked, next we verify it meets our confidence requirements to consider
            // it tracked.
            handData = new HandData
            {
                IsTracked = handState.HandConfidence >= minTrackingConfidence && (handState.Status & OculusApi.HandStatus.HandTracked) != 0,
                UpdatedAt = DateTimeOffset.UtcNow.Ticks
            };

            // If the hand is tracked per requirements, we get updated joint data
            // and other data needed for updating the hand controller's state.
            if (handData.IsTracked)
            {
                handData.RootPose = GetHandRootPose(handedness);
                handData.Joints = GetJointPoses(handedness, handData.RootPose);

                if (includeMeshData && TryGetUpdatedHandMeshData(handedness, out HandMeshData data))
                {
                    handData.Mesh = data;
                }
                else
                {
                    handData.Mesh = default;
                }

                handData.PointerPose = ComputePointerPose(handedness);
            }

            // Even if the hand is being tracked by the system but the confidence did not
            // meet our requirements, we return true. This allows the hand controller and visualizers
            // to react to tracking loss and keep the hand up for a given time before destroying the controller.
            return true;
        }

        private MixedRealityPose[] GetJointPoses(Handedness handedness, MixedRealityPose handRootPose)
        {
            var jointPoses = new MixedRealityPose[HandData.JointCount];

            jointPoses[(int)TrackedHandJoint.Wrist] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]);

            jointPoses[(int)TrackedHandJoint.ThumbMetacarpal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
            jointPoses[(int)TrackedHandJoint.ThumbProximal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2]);
            jointPoses[(int)TrackedHandJoint.ThumbDistal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3]);
            jointPoses[(int)TrackedHandJoint.ThumbTip] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip]);

            jointPoses[(int)TrackedHandJoint.IndexProximal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index1]);
            jointPoses[(int)TrackedHandJoint.IndexIntermediate] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index2]);
            jointPoses[(int)TrackedHandJoint.IndexDistal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index3]);
            jointPoses[(int)TrackedHandJoint.IndexTip] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip]);

            jointPoses[(int)TrackedHandJoint.MiddleProximal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
            jointPoses[(int)TrackedHandJoint.MiddleIntermediate] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2]);
            jointPoses[(int)TrackedHandJoint.MiddleDistal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
            jointPoses[(int)TrackedHandJoint.MiddleTip] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip]);

            jointPoses[(int)TrackedHandJoint.RingProximal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1]);
            jointPoses[(int)TrackedHandJoint.RingIntermediate] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring2]);
            jointPoses[(int)TrackedHandJoint.RingDistal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring3]);
            jointPoses[(int)TrackedHandJoint.RingTip] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip]);

            jointPoses[(int)TrackedHandJoint.LittleMetacarpal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            jointPoses[(int)TrackedHandJoint.LittleProximal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1]);
            jointPoses[(int)TrackedHandJoint.LittleIntermediate] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky2]);
            jointPoses[(int)TrackedHandJoint.LittleDistal] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky3]);
            jointPoses[(int)TrackedHandJoint.LittleTip] = ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip]);

            // Estimated: These joint poses are not provided by the Ouclus
            // hand tracking implementation. But with the data we now have, we can
            // estimate their poses fairly well.
            jointPoses[(int)TrackedHandJoint.Palm] = HandUtilities.GetEstimatedPalmPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.IndexMetacarpal] = HandUtilities.GetEstimatedIndexMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.MiddleMetacarpal] = HandUtilities.GetEstimatedMiddleMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.RingMetacarpal] = HandUtilities.GetEstimatedRingMetacarpalPose(jointPoses);

            return jointPoses;
        }

        private bool TryGetUpdatedHandMeshData(Handedness handedness, out HandMeshData data)
        {
            if (OculusApi.GetMesh(handedness.ToMeshType(), out handMesh))
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

                data = new HandMeshData(vertices, triangles, normals, uvs);

                return true;
            }

            data = default;
            return false;
        }

        private MixedRealityPose ComputeJointPose(Handedness handedness, MixedRealityPose handRootPose, OculusApi.Bone bone)
        {
            // The Pinky/Thumb 1+ bones depend on the Pinky/Thumb 0 bone
            // to be available, which the XRTK hand tracking does not use. We still gotta update them to
            // be able to resolve pose dependencies.
            if (bone.Id == OculusApi.BoneId.Hand_Thumb1)
            {
                ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb0]);
            }
            else if (bone.Id == OculusApi.BoneId.Hand_Pinky1)
            {
                ComputeJointPose(handedness, handRootPose, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            }

            var rootBoneTransform = GetProxyTransform(handedness, OculusApi.BoneId.Hand_Start);
            var boneProxyTransform = GetProxyTransform(handedness, bone.Id);
            var parentProxyTransform = GetProxyTransform(handedness, (OculusApi.BoneId)bone.ParentBoneIndex);

            boneProxyTransform.SetParent(parentProxyTransform, false);
            boneProxyTransform.localPosition = bone.Pose.Position.FromFlippedZVector3f();
            boneProxyTransform.localRotation = handState.BoneRotations[(int)bone.Id].FromFlippedZQuatf();

            return FixRotation(handedness, new MixedRealityPose(
                rootBoneTransform.InverseTransformPoint(boneProxyTransform.position),
                Quaternion.Inverse(rootBoneTransform.rotation) * boneProxyTransform.rotation));
        }

        /// <summary>
        /// WARNING THIS CODE IS SUBJECT TO CHANGE WITH THE OCULUS SDK.
        /// This fix is a hack to fix broken and inconsistent rotations for hands.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the pose belongs to.</param>
        /// <param name="jointPose">The joint pose to apply the fix to.</param>
        /// <returns>Joint pose with fixed rotation.</returns>
        private MixedRealityPose FixRotation(Handedness handedness, MixedRealityPose jointPose)
        {
            if (handedness == Handedness.Left)
            {
                // Rotate bone 180 degrees on X to flip up.
                jointPose.Rotation *= Quaternion.Euler(180f, 0f, 0f);

                // Rotate bone 90 degrees on Y to align X with right.
                jointPose.Rotation *= Quaternion.Euler(0f, 90f, 0f);
            }
            else
            {
                // Rotate bone 90 degrees on Y to align X with left.
                jointPose.Rotation *= Quaternion.Euler(0f, -90f, 0f);
            }

            return jointPose;
        }

        /// <summary>
        /// THe oculus APIs return joint poses relative to their parent joint unlike
        /// other platforms where joint poses are relative to the hand root. To convert
        /// the joint-->parent-joint relation to hand-root-->joint relations proxy <see cref="Transform"/>s
        /// are used. The proxies are parented to their respective parent <see cref="Transform"/>.
        /// That way we can make use of Unity APIs to translate coordinate spaces.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the proxy <see cref="Transform"/> belongs to.</param>
        /// <param name="boneId">The Oculus bone ID to lookup the proxy <see cref="Transform"/> for.</param>
        /// <returns>The proxy <see cref="Transform"/>.</returns>
        private Transform GetProxyTransform(Handedness handedness, OculusApi.BoneId boneId)
        {
            if (boneId == OculusApi.BoneId.Invalid)
            {
                return MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
            }

            if (boneProxyTransforms.ContainsKey(boneId))
            {
                return boneProxyTransforms[boneId];
            }

            var transform = new GameObject($"Oculus {handedness} Hand {boneId} Proxy").transform;
            transform.gameObject.SetActive(false);
            boneProxyTransforms.Add(boneId, transform);

            return transform;
        }

        private MixedRealityPose GetHandRootPose(Handedness handedness)
        {
            var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
            var cameraTransform = MixedRealityToolkit.CameraSystem != null
                ? MixedRealityToolkit.CameraSystem.MainCameraRig.PlayerCamera.transform
                : CameraCache.Main.transform;

            var rootPosition = playspaceTransform.InverseTransformPoint(handState.RootPose.Position.FromFlippedZVector3f());
            var rootRotation = Quaternion.Inverse(playspaceTransform.rotation) * handState.RootPose.Orientation.FromFlippedZQuatf();

            return FixRotation(handedness, new MixedRealityPose(rootPosition + new Vector3(0f, cameraTransform.localPosition.y, 0f), rootRotation));
        }

        /// <summary>
        /// Gets the hand's local pointer pose.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the pose belongs to.</param>
        /// <returns>Pointer pose relative to <see cref="HandData.RootPose"/>.</returns>
        private MixedRealityPose ComputePointerPose(Handedness handedness)
        {
            var platformPointerPose = new MixedRealityPose(
                handState.PointerPose.Position.FromFlippedZVector3f(),
                handState.PointerPose.Orientation.FromFlippedZQuatf());

            return FixRotation(handedness, platformPointerPose);
        }
    }
}