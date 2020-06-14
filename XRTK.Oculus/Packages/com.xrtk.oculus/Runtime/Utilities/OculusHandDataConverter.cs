// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Extensions;
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
        /// <summary>
        /// Destructor.
        /// </summary>
        ~OculusHandDataConverter()
        {
            if (!conversionProxyRootTransform.IsNull())
            {
                conversionProxyTransforms.Clear();
                conversionProxyRootTransform.Destroy();
            }
        }

        private Transform conversionProxyRootTransform;
        private readonly Dictionary<OculusApi.BoneId, Transform> conversionProxyTransforms = new Dictionary<OculusApi.BoneId, Transform>();

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
                TrackingState = (handState.HandConfidence >= minTrackingConfidence && (handState.Status & OculusApi.HandStatus.HandTracked) != 0) ? TrackingState.Tracked : TrackingState.NotTracked,
                UpdatedAt = DateTimeOffset.UtcNow.Ticks
            };

            // If the hand is tracked per requirements, we get updated joint data
            // and other data needed for updating the hand controller's state.
            if (handData.TrackingState == TrackingState.Tracked)
            {
                handData.RootPose = GetHandRootPose(handedness);
                handData.Joints = GetJointPoses(handedness);
                handData.PointerPose = GetPointerPose(handedness);

                if (includeMeshData && TryGetUpdatedHandMeshData(handedness, out HandMeshData data))
                {
                    handData.Mesh = data;
                }
                else
                {
                    handData.Mesh = HandMeshData.Empty;
                }
            }

            // Even if the hand is being tracked by the system but the confidence did not
            // meet our requirements, we return true. This allows the hand controller and visualizers
            // to react to tracking loss and keep the hand up for a given time before destroying the controller.
            return true;
        }

        /// <summary>
        /// Gets updated joint poses for all <see cref="TrackedHandJoint"/>s.
        /// </summary>
        /// <param name="handedness">Handedness of the hand to read joint poses for.</param>
        /// <returns>Joint poses in <see cref="TrackedHandJoint"/> order.</returns>
        private MixedRealityPose[] GetJointPoses(Handedness handedness)
        {
            var jointPoses = new MixedRealityPose[HandData.JointCount];

            jointPoses[(int)TrackedHandJoint.Wrist] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_WristRoot]);

            jointPoses[(int)TrackedHandJoint.ThumbMetacarpal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb1]);
            jointPoses[(int)TrackedHandJoint.ThumbProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb2]);
            jointPoses[(int)TrackedHandJoint.ThumbDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb3]);
            jointPoses[(int)TrackedHandJoint.ThumbTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_ThumbTip]);

            jointPoses[(int)TrackedHandJoint.IndexProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index1]);
            jointPoses[(int)TrackedHandJoint.IndexIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index2]);
            jointPoses[(int)TrackedHandJoint.IndexDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Index3]);
            jointPoses[(int)TrackedHandJoint.IndexTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_IndexTip]);

            jointPoses[(int)TrackedHandJoint.MiddleProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle1]);
            jointPoses[(int)TrackedHandJoint.MiddleIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle2]);
            jointPoses[(int)TrackedHandJoint.MiddleDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Middle3]);
            jointPoses[(int)TrackedHandJoint.MiddleTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_MiddleTip]);

            jointPoses[(int)TrackedHandJoint.RingProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring1]);
            jointPoses[(int)TrackedHandJoint.RingIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring2]);
            jointPoses[(int)TrackedHandJoint.RingDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Ring3]);
            jointPoses[(int)TrackedHandJoint.RingTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_RingTip]);

            jointPoses[(int)TrackedHandJoint.LittleMetacarpal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            jointPoses[(int)TrackedHandJoint.LittleProximal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky1]);
            jointPoses[(int)TrackedHandJoint.LittleIntermediate] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky2]);
            jointPoses[(int)TrackedHandJoint.LittleDistal] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky3]);
            jointPoses[(int)TrackedHandJoint.LittleTip] = GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_PinkyTip]);

            // Estimated: These joint poses are not provided by the Ouclus
            // hand tracking implementation. But with the data we now have, we can
            // estimate their poses fairly well.
            jointPoses[(int)TrackedHandJoint.Palm] = HandUtilities.GetEstimatedPalmPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.IndexMetacarpal] = HandUtilities.GetEstimatedIndexMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.MiddleMetacarpal] = HandUtilities.GetEstimatedMiddleMetacarpalPose(jointPoses);
            jointPoses[(int)TrackedHandJoint.RingMetacarpal] = HandUtilities.GetEstimatedRingMetacarpalPose(jointPoses);

            return jointPoses;
        }

        /// <summary>
        /// Attempts to get updated hand mesh data.
        /// </summary>
        /// <param name="handedness">The handedness of the hand to get mesh data for.</param>
        /// <param name="data">Mesh information retrieved in case of success.</param>
        /// <returns>True, if mesh data could be loaded.</returns>
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

        /// <summary>
        /// Gets a single joint's pose relative to the hand root pose.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the pose belongs to.</param>
        /// <param name="bone">Bone data retrieved from Oculus API with pose information.</param>
        /// <returns>Converted joint pose in hand space.</returns>
        private MixedRealityPose GetJointPose(Handedness handedness, OculusApi.Bone bone)
        {
            // The Pinky/Thumb 1+ bones depend on the Pinky/Thumb 0 bone
            // to be available, which the XRTK hand tracking does not use. We still have to compute them to
            // be able to resolve pose relation dependencies.
            if (bone.Id == OculusApi.BoneId.Hand_Thumb1)
            {
                GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Thumb0]);
            }
            else if (bone.Id == OculusApi.BoneId.Hand_Pinky1)
            {
                GetJointPose(handedness, handSkeleton.Bones[(int)OculusApi.BoneId.Hand_Pinky0]);
            }

            var boneProxyTransform = GetProxyTransform(handedness, bone.Id);
            var parentProxyTransform = GetProxyTransform(handedness, (OculusApi.BoneId)bone.ParentBoneIndex);

            boneProxyTransform.parent = parentProxyTransform;

            if (bone.ParentBoneIndex == (int)OculusApi.BoneId.Invalid)
            {
                var rootPose = FixRotation(handedness, new MixedRealityPose(conversionProxyRootTransform.localPosition, conversionProxyRootTransform.localRotation));
                boneProxyTransform.localPosition = rootPose.Position;
                boneProxyTransform.localRotation = rootPose.Rotation;
            }
            else
            {
                boneProxyTransform.localPosition = bone.Pose.Position.FromFlippedZVector3f();
                boneProxyTransform.localRotation = handState.BoneRotations[(int)bone.Id].FromFlippedZQuatf();
            }

            return FixRotation(handedness, new MixedRealityPose(
                conversionProxyRootTransform.InverseTransformPoint(boneProxyTransform.position),
                Quaternion.Inverse(conversionProxyRootTransform.rotation) * boneProxyTransform.rotation));
        }

        /// <summary>
        /// WARNING: THIS CODE IS SUBJECT TO CHANGE WITH THE OCULUS SDK.
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
        /// the joint-->parent-joint relation to joint-->hand-root relations proxy <see cref="Transform"/>s
        /// are used. The proxies are parented to their respective parent <see cref="Transform"/>.
        /// That way we can make use of Unity APIs to translate coordinate spaces.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the proxy <see cref="Transform"/> belongs to.</param>
        /// <param name="boneId">The Oculus bone ID to lookup the proxy <see cref="Transform"/> for.</param>
        /// <returns>The proxy <see cref="Transform"/>.</returns>
        private Transform GetProxyTransform(Handedness handedness, OculusApi.BoneId boneId)
        {
            if (conversionProxyRootTransform.IsNull())
            {
                conversionProxyRootTransform = new GameObject($"Oculus Hand Conversion Proxy").transform;
                conversionProxyRootTransform.transform.SetParent(MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform, false);
                conversionProxyRootTransform.gameObject.SetActive(false);
            }

            // Depending on the handedness we are currently working on, we need to
            // rotate the conversion root. Same dilemma as with FixRotation above.
            conversionProxyRootTransform.localRotation = Quaternion.Euler(0f, handedness == Handedness.Right ? 180f : 0f, 0f);

            if (boneId == OculusApi.BoneId.Invalid)
            {
                return conversionProxyRootTransform;
            }

            if (conversionProxyTransforms.ContainsKey(boneId))
            {
                return conversionProxyTransforms[boneId];
            }

            var transform = new GameObject($"Oculus Hand {boneId} Proxy").transform;

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(.01f, .01f, .01f);
            cube.transform.SetParent(transform, false);

            conversionProxyTransforms.Add(boneId, transform);

            return transform;
        }

        /// <summary>
        /// Gets the hand's root pose.
        /// </summary>
        /// <param name="handedness">Handedness of the hand to get the pose for.</param>
        /// <returns>The hands <see cref="HandData.RootPose"/> value.</returns>
        private MixedRealityPose GetHandRootPose(Handedness handedness)
        {
            var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
            var rootPosition = playspaceTransform.InverseTransformPoint(handState.RootPose.Position.FromFlippedZVector3f());
            var rootRotation = Quaternion.Inverse(playspaceTransform.rotation) * handState.RootPose.Orientation.FromFlippedZQuatf();

            return FixRotation(handedness, new MixedRealityPose(rootPosition + new Vector3(0f, OculusApi.EyeHeight, 0f), rootRotation));
        }

        /// <summary>
        /// Gets the hand's local pointer pose.
        /// </summary>
        /// <param name="handedness">Handedness of the hand the pose belongs to.</param>
        /// <returns>The hands <see cref="HandData.PointerPose"/> value.</returns>
        private MixedRealityPose GetPointerPose(Handedness handedness)
        {
            var playspaceTransform = MixedRealityToolkit.CameraSystem.MainCameraRig.PlayspaceTransform;
            var rootPose = GetHandRootPose(handedness);
            var platformRootPosition = handState.RootPose.Position.FromFlippedZVector3f();

            var platformPointerPosition = rootPose.Position + handState.PointerPose.Position.FromFlippedZVector3f() - platformRootPosition;
            var platformPointerRotation = Quaternion.Inverse(playspaceTransform.rotation) * handState.PointerPose.Orientation.FromFlippedZQuatf();

            return new MixedRealityPose(platformPointerPosition, platformPointerRotation);
        }
    }
}