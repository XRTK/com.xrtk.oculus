// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using XRTK.Definitions.Utilities;

namespace XRTK.Oculus.Plugins
{
    public static class OculusApi
    {
        #region Oculus API Properties

        private static Version _versionZero = new Version(0, 0, 0);
        private static readonly Version OVRP_1_38_0_version = new Version(1, 38, 0);
        private static readonly Version OVRP_1_42_0_version = new Version(1, 42, 0);
        private static readonly Version OVRP_1_44_0_version = new Version(1, 44, 0);
        private static readonly Version OVRP_1_45_0_version = new Version(1, 45, 0);
        private static readonly Version OVRP_1_46_0_version = new Version(1, 46, 0);
        private static readonly Version OVRP_1_48_0_version = new Version(1, 48, 0);
        private static readonly Version OVRP_1_49_0_version = new Version(1, 49, 0);
        private static readonly Version OVRP_1_55_0_version = new Version(1, 55, 0);
        private static readonly Version OVRP_1_55_1_version = new Version(1, 55, 1);
        private static readonly Version OVRP_1_57_0_version = new Version(1, 57, 0);
        private static readonly Version OVRP_1_63_0_version = new Version(1, 63, 0);

        private const string pluginName = "OVRPlugin";

        private static Version _version;

        private static EventDataBuffer eventDataBuffer = new EventDataBuffer();

        /// <summary>
        /// Current version of the Oculus API library in use
        /// </summary>
        public static Version Version
        {
            get
            {
                if (_version == null)
                {
                    try
                    {
                        string pluginVersion = ovrp_GetVersion();

                        if (pluginVersion != null)
                        {
                            // Truncate unsupported trailing version info for System.Version. Original string is returned if not present.
                            pluginVersion = pluginVersion.Split('-')[0];
                            _version = new Version(pluginVersion);
                            if (Debug.isDebugBuild && Application.isPlaying)
                            {
                                Debug.Log($"Oculus API version detected is - [{_version.ToString()}]");
                            }
                        }
                        else
                        {
                            _version = _versionZero;
                        }
                    }
                    catch
                    {
                        _version = _versionZero;
                    }
                }

                return _version;
            }
        }

        internal static readonly float AXIS_AS_BUTTON_THRESHOLD = 0.5f;
        internal static readonly float AXIS_DEADZONE_THRESHOLD = 0.2f;

        internal static Step stepType = Step.Render;

        private static OVRControllerBase[] controllers;

        internal static OVRControllerBase[] Controllers
        {
            get
            {
                if (controllers == null)
                {
                    controllers = new OVRControllerBase[]
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                        new OVRControllerGamepadAndroid(),
                        new OVRControllerLTouch(),
                        new OVRControllerRTouch(),
                        new OVRControllerTouch(),
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                        new OVRControllerGamepadMac(),
#else
                        new OVRControllerGamepadPC(),
                        new OVRControllerLTouch(),
                        new OVRControllerRTouch(),
                        new OVRControllerRemote(),
                        new OVRControllerTouch(),
#endif
                    };
                }
                return controllers;
            }
            set { controllers = value; }
        }

        internal static Controller activeControllerType = Controller.None;
        internal static Controller connectedControllerTypes = Controller.None;

        /// <summary>
        /// Oculus API Initialized check
        /// </summary>
        public static bool Initialized
        {
            get
            {
                try
                {
                    return ovrp_GetInitialized() == Bool.True;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion Oculus API Properties

        #region Oculus Events

        /// <summary>
        /// Occurs when an HMD is put on the user's head.
        /// </summary>
        public static event Action HMDMounted;

        /// <summary>
        /// Occurs when an HMD is taken off the user's head.
        /// </summary>
        public static event Action HMDUnmounted;

        /// <summary>
        /// Occurs when the display refresh rate changes
        /// @params (float fromRefreshRate, float toRefreshRate)
        /// </summary>
        public static event Action<float, float> DisplayRefreshRateChanged;

        #endregion Oculus Events

        #region Oculus Update

        internal static void UpdateHMDEvents()
        {
            while (PollEvent(ref eventDataBuffer))
            {
                switch (eventDataBuffer.EventType)
                {
                    case EventType.DisplayRefreshRateChanged:
                        if (DisplayRefreshRateChanged != null)
                        {
                            float FromRefreshRate = BitConverter.ToSingle(eventDataBuffer.EventData, 0);
                            float ToRefreshRate = BitConverter.ToSingle(eventDataBuffer.EventData, 4);
                            DisplayRefreshRateChanged(FromRefreshRate, ToRefreshRate);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private static bool _isUserPresent = false;
        private static bool _wasUserPresent = false;

        internal static void UpdateUserEvents()
        {
            _isUserPresent = UserPresent;

            if (_wasUserPresent && !_isUserPresent)
            {
                try
                {
                    Debug.Log("[OVRManager] HMDUnmounted event");
                    if (HMDUnmounted != null)
                    {
                        HMDUnmounted();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Caught Exception: " + e);
                }
            }

            if (!_wasUserPresent && _isUserPresent)
            {
                try
                {
                    Debug.Log("[OVRManager] HMDMounted event");
                    if (HMDMounted != null)
                    {
                        HMDMounted();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Caught Exception: " + e);
                }
            }

            _wasUserPresent = _isUserPresent;
        }

        #endregion Oculus Update

        #region Oculus Device Characteristics

        /// <summary>
        /// Reported Eye Depth from the Oculus API
        /// </summary>
        public static float EyeDepth
        {
            get
            {
                if (!Initialized)
                {
                    return 0.0f;
                }

                return ovrp_GetUserEyeDepth();
            }
            set
            {
                ovrp_SetUserEyeDepth(value);
            }
        }

        /// <summary>
        /// Reported Eye Height from the Oculus API
        /// </summary>
        public static float EyeHeight
        {
            get
            {
                return ovrp_GetUserEyeHeight();
            }
            set
            {
                ovrp_SetUserEyeHeight(value);
            }
        }

        /// <summary>
        /// Returns whether the user is wearing the headset or not from the Oculus API
        /// </summary>
        public static bool UserPresent
        {
            get
            {
                return Initialized && ovrp_GetUserPresent() == Bool.True;
            }
        }

        public static bool HeadphonesPresent
        {
            get
            {
#if OVRPLUGIN_UNSUPPORTED_PLATFORM
                return false;
#else
                return Initialized && ovrp_GetSystemHeadphonesPresent() == Bool.True;
#endif
            }
        }

        /// <summary>
        /// Returns the current battery level of the device
        /// </summary>
        public static float BatteryLevel
        {
            get
            {
                return ovrp_GetSystemBatteryLevel();
            }
        }

        /// <summary>
        /// Returns the current battery temperature
        /// </summary>
        public static float BatteryTemperature
        {
            get
            {
                return ovrp_GetSystemBatteryTemperature();
            }
        }

        /// <summary>
        /// Manages the device runtime CPU level for performance
        /// </summary>
        /// <remarks>
        /// Defaults to 2, normally set by the device
        /// </remarks>
        public static int cpuLevel
        {
            get
            {
                return ovrp_GetSystemCpuLevel();
            }
            set
            {
                ovrp_SetSystemCpuLevel(value);
            }
        }
        /// <summary>
        /// Manages the device runtime GPU level for performance
        /// </summary>
        /// <remarks>
        /// Defaults to 2, normally set by the device
        /// </remarks>
        public static int gpuLevel
        {
            get
            {
                return ovrp_GetSystemGpuLevel();
            }
            set
            {
                ovrp_SetSystemGpuLevel(value);
            }
        }

        /// <summary>
        /// Manages the device vSync Count
        /// </summary>
        public static int vsyncCount
        {
            get
            {
                return ovrp_GetSystemVSyncCount();
            }
            set
            {
                ovrp_SetSystemVSyncCount(value);
            }
        }

        /// <summary>
        /// Returns the current system volume
        /// </summary>
        public static float systemVolume
        {
            get
            {

                return ovrp_GetSystemVolume();
            }
        }

        /// <summary>
        /// Manages the Software IPD level for the device
        /// </summary>
        public static float ipd
        {
            get
            {
                return ovrp_GetUserIPD();
            }
            set
            {
                ovrp_SetUserIPD(value);
            }
        }

        /// <summary>
        /// Manages the software occlusion mesh for the eyes
        /// </summary>
        public static bool occlusionMesh
        {
            get
            {
                return Initialized && (ovrp_GetEyeOcclusionMeshEnabled() == Bool.True);
            }
            set
            {
                if (!Initialized)
                {
                    return;
                }

                ovrp_SetEyeOcclusionMeshEnabled(ToBool(value));
            }
        }

        /// <summary>
        /// Returns the current battery status for the device
        /// </summary>
        public static BatteryStatus batteryStatus
        {
            get
            {
                return ovrp_GetSystemBatteryStatus();
            }
        }


        #endregion Oculus Device Characteristics

        #region Oculus API import

        private static Bool ToBool(bool b)
        {
            return (b) ? Bool.True : Bool.False;
        }

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetInitialized();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetVersion")]
        private static extern IntPtr _ovrp_GetVersion();
        private static string ovrp_GetVersion() { return Marshal.PtrToStringAnsi(_ovrp_GetVersion()); }

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern TrackingOrigin ovrp_GetTrackingOriginType();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetTrackingOriginType(TrackingOrigin originType);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Posef ovrp_GetTrackingCalibratedOrigin();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrpi_SetTrackingCalibratedOrigin();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetUserPresent();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float ovrp_GetUserEyeDepth();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetUserEyeDepth(float value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float ovrp_GetUserEyeHeight();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetEyeOcclusionMeshEnabled();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetEyeOcclusionMeshEnabled(Bool value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetSystemHeadphonesPresent();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float ovrp_GetUserIPD();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetUserIPD(float value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ovrp_GetSystemCpuLevel();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetSystemCpuLevel(int value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ovrp_GetSystemGpuLevel();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetSystemGpuLevel(int value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetSystemPowerSavingMode();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float ovrp_GetSystemDisplayFrequency();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int ovrp_GetSystemVSyncCount();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float ovrp_GetSystemVolume();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetSystemVSyncCount(int vsyncCount);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern BatteryStatus ovrp_GetSystemBatteryStatus();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float ovrp_GetSystemBatteryLevel();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern float ovrp_GetSystemBatteryTemperature();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetExternalCameraIntrinsics(int cameraId, out CameraIntrinsics cameraIntrinsics);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetExternalCameraExtrinsics(int cameraId, out CameraExtrinsics cameraExtrinsics);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetUserEyeHeight(float value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetControllerState4(uint controllerMask, ref ControllerState4 controllerState);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern PoseStatef ovrp_GetNodePoseState(Step stepId, Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetDominantHand(out Handedness dominantHand);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetNodePresent(Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetNodeOrientationTracked(Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetNodePositionTracked(Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetNodeOrientationValid(Node nodeId, ref Bool nodeOrientationValid);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetNodePositionValid(Node nodeId, ref Bool nodePositionValid);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetBoundaryConfigured();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern BoundaryTestResult ovrp_TestBoundaryNode(Node nodeId, BoundaryType boundaryType);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern BoundaryTestResult ovrp_TestBoundaryPoint(Vector3f point, BoundaryType boundaryType);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Vector3f ovrp_GetBoundaryDimensions(BoundaryType boundaryType);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_GetBoundaryVisible();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetBoundaryVisible(bool value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern SystemHeadset ovrp_GetSystemHeadsetType();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Controller ovrp_GetActiveController();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Controller ovrp_GetConnectedControllers();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_Update2(int stateId, int frameIndex, double predictionSeconds);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern HapticsDesc ovrp_GetControllerHapticsDesc(uint controllerMask);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern HapticsState ovrp_GetControllerHapticsState(uint controllerMask);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_RecenterTrackingOrigin(uint flags);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Bool ovrp_SetControllerVibration(uint controllerMask, float frequency, float amplitude);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetAdaptiveGpuPerformanceScale2(ref float adaptiveGpuPerformanceScale);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetExternalCameraCalibrationRawPose(int cameraId, out Posef rawPose);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetHandTrackingEnabled(ref Bool handTrackingEnabled);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetHandState(Step stepId, Hand hand, out HandStateInternal handState);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetSkeleton(SkeletonType skeletonType, out Skeleton skeleton);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetMesh(MeshType meshType, out Mesh mesh);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_OverrideExternalCameraFov(int cameraId, Bool useOverriddenFov, ref Fovf fov);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetUseOverriddenExternalCameraFov(int cameraId, out Bool useOverriddenFov);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_OverrideExternalCameraStaticPose(int cameraId, Bool useOverriddenPose, ref Posef pose);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetUseOverriddenExternalCameraStaticPose(int cameraId, out Bool useOverriddenStaticPose);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_ResetDefaultExternalCamera();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern Result ovrp_SetDefaultExternalCamera(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetSystemHmd3DofModeEnabled(ref Bool enabled);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetTiledMultiResSupported(out Bool foveationSupported);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetTiledMultiResLevel(out FixedFoveatedRenderingLevel level);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_SetTiledMultiResLevel(FixedFoveatedRenderingLevel level);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetTiledMultiResDynamic(out Bool isDynamic);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_SetTiledMultiResDynamic(Bool isDynamic);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_SetExternalCameraProperties(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_SetClientColorDesc(ColorSpace colorSpace);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetHmdColorDesc(ref ColorSpace colorSpace);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_Media_SetHeadsetControllerPose(Posef headsetPose, Posef leftControllerPose, Posef rightControllerPose);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetSkeleton2(SkeletonType skeletonType, out Skeleton2Internal skeleton);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_PollEvent(ref EventDataBuffer eventDataBuffer);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetNativeXrApiType(out XrApi xrApi);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_GetNativeOpenXRHandles(out UInt64 xrInstance, out UInt64 xrSession);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Result ovrp_PollEvent2(ref EventType eventType, ref IntPtr eventData);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_SetEyeFovPremultipliedAlphaMode(Bool enabled);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_GetEyeFovPremultipliedAlphaMode(ref Bool enabled);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_SetKeyboardOverlayUV(Vector2f uv);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_InitializeInsightPassthrough();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_ShutdownInsightPassthrough();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetInsightPassthroughInitialized();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_SetInsightPassthroughStyle(int layerId, InsightPassthroughStyle style);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_CreateInsightTriangleMesh(
            int layerId, IntPtr vertices, int vertexCount, IntPtr triangles, int triangleCount, out ulong meshHandle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_DestroyInsightTriangleMesh(ulong meshHandle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_AddInsightPassthroughSurfaceGeometry(int layerId, ulong meshHandle, Matrix4x4 T_world_model, out ulong geometryInstanceHandle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_DestroyInsightPassthroughGeometryInstance(ulong geometryInstanceHandle);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_UpdateInsightPassthroughGeometryTransform(ulong geometryInstanceHandle, Matrix4x4 T_world_model);

        #endregion Oculus API import

        #region Oculus Data Types

        /// <summary>
        /// Oculus API result states
        /// </summary>
        public enum Result
        {
            /// Success
            Success = 0,

            /// Failure
            Failure = -1000,
            Failure_InvalidParameter = -1001,
            Failure_NotInitialized = -1002,
            Failure_InvalidOperation = -1003,
            Failure_Unsupported = -1004,
            Failure_NotYetImplemented = -1005,
            Failure_OperationFailed = -1006,
            Failure_InsufficientSize = -1007,
            Failure_DataIsInvalid = -1008,
            Failure_DeprecatedOperation = -1009
        }

        /// <summary>
        /// Oculus native Bool type, overridden from base .NET bool
        /// </summary>
        public enum Bool
        {
            False = 0,
            True
        }

        /// <summary>
        /// Oculus Camera Status type
        /// </summary>
        public enum CameraStatus
        {
            CameraStatus_None,
            CameraStatus_Connected,
            CameraStatus_Calibrating,
            CameraStatus_CalibrationFailed,
            CameraStatus_Calibrated,
            CameraStatus_ThirdPerson,
            CameraStatus_EnumSize = 0x7fffffff
        }

        public enum CameraAnchorType
        {
            CameraAnchorType_PreDefined = 0,
            CameraAnchorType_Custom = 1,
            CameraAnchorType_Count,
            CameraAnchorType_EnumSize = 0x7fffffff
        }

        public enum XrApi
        {
            Unknown = 0,
            CAPI = 1,
            VRAPI = 2,
            OpenXR = 3,
            EnumSize = 0x7fffffff
        }

        /// <summary>
        /// Oculus API native Size (integer)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Sizei
        {
            public int w;
            public int h;

            public static readonly Sizei zero = new Sizei { w = 0, h = 0 };
        }

        /// <summary>
        /// Oculus API native Size (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Sizef
        {
            public float w;
            public float h;

            public static readonly Sizef zero = new Sizef { w = 0, h = 0 };
        }

        /// <summary>
        /// Oculus API native Vector2 (integer)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Vector2i
        {
            public int x;
            public int y;
        }

        /// <summary>
        /// Oculus API native Rect (integer)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Recti
        {
            public Vector2i Pos;
            public Sizei Size;
        }

        /// <summary>
        /// Oculus API native Rect (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Rectf
        {
            public Vector2f Pos;
            public Sizef Size;
        }

        /// <summary>
        /// Oculus API native Frustrum (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Frustumf
        {
            public float zNear;
            public float zFar;
            public float fovX;
            public float fovY;
        }

        /// <summary>
        /// Oculus API native Frustrum (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Frustumf2
        {
            public float zNear;
            public float zFar;
            public Fovf Fov;
        }

        /// <summary>
        /// Oculus API native Vector2
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Vector2f
        {
            public float x;
            public float y;

            public static implicit operator Vector2f(Vector2 v) => new Vector2f { x = v.x, y = v.y };

            public static implicit operator Vector2(Vector2f v) => new Vector2(v.x, v.y);
        }

        /// <summary>
        /// Oculus API native Vector3
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Vector3f
        {
            public float x;
            public float y;
            public float z;
            public static readonly Vector3f zero = new Vector3f { x = 0.0f, y = 0.0f, z = 0.0f };
            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}, {2}", x, y, z);
            }

            public static implicit operator Vector3f(Vector3 v) => new Vector3f { x = v.x, y = v.y, z = -v.z };

            public static implicit operator Vector3(Vector3f v) => new Vector3(v.x, v.y, -v.z);

            /// <summary>
            /// Returns a <see cref="Vector3"/> from an <see cref="OculusApi.Vector3f"/>
            /// </summary>
            /// <returns>New <see cref="Vector3"/> with a flipped Z axis.</returns>
            public Vector3 ToVector3FlippedZ()
            {
                return new Vector3(x, y, z);
            }
        }

        /// <summary>
        /// Oculus API native Vector4 (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Vector4f
        {
            public float x;
            public float y;
            public float z;
            public float w;
            public static readonly Vector4f zero = new Vector4f { x = 0.0f, y = 0.0f, z = 0.0f, w = 0.0f };
            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", x, y, z, w);
            }

            public static implicit operator Vector4f(Vector4 v) => new Vector4f { x = v.x, y = v.y, z = v.z, w = v.w };

            public static implicit operator Vector4(Vector4f v) => new Vector4(v.x, v.y, v.z, v.w);
        }

        /// <summary>
        /// Oculus API native Vector4 (string)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Vector4s
        {
            public short x;
            public short y;
            public short z;
            public short w;
            public static readonly Vector4s zero = new Vector4s { x = 0, y = 0, z = 0, w = 0 };
            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", x, y, z, w);
            }
        }

        /// <summary>
        /// Oculus API native Quaternion (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Quatf
        {
            public float x;
            public float y;
            public float z;
            public float w;
            public static readonly Quatf identity = new Quatf { x = 0.0f, y = 0.0f, z = 0.0f, w = 1.0f };
            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}, {1}, {2}, {3}", x, y, z, w);
            }

            public static implicit operator Quatf(Quaternion q) => new Quatf { x = q.x, y = q.y, z = q.z, w = q.w };

            public static implicit operator Quaternion(Quatf v) => new Quaternion(v.x, v.y, v.z, v.w);

            /// <summary>
            /// Gets a <see cref="Quaternion"/> from a flipped X and Y axis <see cref="OculusApi.Quatf"/>.
            /// </summary>
            /// <returns>Unity orientation.</returns>
            public Quaternion ToQuaternionFlippedXY()
            {
                return new Quaternion(-x, -y, z, w);
            }
        }

        /// <summary>
        /// Oculus API native Pose (Position + Rotation) (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Posef
        {
            public Quatf Orientation;
            public Vector3f Position;
            public static readonly Posef identity = new Posef { Orientation = Quatf.identity, Position = Vector3f.zero };

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "Position ({0}), Orientation({1})", Position, Orientation);
            }

            public static implicit operator Posef(MixedRealityPose p) =>
                new Posef { Position = p.Position, Orientation = p.Rotation };

            public static implicit operator MixedRealityPose(Posef p) =>
                new MixedRealityPose(p.Position, p.Orientation);


            /// <summary>
            /// Extension method to convert a <see cref="OculusApi.Posef"/> to a <see cref="MixedRealityPose"/>
            /// </summary>
            /// <param name="adjustForEyeHeight"></param>
            /// <returns>Returns an XRTK MixedRealityPose</returns>
            public MixedRealityPose ToMixedRealityPoseFlippedQuaternionXY(bool adjustForEyeHeight = false)
            {
                return new MixedRealityPose
                (
                    position: new Vector3(Position.x,
                        adjustForEyeHeight ? Position.y + EyeHeight : Position.y,
                        -Position.z),
                    rotation: Orientation.ToQuaternionFlippedXY()
                );
            }
        }

        /// <summary>
        /// Oculus native pose extension, for velocity definitions
        /// </summary>
        /// <remarks>For future use</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct PoseStatef
        {
            public Posef Pose;
            public Vector3f Velocity;
            public Vector3f Acceleration;
            public Vector3f AngularVelocity;
            public Vector3f AngularAcceleration;
            public double Time;

            public static readonly PoseStatef identity = new PoseStatef
            {
                Pose = Posef.identity,
                Velocity = Vector3f.zero,
                Acceleration = Vector3f.zero,
                AngularVelocity = Vector3f.zero,
                AngularAcceleration = Vector3f.zero
            };
        }

        /// <summary>
        /// Oculus native node definition, detailing tracking objects
        /// </summary>
        public enum Node
        {
            None = -1,
            EyeLeft = 0,
            EyeRight = 1,
            EyeCenter = 2,
            HandLeft = 3,
            HandRight = 4,
            TrackerZero = 5,
            TrackerOne = 6,
            TrackerTwo = 7,
            TrackerThree = 8,
            Head = 9,
            DeviceObjectZero = 10,
            Count
        }

        /// <summary>
        /// Oculus native controller data, combines all input in to a single state
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ControllerState4
        {
            public uint ConnectedControllers;
            public uint Buttons;
            public uint Touches;
            public uint NearTouches;
            public float LIndexTrigger;
            public float RIndexTrigger;
            public float LHandTrigger;
            public float RHandTrigger;
            public Vector2f LThumbstick;
            public Vector2f RThumbstick;
            public Vector2f LTouchpad;
            public Vector2f RTouchpad;
            public byte LBatteryPercentRemaining;
            public byte RBatteryPercentRemaining;
            public byte LRecenterCount;
            public byte RRecenterCount;
            public byte Reserved_27;
            public byte Reserved_26;
            public byte Reserved_25;
            public byte Reserved_24;
            public byte Reserved_23;
            public byte Reserved_22;
            public byte Reserved_21;
            public byte Reserved_20;
            public byte Reserved_19;
            public byte Reserved_18;
            public byte Reserved_17;
            public byte Reserved_16;
            public byte Reserved_15;
            public byte Reserved_14;
            public byte Reserved_13;
            public byte Reserved_12;
            public byte Reserved_11;
            public byte Reserved_10;
            public byte Reserved_09;
            public byte Reserved_08;
            public byte Reserved_07;
            public byte Reserved_06;
            public byte Reserved_05;
            public byte Reserved_04;
            public byte Reserved_03;
            public byte Reserved_02;
            public byte Reserved_01;
            public byte Reserved_00;
        }

        /// <summary>
        /// Oculus native haptics buffer management
        /// </summary>
        /// <remarks>For future use</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct HapticsBuffer
        {
            public IntPtr Samples;
            public int SamplesCount;
        }

        /// <summary>
        /// Oculus native haptics state management
        /// </summary>
        /// <remarks>For future use</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct HapticsState
        {
            public int SamplesAvailable;
            public int SamplesQueued;
        }

        /// <summary>
        /// Oculus native haptics descriptive data
        /// </summary>
        /// <remarks>For future use</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct HapticsDesc
        {
            public int SampleRateHz;
            public int SampleSizeInBytes;
            public int MinimumSafeSamplesQueued;
            public int MinimumBufferSamplesCount;
            public int OptimalBufferSamplesCount;
            public int MaximumBufferSamplesCount;
        }

        /// <summary>
        /// Oculus native button definitions
        /// </summary>
        /// <remarks>Oculus API only uses RAW definitions.  Oculus Asset also uses Virtual mappings, but it's not clear why
        /// (Oculus) Raw button mappings that can be used to directly query the state of a controller.</remarks>
        [Flags]
        public enum RawButton
        {
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            None = 0,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch: A], [LTrackedRemote: LIndexTrigger], [RTrackedRemote: RIndexTrigger], [LTouch, Touchpad, Remote: None]
            /// </summary>
            A = 0x00000001,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch: B], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            B = 0x00000002,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch: X], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            X = 0x00000100,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch: Y], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            Y = 0x00000200,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Start], [RTouch: None]
            /// </summary>
            Start = 0x00100000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Back], [Touch, LTouch, RTouch: None]
            /// </summary>
            Back = 0x00200000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad: LShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LShoulder = 0x00000800,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote: LIndexTrigger], [RTouch, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LIndexTrigger = 0x10000000,
            /// <summary>
            /// Maps to Physical Button: [Touch, LTouch: LHandTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LHandTrigger = 0x20000000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstick], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbstick = 0x00000400,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickUp], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbstickUp = 0x00000010,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickDown], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbstickDown = 0x00000020,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickLeft], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbstickLeft = 0x00000040,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickRight], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbstickRight = 0x00000080,
            /// <summary>
            /// Maps to Physical Button: [LTrackedRemote: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LTouchpad = 0x40000000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad: RShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RShoulder = 0x00000008,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch, RTrackedRemote: RIndexTrigger], [LTouch, LTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RIndexTrigger = 0x04000000,
            /// <summary>
            /// Maps to Physical Button: [Touch, RTouch: RHandTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RHandTrigger = 0x08000000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstick], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbstick = 0x00000004,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickUp], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbstickUp = 0x00001000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickDown], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbstickDown = 0x00002000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickLeft], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbstickLeft = 0x00004000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickRight], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbstickRight = 0x00008000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RTouchpad = unchecked((int)0x80000000),
            /// <summary>
            /// Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadUp], [Touch, LTouch, RTouch: None]
            /// </summary>
            DpadUp = 0x00010000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadDown], [Touch, LTouch, RTouch: None]
            /// </summary>
            DpadDown = 0x00020000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadLeft], [Touch, LTouch, RTouch: None]
            /// </summary>
            DpadLeft = 0x00040000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadRight], [Touch, LTouch, RTouch: None]
            /// </summary>
            DpadRight = 0x00080000,
            /// <summary>
            /// Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Any]
            /// </summary>
            Any = ~None
        }

        /// <summary>
        /// Oculus native touch definitions
        /// </summary>
        /// <remarks>Oculus API only uses RAW definitions.  Oculus Asset also uses Virtual mappings, but it's not clear why
        /// (Oculus)  Raw capacitive touch mappings that can be used to directly query the state of a controller.</remarks>
        [Flags]
        public enum RawTouch
        {
            /// <summary>
            /// Maps to Physical Touch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            None = 0,
            /// <summary>
            /// Maps to Physical Touch: [Touch, RTouch: A], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            A = RawButton.A,
            /// <summary>
            /// Maps to Physical Touch: [Touch, RTouch: B], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            B = RawButton.B,
            /// <summary>
            /// Maps to Physical Touch: [Touch, LTouch: X], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            X = RawButton.X,
            /// <summary>
            /// Maps to Physical Touch: [Touch, LTouch: Y], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            Y = RawButton.Y,
            /// <summary>
            /// Maps to Physical Touch: [Touch, LTouch: LIndexTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LIndexTrigger = 0x00001000,
            /// <summary>
            /// Maps to Physical Touch: [Touch, LTouch: LThumbstick], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbstick = RawButton.LThumbstick,
            /// <summary>
            /// Maps to Physical Touch: [Touch, LTouch: LThumbRest], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbRest = 0x00000800,
            /// <summary>
            /// Maps to Physical Touch: [LTrackedRemote, Touchpad: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Remote: None]
            /// </summary>
            LTouchpad = RawButton.LTouchpad,
            /// <summary>
            /// Maps to Physical Touch: [Touch, RTouch: RIndexTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RIndexTrigger = 0x00000010,
            /// <summary>
            /// Maps to Physical Touch: [Touch, RTouch: RThumbstick], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbstick = RawButton.RThumbstick,
            /// <summary>
            /// Maps to Physical Touch: [Touch, RTouch: RThumbRest], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbRest = 0x00000008,
            /// <summary>
            /// Maps to Physical Touch: [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RTouchpad = RawButton.RTouchpad,
            /// <summary>
            /// Maps to Physical Touch: [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad: Any], [Gamepad, Remote: None]
            /// </summary>
            Any = ~None
        }

        /// <summary>
        /// Oculus native near touch definitions
        /// </summary>
        /// <remarks>Oculus API only uses RAW definitions.  Oculus Asset also uses Virtual mappings, but it's not clear why
        /// (Oculus) Raw near touch mappings that can be used to directly query the state of a controller.</remarks>
        [Flags]
        public enum RawNearTouch
        {
            /// <summary>
            /// Maps to Physical NearTouch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            None = 0,
            /// <summary>
            /// Maps to Physical NearTouch: [Touch, LTouch: Implies finger is in close proximity to LIndexTrigger.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LIndexTrigger = 0x00000001,
            /// <summary>
            /// Maps to Physical NearTouch: [Touch, LTouch: Implies thumb is in close proximity to LThumbstick OR X/Y buttons.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbButtons = 0x00000002,
            /// <summary>
            /// Maps to Physical NearTouch: [Touch, RTouch: Implies finger is in close proximity to RIndexTrigger.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RIndexTrigger = 0x00000004,
            /// <summary>
            /// Maps to Physical NearTouch: [Touch, RTouch: Implies thumb is in close proximity to RThumbstick OR A/B buttons.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbButtons = 0x00000008,
            /// <summary>
            /// Maps to Physical NearTouch: [Touch, LTouch, RTouch: Any], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            Any = ~None
        }

        /// <summary>
        /// Oculus native single axis definitions
        /// </summary>
        /// <remarks>Oculus API only uses RAW definitions.  Oculus Asset also uses Virtual mappings, but it's not clear why
        /// (Oculus) Raw 1-dimensional axis (float) mappings that can be used to directly query the state of a controller.</remarks>
        [Flags]
        public enum RawAxis1D
        {
            /// <summary>
            /// Maps to Physical Axis1D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            None = 0,
            /// <summary>
            /// Maps to Physical Axis1D: [Gamepad, Touch, LTouch: LIndexTrigger], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LIndexTrigger = 0x01,
            /// <summary>
            /// Maps to Physical Axis1D: [Touch, LTouch: LHandTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LHandTrigger = 0x04,
            /// <summary>
            /// Maps to Physical Axis1D: [Gamepad, Touch, RTouch: RIndexTrigger], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RIndexTrigger = 0x02,
            /// <summary>
            /// Maps to Physical Axis1D: [Touch, RTouch: RHandTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RHandTrigger = 0x08,
            /// <summary>
            /// Maps to Physical Axis1D: [Gamepad, Touch, LTouch, RTouch: Any], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            Any = ~None
        }

        /// <summary>
        /// Oculus native dual axis definitions
        /// </summary>
        /// <remarks>Oculus API only uses RAW definitions.  Oculus Asset also uses Virtual mappings, but it's not clear why
        /// (Oculus) Raw 2-dimensional axis (Vector2) mappings that can be used to directly query the state of a controller.</remarks>
        [Flags]
        public enum RawAxis2D
        {
            /// <summary>
            /// Maps to Physical Axis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            None = 0,
            /// <summary>
            /// Maps to Physical Axis2D: [Gamepad, Touch, LTouch: LThumbstick], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            LThumbstick = 0x01,
            /// <summary>
            /// Maps to Physical Axis2D: [LTrackedRemote, Touchpad: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Remote: None]
            /// </summary>
            LTouchpad = 0x04,
            /// <summary>
            /// Maps to Physical Axis2D: [Gamepad, Touch, RTouch: RThumbstick], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RThumbstick = 0x02,
            /// <summary>
            /// Maps to Physical Axis2D: [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, Touchpad, Remote: None]
            /// </summary>
            RTouchpad = 0x08,
            /// <summary>
            /// Maps to Physical Axis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote: Any], [Touchpad, Remote: None]
            /// </summary>
            Any = ~None
        }

        /// <summary>
        /// Oculus native controller type definitions
        /// </summary>
        /// <remarks>(Oculus) Identifies a controller which can be used to query the virtual or raw input state.</remarks>
        [Flags]
        public enum Controller
        {
            None = 0,
            LTouch = 0x00000001,
            RTouch = 0x00000002,
            Touch = LTouch | RTouch,
            Remote = 0x00000004,
            Gamepad = 0x00000010,
            LHand = 0x00000020,
            RHand = 0x00000040,
            Hands = LHand | RHand,
            Active = unchecked((int)0x80000000),
            All = ~None,
        }

        /// <summary>
        /// Oculus native controller Handedness definitions
        /// </summary>
        public enum Handedness
        {
            Unsupported = 0,
            LeftHanded = 1,
            RightHanded = 2
        }

        /// <summary>
        /// Oculus native controller Render/Physics step definitions.  Used by the API when calculating feedback.
        /// </summary>
        public enum Step
        {
            Render = -1,
            Physics = 0
        }

        /// <summary>
        /// Oculus native controller Tracking definitions
        /// </summary>
        /// <remarks>For future use</remarks>
        public enum TrackingOrigin
        {
            EyeLevel = 0,
            FloorLevel = 1,
            Stage = 2,
            Count
        }

        /// <summary>
        /// Oculus native controller re-centering parameters
        /// </summary>
        /// <remarks>For future use</remarks>
        public enum RecenterFlags
        {
            Default = 0,
            IgnoreAll = unchecked((int)0x80000000),
            Count,
        }

        /// <summary>
        /// Oculus native battery status
        /// </summary>
        /// <remarks>For future use</remarks>
        public enum BatteryStatus
        {
            Charging = 0,
            Discharging,
            Full,
            NotCharging,
            Unknown
        }

        /// <summary>
        /// Oculus native boundary type setting
        /// </summary>
        /// <remarks>For future use</remarks>
        public enum BoundaryType
        {
            OuterBoundary = 0x0001,
            PlayArea = 0x0100
        }

        /// <summary>
        /// Oculus native boundary collision test results
        /// </summary>
        /// <remarks>For future use</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct BoundaryTestResult
        {
            public Bool IsTriggering;
            public float ClosestDistance;
            public Vector3f ClosestPoint;
            public Vector3f ClosestPointNormal;
        }

        /// <summary>
        /// Oculus native boundary geometry data
        /// </summary>
        /// <remarks>For future use</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct BoundaryGeometry
        {
            public BoundaryType BoundaryType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public Vector3f[] Points;
            public int PointsCount;
        }

        /// <summary>
        /// Oculus API native Color (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Colorf
        {
            public float r;
            public float g;
            public float b;
            public float a;
        }

        /// <summary>
        /// Oculus API native Fov (float)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Fovf
        {
            public float UpTan;
            public float DownTan;
            public float LeftTan;
            public float RightTan;
        }

        /// <summary>
        /// CameraIntrinsics definition for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CameraIntrinsics
        {
            public Bool IsValid;
            public double LastChangedTimeSeconds;
            public Fovf FOVPort;
            public float VirtualNearPlaneDistanceMeters;
            public float VirtualFarPlaneDistanceMeters;
            public Sizei ImageSensorPixelResolution;
        }

        /// <summary>
        /// CameraExtrinsics definition for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CameraExtrinsics
        {
            public Bool IsValid;
            public double LastChangedTimeSeconds;
            public CameraStatus CameraStatusData;
            public Node AttachedToNode;
            public Posef RelativePose;
        }

        /// <summary>
        /// Type of headset detected by the Oculus API
        /// </summary>
        public enum SystemHeadset
        {
            None = 0,

            // Standalone headsets
            Oculus_Quest = 8,
            Oculus_Quest_2 = 9,
            Placeholder_10,
            Placeholder_11,
            Placeholder_12,
            Placeholder_13,
            Placeholder_14,

            // PC headsets
            Rift_DK1 = 0x1000,
            Rift_DK2,
            Rift_CV1,
            Rift_CB,
            Rift_S,
            Oculus_Link_Quest,
            PC_Placeholder_4102,
            PC_Placeholder_4103,
            PC_Placeholder_4104,
            PC_Placeholder_4105,
            PC_Placeholder_4106,
            PC_Placeholder_4107
        }

        public enum FixedFoveatedRenderingLevel
        {
            Off = 0,
            Low = 1,
            Medium = 2,
            High = 3,
            // High foveation setting with more detail toward the bottom of the view and more foveation near the top (Same as High on Oculus Go)
            HighTop = 4,
            EnumSize = 0x7FFFFFFF
        }

        public enum ColorSpace
        {
            /// The default value from GetHmdColorSpace until SetClientColorDesc is called. Only valid on PC, and will be remapped to Quest on Mobile
            Unknown = 0,
            /// No color correction, not recommended for production use. See documentation for more info
            Unmanaged = 1,
            /// Preferred color space for standardized color across all Oculus HMDs with D65 white point
            Rec_2020 = 2,
            /// Rec. 709 is used on Oculus Go and shares the same primary color coordinates as sRGB
            Rec_709 = 3,
            /// Oculus Rift CV1 uses a unique color space, see documentation for more info
            Rift_CV1 = 4,
            /// Oculus Rift S uses a unique color space, see documentation for more info
            Rift_S = 5,
            /// Oculus Quest's native color space is slightly different than Rift CV1
            Quest = 6,
            /// Similar to DCI-P3. See documentation for more details on P3
            P3 = 7,
            /// Similar to sRGB but with deeper greens using D65 white point
            Adobe_RGB = 8,
        }

        public enum EventType
        {
            Unknown = 0,
            DisplayRefreshRateChanged = 1,
        }

        private const int EventDataBufferSize = 4000;

        [StructLayout(LayoutKind.Sequential)]
        public struct EventDataBuffer
        {
            public EventType EventType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = EventDataBufferSize)]
            public byte[] EventData;
        }

        /// <summary>
        /// Encapsulates an 8-byte-aligned of unmanaged memory.
        /// </summary>
        public class OVRNativeBuffer : IDisposable
        {
            private bool disposed = false;
            private int m_numBytes = 0;
            private IntPtr m_ptr = IntPtr.Zero;

            /// <summary>
            /// Creates a buffer of the specified size.
            /// </summary>
            public OVRNativeBuffer(int numBytes)
            {
                Reallocate(numBytes);
            }

            /// <summary>
            /// Releases unmanaged resources and performs other cleanup operations before the <see cref="OVRNativeBuffer"/> is
            /// reclaimed by garbage collection.
            /// </summary>
            ~OVRNativeBuffer()
            {
                Dispose(false);
            }

            /// <summary>
            /// Reallocates the buffer with the specified new size.
            /// </summary>
            public void Reset(int numBytes)
            {
                Reallocate(numBytes);
            }

            /// <summary>
            /// The current number of bytes in the buffer.
            /// </summary>
            public int GetCapacity()
            {
                return m_numBytes;
            }

            /// <summary>
            /// A pointer to the unmanaged memory in the buffer, starting at the given offset in bytes.
            /// </summary>
            public IntPtr GetPointer(int byteOffset = 0)
            {
                return byteOffset < 0 || byteOffset >= m_numBytes
                    ? IntPtr.Zero
                    : (byteOffset == 0) ? m_ptr : new IntPtr(m_ptr.ToInt64() + byteOffset);
            }

            /// <summary>
            /// Releases all resource used by the <see cref="OVRNativeBuffer"/> object.
            /// </summary>
            /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="OVRNativeBuffer"/>. The <see cref="Dispose"/>
            /// method leaves the <see cref="OVRNativeBuffer"/> in an unusable state. After calling <see cref="Dispose"/>, you must
            /// release all references to the <see cref="OVRNativeBuffer"/> so the garbage collector can reclaim the memory that
            /// the <see cref="OVRNativeBuffer"/> was occupying.</remarks>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposed)
                {
                    return;
                }

                if (disposing)
                {
                    // dispose managed resources
                }

                // dispose unmanaged resources
                Release();

                disposed = true;
            }

            private void Reallocate(int numBytes)
            {
                Release();

                if (numBytes > 0)
                {
                    m_ptr = Marshal.AllocHGlobal(numBytes);
                    m_numBytes = numBytes;
                }
                else
                {
                    m_ptr = IntPtr.Zero;
                    m_numBytes = 0;
                }
            }

            private void Release()
            {
                if (m_ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(m_ptr);
                    m_ptr = IntPtr.Zero;
                    m_numBytes = 0;
                }
            }
        }

        public enum InsightPassthroughColorMapType
        {
            None = 0,
            MonoToRgba = 1,
            MonoToMono = 2,
        }

        public enum InsightPassthroughStyleFlags
        {
            HasTextureOpacityFactor = 1 << 0,
            HasEdgeColor = 1 << 1,
            HasTextureColorMap = 1 << 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct InsightPassthroughStyle
        {
            public InsightPassthroughStyleFlags Flags;
            public float TextureOpacityFactor;
            public Colorf EdgeColor;
            public InsightPassthroughColorMapType TextureColorMapType;
            public uint TextureColorMapDataSize;
            public IntPtr TextureColorMapData;
        }

        #region Hands Implementation

        /// <summary>
        /// Oculus API definition for tracking confidence for Hands / Controllers
        /// </summary>
        public enum TrackingConfidence
        {
            Low = 0,
            High = 0x3f800000,
        }

        /// <summary>
        /// Hand definition for the Oculus API
        /// </summary>
        public enum Hand
        {
            None = -1,
            HandLeft = 0,
            HandRight = 1,
        }

        /// <summary>
        /// Hand Status definition for the Oculus API
        /// </summary>
        [Flags]
        public enum HandStatus
        {
            HandTracked = (1 << 0), // if this is set the hand pose and bone rotations data is usable
            InputStateValid = (1 << 1), // if this is set the pointer pose and pinch data is usable
            SystemGestureInProgress = (1 << 6), // if this is set the hand is currently processing a system gesture
            DominantHand = (1 << 7), // if this is set the hand is currently the dominant hand
            MenuPressed = (1 << 8) // if this is set the hand performed a menu press
        }

        /// <summary>
        /// Bone definition for the Oculus API
        /// </summary>
        public enum BoneId
        {
            Invalid = -1,

            Hand_Start = 0,
            Hand_WristRoot = Hand_Start + 0, // root frame of the hand, where the wrist is located
            Hand_ForearmStub = Hand_Start + 1, // frame for user's forearm
            Hand_Thumb0 = Hand_Start + 2, // thumb trapezium bone
            Hand_Thumb1 = Hand_Start + 3, // thumb metacarpal bone
            Hand_Thumb2 = Hand_Start + 4, // thumb proximal phalange bone
            Hand_Thumb3 = Hand_Start + 5, // thumb distal phalange bone
            Hand_Index1 = Hand_Start + 6, // index proximal phalange bone
            Hand_Index2 = Hand_Start + 7, // index intermediate phalange bone
            Hand_Index3 = Hand_Start + 8, // index distal phalange bone
            Hand_Middle1 = Hand_Start + 9, // middle proximal phalange bone
            Hand_Middle2 = Hand_Start + 10, // middle intermediate phalange bone
            Hand_Middle3 = Hand_Start + 11, // middle distal phalange bone
            Hand_Ring1 = Hand_Start + 12, // ring proximal phalange bone
            Hand_Ring2 = Hand_Start + 13, // ring intermediate phalange bone
            Hand_Ring3 = Hand_Start + 14, // ring distal phalange bone
            Hand_Pinky0 = Hand_Start + 15, // pinky metacarpal bone
            Hand_Pinky1 = Hand_Start + 16, // pinky proximal phalange bone
            Hand_Pinky2 = Hand_Start + 17, // pinky intermediate phalange bone
            Hand_Pinky3 = Hand_Start + 18, // pinky distal phalange bone
            Hand_MaxSkinnable = Hand_Start + 19,
            // Bone tips are position only. They are not used for skinning but are useful for hit-testing.
            // NOTE: Hand_ThumbTip == Hand_MaxSkinnable since the extended tips need to be contiguous
            Hand_ThumbTip = Hand_Start + Hand_MaxSkinnable + 0, // tip of the thumb
            Hand_IndexTip = Hand_Start + Hand_MaxSkinnable + 1, // tip of the index finger
            Hand_MiddleTip = Hand_Start + Hand_MaxSkinnable + 2, // tip of the middle finger
            Hand_RingTip = Hand_Start + Hand_MaxSkinnable + 3, // tip of the ring finger
            Hand_PinkyTip = Hand_Start + Hand_MaxSkinnable + 4, // tip of the pinky
            Hand_End = Hand_Start + Hand_MaxSkinnable + 5,

            // add new bones here

            Max = Hand_End + 0,
        }

        /// <summary>
        /// Finger definition for the Oculus API
        /// </summary>
        public enum HandFinger
        {
            Thumb = 0,
            Index = 1,
            Middle = 2,
            Ring = 3,
            Pinky = 4,
            Max = 5,
        }

        /// <summary>
        /// Pinch gesture definition for the Oculus API
        /// </summary>
        [Flags]
        public enum HandFingerPinch
        {
            Thumb = (1 << HandFinger.Thumb),
            Index = (1 << HandFinger.Index),
            Middle = (1 << HandFinger.Middle),
            Ring = (1 << HandFinger.Ring),
            Pinky = (1 << HandFinger.Pinky),
        }

        /// <summary>
        /// Hand State definition for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct HandState
        {
            public HandStatus Status;
            public Posef RootPose;
            public Quatf[] BoneRotations;
            public HandFingerPinch Pinches;
            public float[] PinchStrength;
            public Posef PointerPose;
            public float HandScale;
            public TrackingConfidence HandConfidence;
            public TrackingConfidence[] FingerConfidences;
            public double RequestedTimeStamp;
            public double SampleTimeStamp;
        }

        /// <summary>
        /// Hand / Finger pose internal definition for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct HandStateInternal
        {
            public HandStatus Status;
            public Posef RootPose;
            public Quatf BoneRotations_0;
            public Quatf BoneRotations_1;
            public Quatf BoneRotations_2;
            public Quatf BoneRotations_3;
            public Quatf BoneRotations_4;
            public Quatf BoneRotations_5;
            public Quatf BoneRotations_6;
            public Quatf BoneRotations_7;
            public Quatf BoneRotations_8;
            public Quatf BoneRotations_9;
            public Quatf BoneRotations_10;
            public Quatf BoneRotations_11;
            public Quatf BoneRotations_12;
            public Quatf BoneRotations_13;
            public Quatf BoneRotations_14;
            public Quatf BoneRotations_15;
            public Quatf BoneRotations_16;
            public Quatf BoneRotations_17;
            public Quatf BoneRotations_18;
            public Quatf BoneRotations_19;
            public Quatf BoneRotations_20;
            public Quatf BoneRotations_21;
            public Quatf BoneRotations_22;
            public Quatf BoneRotations_23;
            public HandFingerPinch Pinches;
            public float PinchStrength_0;
            public float PinchStrength_1;
            public float PinchStrength_2;
            public float PinchStrength_3;
            public float PinchStrength_4;
            public Posef PointerPose;
            public float HandScale;
            public TrackingConfidence HandConfidence;
            public TrackingConfidence FingerConfidences_0;
            public TrackingConfidence FingerConfidences_1;
            public TrackingConfidence FingerConfidences_2;
            public TrackingConfidence FingerConfidences_3;
            public TrackingConfidence FingerConfidences_4;
            public double RequestedTimeStamp;
            public double SampleTimeStamp;
        }

        /// <summary>
        /// Bone collison definition for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct BoneCapsule
        {
            public short BoneIndex;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public Vector3f[] Points;
            public float Radius;
        }

        /// <summary>
        /// Bone structure for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Bone
        {
            public BoneId Id;
            public short ParentBoneIndex;
            public Posef Pose;
        }

        /// <summary>
        /// Skeletal constants for the Oculus API
        /// </summary>
        public enum SkeletonConstants
        {
            MaxBones = BoneId.Max,
            MaxBoneCapsules = 19,
        }

        /// <summary>
        /// Skeleton type definition for the Oculus API
        /// </summary>
        public enum SkeletonType
        {
            None = -1,
            HandLeft = 0,
            HandRight = 1,
        }

        /// <summary>
        /// Skeletal definition for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Skeleton
        {
            public SkeletonType Type;
            public uint NumBones;
            public uint NumBoneCapsules;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)SkeletonConstants.MaxBones)]
            public Bone[] Bones;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)SkeletonConstants.MaxBoneCapsules)]
            public BoneCapsule[] BoneCapsules;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Skeleton2
        {
            public SkeletonType Type;
            public uint NumBones;
            public uint NumBoneCapsules;
            public Bone[] Bones;
            public BoneCapsule[] BoneCapsules;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Skeleton2Internal
        {
            public SkeletonType Type;
            public uint NumBones;
            public uint NumBoneCapsules;
            public Bone Bones_0;
            public Bone Bones_1;
            public Bone Bones_2;
            public Bone Bones_3;
            public Bone Bones_4;
            public Bone Bones_5;
            public Bone Bones_6;
            public Bone Bones_7;
            public Bone Bones_8;
            public Bone Bones_9;
            public Bone Bones_10;
            public Bone Bones_11;
            public Bone Bones_12;
            public Bone Bones_13;
            public Bone Bones_14;
            public Bone Bones_15;
            public Bone Bones_16;
            public Bone Bones_17;
            public Bone Bones_18;
            public Bone Bones_19;
            public Bone Bones_20;
            public Bone Bones_21;
            public Bone Bones_22;
            public Bone Bones_23;
            public Bone Bones_24;
            public Bone Bones_25;
            public Bone Bones_26;
            public Bone Bones_27;
            public Bone Bones_28;
            public Bone Bones_29;
            public Bone Bones_30;
            public Bone Bones_31;
            public Bone Bones_32;
            public Bone Bones_33;
            public Bone Bones_34;
            public Bone Bones_35;
            public Bone Bones_36;
            public Bone Bones_37;
            public Bone Bones_38;
            public Bone Bones_39;
            public Bone Bones_40;
            public Bone Bones_41;
            public Bone Bones_42;
            public Bone Bones_43;
            public Bone Bones_44;
            public Bone Bones_45;
            public Bone Bones_46;
            public Bone Bones_47;
            public Bone Bones_48;
            public Bone Bones_49;
            public BoneCapsule BoneCapsules_0;
            public BoneCapsule BoneCapsules_1;
            public BoneCapsule BoneCapsules_2;
            public BoneCapsule BoneCapsules_3;
            public BoneCapsule BoneCapsules_4;
            public BoneCapsule BoneCapsules_5;
            public BoneCapsule BoneCapsules_6;
            public BoneCapsule BoneCapsules_7;
            public BoneCapsule BoneCapsules_8;
            public BoneCapsule BoneCapsules_9;
            public BoneCapsule BoneCapsules_10;
            public BoneCapsule BoneCapsules_11;
            public BoneCapsule BoneCapsules_12;
            public BoneCapsule BoneCapsules_13;
            public BoneCapsule BoneCapsules_14;
            public BoneCapsule BoneCapsules_15;
            public BoneCapsule BoneCapsules_16;
            public BoneCapsule BoneCapsules_17;
            public BoneCapsule BoneCapsules_18;
        }

        /// <summary>
        /// Raw Mesh constants for the Oculus API
        /// </summary>
        public enum MeshConstants
        {
            MaxVertices = 3000,
            MaxIndices = MaxVertices * 6,
        }

        /// <summary>
        /// Raw Mesh type definition for the Oculus API
        /// </summary>
        public enum MeshType
        {
            None = -1,
            HandLeft = 0,
            HandRight = 1,
        }

        /// <summary>
        /// Raw mesh definition for the Oculus API
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Mesh
        {
            public MeshType Type;
            public uint NumVertices;
            public uint NumIndices;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MeshConstants.MaxVertices)]
            public Vector3f[] VertexPositions;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MeshConstants.MaxIndices)]
            public short[] Indices;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MeshConstants.MaxVertices)]
            public Vector3f[] VertexNormals;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MeshConstants.MaxVertices)]
            public Vector2f[] VertexUV0;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MeshConstants.MaxVertices)]
            public Vector4s[] BlendIndices;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)MeshConstants.MaxVertices)]
            public Vector4f[] BlendWeights;
        }


        #endregion Hands Implementation

        #endregion Oculus Data Types

        #region Oculus Controller Definition

        internal abstract class OVRControllerBase
        {
            public Controller controllerType = Controller.None;
            public ControllerState4 previousState = new ControllerState4();
            public ControllerState4 currentState = new ControllerState4();
            public bool shouldApplyDeadzone = true;

            public virtual void SetControllerVibration(float frequency, float amplitude)
            {
                OculusApi.SetControllerVibration((uint)controllerType, frequency, amplitude);
            }

            public virtual void RecenterController()
            {
                RecenterTrackingOrigin(RecenterFlags.Default);
            }

            public virtual bool WasRecentered()
            {
                return false;
            }

            public virtual byte GetRecenterCount()
            {
                return 0;
            }

            public virtual byte GetBatteryPercentRemaining()
            {
                return 0;
            }
        }

        private class OVRControllerTouch : OVRControllerBase
        {
            public OVRControllerTouch()
            {
                controllerType = Controller.Touch;
            }

            public override bool WasRecentered()
            {
                return ((currentState.LRecenterCount + currentState.RRecenterCount) != (previousState.LRecenterCount + previousState.RRecenterCount));
            }

            public override byte GetRecenterCount()
            {
                return (byte)(currentState.LRecenterCount + currentState.RRecenterCount);
            }

            public override byte GetBatteryPercentRemaining()
            {
                byte leftBattery = currentState.LBatteryPercentRemaining;
                byte rightBattery = currentState.RBatteryPercentRemaining;
                byte minBattery = (leftBattery <= rightBattery) ? leftBattery : rightBattery;

                return minBattery;
            }
        }

        private class OVRControllerLTouch : OVRControllerBase
        {
            public OVRControllerLTouch()
            {
                controllerType = Controller.LTouch;
            }

            public override bool WasRecentered()
            {
                return (currentState.LRecenterCount != previousState.LRecenterCount);
            }

            public override byte GetRecenterCount()
            {
                return currentState.LRecenterCount;
            }

            public override byte GetBatteryPercentRemaining()
            {
                return currentState.LBatteryPercentRemaining;
            }
        }

        private class OVRControllerRTouch : OVRControllerBase
        {
            public OVRControllerRTouch()
            {
                controllerType = Controller.RTouch;
            }

            public override bool WasRecentered()
            {
                return (currentState.RRecenterCount != previousState.RRecenterCount);
            }

            public override byte GetRecenterCount()
            {
                return currentState.RRecenterCount;
            }

            public override byte GetBatteryPercentRemaining()
            {
                return currentState.RBatteryPercentRemaining;
            }
        }

        private class OVRControllerRemote : OVRControllerBase
        {
            public OVRControllerRemote()
            {
                controllerType = Controller.Remote;
            }
        }

        private class OVRControllerGamepadPC : OVRControllerBase
        {
            public OVRControllerGamepadPC()
            {
                controllerType = Controller.Gamepad;
            }
        }

        private class OVRControllerGamepadMac : OVRControllerBase
        {
            public OVRControllerGamepadMac()
            {
                controllerType = Controller.Gamepad;
            }
        }

        private class OVRControllerGamepadAndroid : OVRControllerBase
        {
            public OVRControllerGamepadAndroid()
            {
                controllerType = Controller.Gamepad;
            }
        }

        #endregion Oculus Controller Definition

        #region Oculus Positional Tracking

        /// <summary>
        /// Oculus native api translation, gets the current input state for all input based on the current controller bitmask
        /// </summary>
        /// <param name="controllerMask">Bit mask for the currently connected controllers</param>
        /// <returns>Oculus native controller definition containing the current state values for input</returns>
        public static ControllerState4 GetControllerState4(uint controllerMask)
        {
            ControllerState4 controllerState = new ControllerState4();
            ovrp_GetControllerState4(controllerMask, ref controllerState);
            return controllerState;
        }

        /// <summary>
        /// Oculus native api translation, gets the current input pose for a specific node type in the current render cycle
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <param name="stepId">render / physics step</param>
        /// <returns>Oculus native Vector3 detailing the current input pose</returns>
        public static Posef GetNodePose(Node nodeId, Step stepId)
        {
            return ovrp_GetNodePoseState(stepId, nodeId).Pose;
        }

        /// <summary>
        /// Oculus native api translation, gets the current input velocity for a specific node type in the current render cycle
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <param name="stepId">render / physics step</param>
        /// <returns>Oculus native Vector3 detailing the current input velocity</returns>
        public static Vector3f GetNodeVelocity(Node nodeId, Step stepId)
        {
            return ovrp_GetNodePoseState(stepId, nodeId).Velocity;
        }

        /// <summary>
        /// Oculus native api translation, gets the current input velocity for a specific node type in the current render cycle
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <param name="stepId">render / physics step</param>
        /// <returns>Oculus native Vector3 detailing the current input velocity</returns>
        public static PoseStatef GetNodeState(Node nodeId, Step stepId)
        {
            return ovrp_GetNodePoseState(stepId, nodeId);
        }

        /// <summary>
        /// Gets the position of the given Controller local to its tracking space.
        /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
        /// </summary>
        /// <param name="controllerType">Native Oculus Controller type</param>
        /// <returns>Position of the selected controller</returns>
        public static Vector3 GetLocalControllerPosition(Controller controllerType)
        {
            switch (controllerType)
            {
                case Controller.LTouch:
                    return GetNodePose(Node.HandLeft, stepType).Position;
                case Controller.RTouch:
                    return GetNodePose(Node.HandRight, stepType).Position;
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the dominant hand that the user has specified in settings, for mobile devices.
        /// </summary>
        /// <returns>Whether the left or right hand is dominant</returns>
        /// <remarks>For future use</remarks>
        public static Handedness GetDominantHand()
        {
            if (ovrp_GetDominantHand(out var dominantHand) == Result.Success)
            {
                return dominantHand;
            }

            return Handedness.Unsupported;
        }

        /// <summary>
        /// Test for whether a specific Oculus Node is currently connected
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <returns>True or False depending on whether the input was detected</returns>
        public static bool GetNodePresent(Node nodeId)
        {
            return ovrp_GetNodePresent(nodeId) == Bool.True;
        }

        /// <summary>
        /// Test for the orientation for a tracked controller
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <returns>True or False depending on whether the orientation is tracked</returns>
        public static bool GetNodeOrientationTracked(Node nodeId)
        {
            return ovrp_GetNodeOrientationTracked(nodeId) == Bool.True;
        }

        /// <summary>
        /// Test for whether a specific Oculus Node orientation is valid
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <returns>True or False depending on whether the orientation is valid</returns>
        public static bool GetNodeOrientationValid(Node nodeId)
        {
            if (Version >= OVRP_1_38_0_version)
            {
                Bool orientationValid = Bool.False;
                Result result = ovrp_GetNodeOrientationValid(nodeId, ref orientationValid);
                return result == Result.Success && orientationValid == Bool.True;
            }

            return GetNodeOrientationTracked(nodeId);
        }

        /// <summary>
        /// Test for the position for a tracked controller
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <returns>True or False depending on whether the position is tracked</returns>
        public static bool GetNodePositionTracked(Node nodeId)
        {
            return ovrp_GetNodePositionTracked(nodeId) == Bool.True;
        }

        /// <summary>
        /// Test for whether a specific Oculus Node position is valid
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <returns>True or False depending on whether the position is valid</returns>
        public static bool GetNodePositionValid(Node nodeId)
        {
            if (Version >= OVRP_1_38_0_version)
            {
                Bool positionValid = Bool.False;
                Result result = ovrp_GetNodePositionValid(nodeId, ref positionValid);
                return result == Result.Success && positionValid == Bool.True;
            }

            return GetNodePositionTracked(nodeId);
        }

        /// <summary>
        /// Update the Oculus API physics calculations in the Native API
        /// </summary>
        /// <param name="frameIndex">Current render frame</param>
        /// <param name="predictionSeconds">Seconds ahead to predict frames</param>
        /// <returns></returns>
        public static bool UpdateNodePhysicsPoses(int frameIndex, double predictionSeconds)
        {
            return ovrp_Update2((int)Step.Physics, frameIndex, predictionSeconds) == Bool.True;
        }

        public static TrackingOrigin GetTrackingOriginType()
        {
            return ovrp_GetTrackingOriginType();
        }

        public static bool SetTrackingOriginType(TrackingOrigin originType)
        {
            return ovrp_SetTrackingOriginType(originType) == Bool.True;
        }

        public static Posef GetTrackingCalibratedOrigin()
        {
            return ovrp_GetTrackingCalibratedOrigin();
        }

        public static bool SetTrackingCalibratedOrigin()
        {
            return ovrpi_SetTrackingCalibratedOrigin() == Bool.True;
        }

        public static bool RecenterTrackingOrigin(RecenterFlags flags)
        {
            return ovrp_RecenterTrackingOrigin((uint)flags) == Bool.True;
        }

        public static bool GetSystemHmd3DofModeEnabled()
        {
            if (Version >= OVRP_1_45_0_version)
            {
                Bool val = Bool.False;
                Result res = ovrp_GetSystemHmd3DofModeEnabled(ref val);
                if (res == Result.Success)
                {
                    return val == Bool.True;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        #endregion Oculus Positional Tracking

        #region Oculus Boundary Functions

        /// <summary>
        /// Oculus native api translation, for determining if the Boundary has been configured
        /// </summary>
        /// <returns></returns>
        public static bool GetBoundaryConfigured()
        {
            return ovrp_GetBoundaryConfigured() == Bool.True;
        }

        /// <summary>
        /// Oculus native api translation, to test whether a controller is within with the boundary
        /// </summary>
        /// <param name="nodeId">Oculus Node definition, e.g. LeftHand / RightHand</param>
        /// <param name="boundaryType">Oculus native type of boundary</param>
        /// <returns></returns>
        public static BoundaryTestResult TestBoundaryNode(Node nodeId, BoundaryType boundaryType)
        {
            return ovrp_TestBoundaryNode(nodeId, boundaryType);
        }

        /// <summary>
        /// Oculus native api translation, to test whether a point is within with the boundary
        /// </summary>
        /// <param name="point">Vector3 location to test</param>
        /// <param name="boundaryType">Oculus native type of boundary</param>
        /// <returns></returns>
        public static BoundaryTestResult TestBoundaryPoint(Vector3f point, BoundaryType boundaryType)
        {
            return ovrp_TestBoundaryPoint(point, boundaryType);
        }

        /// <summary>
        /// Oculus native api translation, for returning the points list of boundary elements
        /// </summary>
        /// <param name="boundaryType">Oculus native type of boundary</param>
        /// <param name="points">Pointer to a points list</param>
        /// <param name="pointsCount">Ref to a Points counter</param>
        /// <returns>Returns true if the boundary geometry was successfully retrieved </returns>
        /// <remarks>For future use</remarks>
        public static bool GetBoundaryGeometry(BoundaryType boundaryType, IntPtr points, ref int pointsCount)
        {
            return ovrp_GetBoundaryGeometry2(boundaryType, points, ref pointsCount) == Bool.True;
        }

        /// <summary>
        /// Oculus native api translation, for querying of the boundary dimensions from the API
        /// </summary>
        /// <param name="boundaryType">Oculus native <see cref="BoundaryType"/> definition </param>
        /// <returns>Oculus Vector 3 of boundary whole boundary dimension</returns>
        /// <remarks>For future use</remarks>
        public static Vector3f GetBoundaryDimensions(BoundaryType boundaryType)
        {
            return ovrp_GetBoundaryDimensions(boundaryType);
        }

        /// <summary>
        /// Oculus native api translation, for querying if the boundary is being displayed in the shell
        /// </summary>
        /// <returns>States whether boundary is currently visible</returns>
        /// <remarks>For future use</remarks>
        public static bool GetBoundaryVisible()
        {
            return ovrp_GetBoundaryVisible() == Bool.True;
        }

        /// <summary>
        /// Oculus native api translation, for setting the visibility of the shell boundary
        /// </summary>
        /// <param name="value">Whether you want to enable (true) or disable the boundary display</param>
        /// <returns>Returns true of a boundary is currently displayed</returns>
        /// <remarks>For future use</remarks>
        public static bool SetBoundaryVisible(bool value)
        {
            return ovrp_SetBoundaryVisible(value) == Bool.True;
        }

        public static bool InitializeInsightPassthrough()
        {
            if (Version >= OVRP_1_63_0_version)
            {
                Result result = ovrp_InitializeInsightPassthrough();
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ShutdownInsightPassthrough()
        {
            if (Version >= OVRP_1_63_0_version)
            {
                Result result = ovrp_ShutdownInsightPassthrough();
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsInsightPassthroughInitialized()
        {
            if (Version >= OVRP_1_63_0_version)
            {
                Bool result = ovrp_GetInsightPassthroughInitialized();
                return result == Bool.True;
            }
            else
            {
                return false;
            }
        }

        public static bool CreateInsightTriangleMesh(int layerId, Vector3[] vertices, int[] triangles, out ulong meshHandle)
        {
            meshHandle = 0;
            if (Version >= OVRP_1_63_0_version)
            {
                if (vertices == null || triangles == null || vertices.Length == 0 || triangles.Length == 0)
                {
                    return false;
                }
                int vertexCount = vertices.Length;
                int triangleCount = triangles.Length / 3;
                GCHandle pinnedVertexData = GCHandle.Alloc(vertices, GCHandleType.Pinned);
                IntPtr vertexDataPtr = pinnedVertexData.AddrOfPinnedObject();
                GCHandle pinnedTriangleData = GCHandle.Alloc(triangles, GCHandleType.Pinned);
                IntPtr triangleDataPtr = pinnedTriangleData.AddrOfPinnedObject();
                Result result = ovrp_CreateInsightTriangleMesh(
                    layerId, vertexDataPtr, vertexCount, triangleDataPtr, triangleCount, out meshHandle);
                pinnedTriangleData.Free();
                pinnedVertexData.Free();
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool DestroyInsightTriangleMesh(ulong meshHandle)
        {
            if (Version >= OVRP_1_63_0_version)
            {
                Result result = ovrp_DestroyInsightTriangleMesh(meshHandle);
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool AddInsightPassthroughSurfaceGeometry(int layerId, ulong meshHandle, Matrix4x4 T_world_model, out ulong geometryInstanceHandle)
        {
            geometryInstanceHandle = 0;
            if (Version >= OVRP_1_63_0_version)
            {
                Result result = ovrp_AddInsightPassthroughSurfaceGeometry(layerId, meshHandle, T_world_model, out geometryInstanceHandle);
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool DestroyInsightPassthroughGeometryInstance(ulong geometryInstanceHandle)
        {
            if (Version >= OVRP_1_63_0_version)
            {
                Result result = ovrp_DestroyInsightPassthroughGeometryInstance(geometryInstanceHandle);
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool UpdateInsightPassthroughGeometryTransform(ulong geometryInstanceHandle, Matrix4x4 transform)
        {
            if (Version >= OVRP_1_63_0_version)
            {
                Result result = ovrp_UpdateInsightPassthroughGeometryTransform(geometryInstanceHandle, transform);
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool SetInsightPassthroughStyle(int layerId, InsightPassthroughStyle style)
        {
            if (Version >= OVRP_1_63_0_version)
            {
                Result result = ovrp_SetInsightPassthroughStyle(layerId, style);
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Oculus Boundary Functions

        #region Oculus Controller Interactions

        /// <summary>
        /// Oculus native api translation, for getting the connected system headset type
        /// </summary>
        /// <returns>Native Oculus headset type definition</returns>
        /// <remarks>For future use</remarks>
        public static SystemHeadset GetSystemHeadsetType()
        {
            return ovrp_GetSystemHeadsetType();
        }

        /// <summary>
        /// Oculus native api translation, for getting the list of actively tracked controllers
        /// </summary>
        /// <returns>Returns a bitmask of all detected and tracked controllers</returns>
        public static Controller GetActiveController()
        {
            return ovrp_GetActiveController();
        }

        /// <summary>
        /// Oculus native api translation, for getting the list of connected controllers
        /// </summary>
        /// <returns>Returns a bitmask of all connected controllers</returns>
        public static Controller GetConnectedControllers()
        {
            return ovrp_GetConnectedControllers();
        }

        /// <summary>
        /// Oculus native api translation, sets the current controller vibration level
        /// </summary>
        /// <param name="controllerMask">Controller mask for all controllers to affect</param>
        /// <param name="frequency">Vibration frequency</param>
        /// <param name="amplitude">Vibration amplitude</param>
        /// <returns></returns>
        /// <remarks>For future use</remarks>
        public static bool SetControllerVibration(uint controllerMask, float frequency, float amplitude)
        {
            return ovrp_SetControllerVibration(controllerMask, frequency, amplitude) == Bool.True;
        }

        /// <summary>
        /// Oculus native api translation,  for getting the haptics profile for selected controllers
        /// </summary>
        /// <param name="controllerMask">Bitmask of controllers to query</param>
        /// <returns>A native haptics profile</returns>
        /// <remarks>For future use</remarks>
        public static HapticsDesc GetControllerHapticsDesc(uint controllerMask)
        {
            return ovrp_GetControllerHapticsDesc(controllerMask);
        }

        /// <summary>
        /// Oculus native api translation, for getting the current haptics state
        /// </summary>
        /// <param name="controllerMask">Bitmask of controllers to query</param>
        /// <returns>A native haptics states</returns>
        /// <remarks>For future use</remarks>
        public static HapticsState GetControllerHapticsState(uint controllerMask)
        {
            return ovrp_GetControllerHapticsState(controllerMask);
        }

        /// <summary>
        /// Oculus native api translation, to set the current haptics levels for selected controllers
        /// </summary>
        /// <param name="controllerMask">Bitmask of controllers to query</param>
        /// <param name="hapticsBuffer">Oculus native haptics buffer profile</param>
        /// <returns>Returns true if the haptics interaction was successful</returns>
        /// <remarks>For future use</remarks>
        public static bool SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer)
        {
            return ovrp_SetControllerHaptics(controllerMask, hapticsBuffer) == Bool.True;
        }

        #endregion Oculus Controller Interactions

        #region Oculus Input Functions

        internal static bool ShouldResolveController(Controller controllerType, Controller controllerMask)
        {
            bool isValid = false;

            if ((controllerType & controllerMask) == controllerType)
            {
                isValid = true;
            }

            // If the mask requests both Touch controllers, reject the individual touch controllers.
            if (((controllerMask & Controller.Touch) == Controller.Touch)
                && ((controllerType & Controller.Touch) != 0)
                && ((controllerType & Controller.Touch) != Controller.Touch))
            {
                isValid = false;
            }

            return isValid;
        }

        internal static Vector2 CalculateAbsMax(Vector2 a, Vector2 b)
        {
            float absA = a.sqrMagnitude;
            float absB = b.sqrMagnitude;

            if (absA >= absB)
            {
                return a;
            }

            return b;
        }

        internal static float CalculateAbsMax(float a, float b)
        {
            float absA = (a >= 0) ? a : -a;
            float absB = (b >= 0) ? b : -b;

            if (absA >= absB)
            {
                return a;
            }

            return b;
        }

        internal static Vector2 CalculateDeadzone(Vector2 a, float deadzone)
        {
            if (a.sqrMagnitude <= (deadzone * deadzone))
            {
                return Vector2.zero;
            }

            a *= ((a.magnitude - deadzone) / (1.0f - deadzone));

            if (a.sqrMagnitude > 1.0f)
            {
                return a.normalized;
            }

            return a;
        }

        internal static float CalculateDeadzone(float a, float deadzone)
        {
            float mag = (a >= 0) ? a : -a;

            if (mag <= deadzone)
            {
                return 0.0f;
            }

            a *= (mag - deadzone) / (1.0f - deadzone);

            if ((a * a) > 1.0f)
            {
                return (a >= 0) ? 1.0f : -1.0f;
            }

            return a;
        }

        public static float GetAdaptiveGPUPerformanceScale()
        {
            if (Version >= OVRP_1_42_0_version)
            {
                float adaptiveScale = 1.0f;
                if (ovrp_GetAdaptiveGpuPerformanceScale2(ref adaptiveScale) == Result.Success)
                {
                    return adaptiveScale;
                }
                return 1.0f;
            }
            else
            {
                return 1.0f;
            }
        }

        #endregion Oculus Input Functions

        #region Oculus Hands Interaction

        /// <summary>
        /// Verify the user has Hands Tracking enabled
        /// </summary>
        /// <returns>True if the user has enabled hands tracking in the headset </returns>
        /// <remarks>Only supported on headsets with V12 firmware+</remarks>
        public static bool GetHandTrackingEnabled()
        {
            if (Version >= OVRP_1_44_0_version)
            {
                Bool val = Bool.False;
                Result res = ovrp_GetHandTrackingEnabled(ref val);
                if (res == Result.Success)
                {
                    return val == Bool.True;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        private static HandStateInternal cachedHandState = new HandStateInternal();

        /// <summary>
        /// Updates a HandState reference with the currently reported hand data from the API
        /// </summary>
        /// <param name="stepId">Oculus API Step - Update/Physics</param>
        /// <param name="hand">Hand Node</param>
        /// <param name="handState">Current State reference for the hand data</param>
        /// <returns></returns>
        /// <remarks>Only supported on headsets with V12 firmware+</remarks>
        public static bool GetHandState(Step stepId, Hand hand, ref HandState handState)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                Result res = ovrp_GetHandState(stepId, hand, out cachedHandState);
                if (res == Result.Success)
                {
                    // attempt to avoid allocations if client provides appropriately pre-initialized HandState
                    if (handState.BoneRotations == null || handState.BoneRotations.Length != ((int)BoneId.Hand_End - (int)BoneId.Hand_Start))
                    {
                        handState.BoneRotations = new Quatf[(int)BoneId.Hand_End - (int)BoneId.Hand_Start];
                    }
                    if (handState.PinchStrength == null || handState.PinchStrength.Length != (int)HandFinger.Max)
                    {
                        handState.PinchStrength = new float[(int)HandFinger.Max];
                    }
                    if (handState.FingerConfidences == null || handState.FingerConfidences.Length != (int)HandFinger.Max)
                    {
                        handState.FingerConfidences = new TrackingConfidence[(int)HandFinger.Max];
                    }

                    // unrolling the arrays is necessary to avoid per-frame allocations during marshaling
                    handState.Status = cachedHandState.Status;
                    handState.RootPose = cachedHandState.RootPose;
                    handState.BoneRotations[0] = cachedHandState.BoneRotations_0;
                    handState.BoneRotations[1] = cachedHandState.BoneRotations_1;
                    handState.BoneRotations[2] = cachedHandState.BoneRotations_2;
                    handState.BoneRotations[3] = cachedHandState.BoneRotations_3;
                    handState.BoneRotations[4] = cachedHandState.BoneRotations_4;
                    handState.BoneRotations[5] = cachedHandState.BoneRotations_5;
                    handState.BoneRotations[6] = cachedHandState.BoneRotations_6;
                    handState.BoneRotations[7] = cachedHandState.BoneRotations_7;
                    handState.BoneRotations[8] = cachedHandState.BoneRotations_8;
                    handState.BoneRotations[9] = cachedHandState.BoneRotations_9;
                    handState.BoneRotations[10] = cachedHandState.BoneRotations_10;
                    handState.BoneRotations[11] = cachedHandState.BoneRotations_11;
                    handState.BoneRotations[12] = cachedHandState.BoneRotations_12;
                    handState.BoneRotations[13] = cachedHandState.BoneRotations_13;
                    handState.BoneRotations[14] = cachedHandState.BoneRotations_14;
                    handState.BoneRotations[15] = cachedHandState.BoneRotations_15;
                    handState.BoneRotations[16] = cachedHandState.BoneRotations_16;
                    handState.BoneRotations[17] = cachedHandState.BoneRotations_17;
                    handState.BoneRotations[18] = cachedHandState.BoneRotations_18;
                    handState.BoneRotations[19] = cachedHandState.BoneRotations_19;
                    handState.BoneRotations[20] = cachedHandState.BoneRotations_20;
                    handState.BoneRotations[21] = cachedHandState.BoneRotations_21;
                    handState.BoneRotations[22] = cachedHandState.BoneRotations_22;
                    handState.BoneRotations[23] = cachedHandState.BoneRotations_23;
                    handState.Pinches = cachedHandState.Pinches;
                    handState.PinchStrength[0] = cachedHandState.PinchStrength_0;
                    handState.PinchStrength[1] = cachedHandState.PinchStrength_1;
                    handState.PinchStrength[2] = cachedHandState.PinchStrength_2;
                    handState.PinchStrength[3] = cachedHandState.PinchStrength_3;
                    handState.PinchStrength[4] = cachedHandState.PinchStrength_4;
                    handState.PointerPose = cachedHandState.PointerPose;
                    handState.HandScale = cachedHandState.HandScale;
                    handState.HandConfidence = cachedHandState.HandConfidence;
                    handState.FingerConfidences[0] = cachedHandState.FingerConfidences_0;
                    handState.FingerConfidences[1] = cachedHandState.FingerConfidences_1;
                    handState.FingerConfidences[2] = cachedHandState.FingerConfidences_2;
                    handState.FingerConfidences[3] = cachedHandState.FingerConfidences_3;
                    handState.FingerConfidences[4] = cachedHandState.FingerConfidences_4;
                    handState.RequestedTimeStamp = cachedHandState.RequestedTimeStamp;
                    handState.SampleTimeStamp = cachedHandState.SampleTimeStamp;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the current skeletal definition for a specific hand
        /// </summary>
        /// <param name="skeletonType">Type of skeleton to query for</param>
        /// <param name="skeleton">(Out) Skeletal definition</param>
        /// <returns>True of the API was successful in retriving the hand data</returns>
        public static bool GetSkeleton(SkeletonType skeletonType, out Skeleton skeleton)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                return ovrp_GetSkeleton(skeletonType, out skeleton) == Result.Success;
            }
            else
            {
                skeleton = default(Skeleton);
                return false;
            }
        }

        private static Skeleton cachedSkeleton = new Skeleton();
        private static Skeleton2Internal cachedSkeleton2 = new Skeleton2Internal();

        /// <summary>
        /// Gets the current skeletal definition for a specific hand
        /// </summary>
        /// <param name="skeletonType">Type of skeleton to query for</param>
        /// <param name="skeleton">(Out) Skeletal definition</param>
        /// <returns>True of the API was successful in retriving the hand data</returns>
        public static bool GetSkeleton2(SkeletonType skeletonType, ref Skeleton2 skeleton)
        {
            if (Version >= OVRP_1_55_0_version)
            {
                Result res = ovrp_GetSkeleton2(skeletonType, out cachedSkeleton2);
                if (res == Result.Success)
                {
                    if (skeleton.Bones == null || skeleton.Bones.Length != (int)SkeletonConstants.MaxBones)
                    {
                        skeleton.Bones = new Bone[(int)SkeletonConstants.MaxBones];
                    }
                    if (skeleton.BoneCapsules == null || skeleton.BoneCapsules.Length != (int)SkeletonConstants.MaxBoneCapsules)
                    {
                        skeleton.BoneCapsules = new BoneCapsule[(int)SkeletonConstants.MaxBoneCapsules];
                    }

                    skeleton.Type = cachedSkeleton2.Type;
                    skeleton.NumBones = cachedSkeleton2.NumBones;
                    skeleton.NumBoneCapsules = cachedSkeleton2.NumBoneCapsules;
                    skeleton.Bones[0] = cachedSkeleton2.Bones_0;
                    skeleton.Bones[1] = cachedSkeleton2.Bones_1;
                    skeleton.Bones[2] = cachedSkeleton2.Bones_2;
                    skeleton.Bones[3] = cachedSkeleton2.Bones_3;
                    skeleton.Bones[4] = cachedSkeleton2.Bones_4;
                    skeleton.Bones[5] = cachedSkeleton2.Bones_5;
                    skeleton.Bones[6] = cachedSkeleton2.Bones_6;
                    skeleton.Bones[7] = cachedSkeleton2.Bones_7;
                    skeleton.Bones[8] = cachedSkeleton2.Bones_8;
                    skeleton.Bones[9] = cachedSkeleton2.Bones_9;
                    skeleton.Bones[10] = cachedSkeleton2.Bones_10;
                    skeleton.Bones[11] = cachedSkeleton2.Bones_11;
                    skeleton.Bones[12] = cachedSkeleton2.Bones_12;
                    skeleton.Bones[13] = cachedSkeleton2.Bones_13;
                    skeleton.Bones[14] = cachedSkeleton2.Bones_14;
                    skeleton.Bones[15] = cachedSkeleton2.Bones_15;
                    skeleton.Bones[16] = cachedSkeleton2.Bones_16;
                    skeleton.Bones[17] = cachedSkeleton2.Bones_17;
                    skeleton.Bones[18] = cachedSkeleton2.Bones_18;
                    skeleton.Bones[19] = cachedSkeleton2.Bones_19;
                    skeleton.Bones[20] = cachedSkeleton2.Bones_20;
                    skeleton.Bones[21] = cachedSkeleton2.Bones_21;
                    skeleton.Bones[22] = cachedSkeleton2.Bones_22;
                    skeleton.Bones[23] = cachedSkeleton2.Bones_23;
                    skeleton.Bones[24] = cachedSkeleton2.Bones_24;
                    skeleton.Bones[25] = cachedSkeleton2.Bones_25;
                    skeleton.Bones[26] = cachedSkeleton2.Bones_26;
                    skeleton.Bones[27] = cachedSkeleton2.Bones_27;
                    skeleton.Bones[28] = cachedSkeleton2.Bones_28;
                    skeleton.Bones[29] = cachedSkeleton2.Bones_29;
                    skeleton.Bones[30] = cachedSkeleton2.Bones_30;
                    skeleton.Bones[31] = cachedSkeleton2.Bones_31;
                    skeleton.Bones[32] = cachedSkeleton2.Bones_32;
                    skeleton.Bones[33] = cachedSkeleton2.Bones_33;
                    skeleton.Bones[34] = cachedSkeleton2.Bones_34;
                    skeleton.Bones[35] = cachedSkeleton2.Bones_35;
                    skeleton.Bones[36] = cachedSkeleton2.Bones_36;
                    skeleton.Bones[37] = cachedSkeleton2.Bones_37;
                    skeleton.Bones[38] = cachedSkeleton2.Bones_38;
                    skeleton.Bones[39] = cachedSkeleton2.Bones_39;
                    skeleton.Bones[40] = cachedSkeleton2.Bones_40;
                    skeleton.Bones[41] = cachedSkeleton2.Bones_41;
                    skeleton.Bones[42] = cachedSkeleton2.Bones_42;
                    skeleton.Bones[43] = cachedSkeleton2.Bones_43;
                    skeleton.Bones[44] = cachedSkeleton2.Bones_44;
                    skeleton.Bones[45] = cachedSkeleton2.Bones_45;
                    skeleton.Bones[46] = cachedSkeleton2.Bones_46;
                    skeleton.Bones[47] = cachedSkeleton2.Bones_47;
                    skeleton.Bones[48] = cachedSkeleton2.Bones_48;
                    skeleton.Bones[49] = cachedSkeleton2.Bones_49;
                    skeleton.BoneCapsules[0] = cachedSkeleton2.BoneCapsules_0;
                    skeleton.BoneCapsules[1] = cachedSkeleton2.BoneCapsules_1;
                    skeleton.BoneCapsules[2] = cachedSkeleton2.BoneCapsules_2;
                    skeleton.BoneCapsules[3] = cachedSkeleton2.BoneCapsules_3;
                    skeleton.BoneCapsules[4] = cachedSkeleton2.BoneCapsules_4;
                    skeleton.BoneCapsules[5] = cachedSkeleton2.BoneCapsules_5;
                    skeleton.BoneCapsules[6] = cachedSkeleton2.BoneCapsules_6;
                    skeleton.BoneCapsules[7] = cachedSkeleton2.BoneCapsules_7;
                    skeleton.BoneCapsules[8] = cachedSkeleton2.BoneCapsules_8;
                    skeleton.BoneCapsules[9] = cachedSkeleton2.BoneCapsules_9;
                    skeleton.BoneCapsules[10] = cachedSkeleton2.BoneCapsules_10;
                    skeleton.BoneCapsules[11] = cachedSkeleton2.BoneCapsules_11;
                    skeleton.BoneCapsules[12] = cachedSkeleton2.BoneCapsules_12;
                    skeleton.BoneCapsules[13] = cachedSkeleton2.BoneCapsules_13;
                    skeleton.BoneCapsules[14] = cachedSkeleton2.BoneCapsules_14;
                    skeleton.BoneCapsules[15] = cachedSkeleton2.BoneCapsules_15;
                    skeleton.BoneCapsules[16] = cachedSkeleton2.BoneCapsules_16;
                    skeleton.BoneCapsules[17] = cachedSkeleton2.BoneCapsules_17;
                    skeleton.BoneCapsules[18] = cachedSkeleton2.BoneCapsules_18;

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (GetSkeleton(skeletonType, out cachedSkeleton))
                {
                    if (skeleton.Bones == null || skeleton.Bones.Length != (int)SkeletonConstants.MaxBones)
                    {
                        skeleton.Bones = new Bone[(int)SkeletonConstants.MaxBones];
                    }
                    if (skeleton.BoneCapsules == null || skeleton.BoneCapsules.Length != (int)SkeletonConstants.MaxBoneCapsules)
                    {
                        skeleton.BoneCapsules = new BoneCapsule[(int)SkeletonConstants.MaxBoneCapsules];
                    }

                    skeleton.Type = cachedSkeleton.Type;
                    skeleton.NumBones = cachedSkeleton.NumBones;
                    skeleton.NumBoneCapsules = cachedSkeleton.NumBoneCapsules;

                    for (int i = 0; i < skeleton.NumBones; i++)
                    {
                        skeleton.Bones[i] = cachedSkeleton.Bones[i];
                    }

                    for (int i = 0; i < skeleton.NumBoneCapsules; i++)
                    {
                        skeleton.BoneCapsules[i] = cachedSkeleton.BoneCapsules[i];
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the mesh for the detected hand
        /// </summary>
        /// <param name="meshType">Type of mesh to query for</param>
        /// <param name="mesh">(Out) Mesh instance to return</param>
        /// <returns></returns>
        public static bool GetMesh(MeshType meshType, out Mesh mesh)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                return ovrp_GetMesh(meshType, out mesh) == Result.Success;
            }
            else
            {
                mesh = default(Mesh);
                return false;
            }
        }

        #endregion Oculus Hands Interaction

        #region Oculus Camera

        /// <summary>
        /// Get the current camera properties from the headset via the Oculus API
        /// </summary>
        /// <param name="cameraId">Camera ID to query</param>
        /// <param name="cameraExtrinsics">(Out) Extrinsics defintion for the sepected camera</param>
        /// <param name="cameraIntrinsics">(Out) Intrinsics defintion for the sepected camera</param>
        /// <param name="calibrationRawPose">(Out) calibration raw pose defintion for the sepected camera</param>
        /// <returns>True if the selected camera returned data</returns>
        public static bool GetMixedRealityCameraInfo(int cameraId, out CameraExtrinsics cameraExtrinsics, out CameraIntrinsics cameraIntrinsics, out Posef calibrationRawPose)
        {
            cameraExtrinsics = default(CameraExtrinsics);
            cameraIntrinsics = default(CameraIntrinsics);
            calibrationRawPose = Posef.identity;

            if (Version >= OVRP_1_38_0_version)
            {
                bool retValue = true;

                Result result = ovrp_GetExternalCameraExtrinsics(cameraId, out cameraExtrinsics);
                if (result != Result.Success)
                {
                    retValue = false;
                }

                result = ovrp_GetExternalCameraIntrinsics(cameraId, out cameraIntrinsics);
                if (result != Result.Success)
                {
                    retValue = false;
                }

                result = ovrp_GetExternalCameraCalibrationRawPose(cameraId, out calibrationRawPose);
                if (result != Result.Success)
                {
                    retValue = false;
                }

                return retValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Overrides the selected camera's FOV
        /// </summary>
        /// <param name="cameraId">Camera ID to query</param>
        /// <param name="useOverriddenFov">Force the specific fov to the camera</param>
        /// <param name="fov">Fov definiton to override with</param>
        /// <returns>True if the override was successful</returns>
        public static bool OverrideExternalCameraFov(int cameraId, bool useOverriddenFov, Fovf fov)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                bool retValue = true;
                Result result = ovrp_OverrideExternalCameraFov(cameraId, useOverriddenFov ? Bool.True : Bool.False, ref fov);
                if (result != Result.Success)
                {
                    retValue = false;
                }
                return retValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Is the current FOV for the selected camera overridden
        /// </summary>
        /// <param name="cameraId">Camera ID to query</param>
        /// <returns>True of the camera FOV is overridden</returns>
        public static bool GetUseOverriddenExternalCameraFov(int cameraId)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                bool retValue = true;
                Bool useOverriddenFov = Bool.False;
                Result result = ovrp_GetUseOverriddenExternalCameraFov(cameraId, out useOverriddenFov);
                if (result != Result.Success)
                {
                    retValue = false;
                }
                if (useOverriddenFov == Bool.False)
                {
                    retValue = false;
                }
                return retValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Override the pose of the selected camera
        /// </summary>
        /// <param name="cameraId">Camera ID to query</param>
        /// <param name="useOverriddenPose">Force the specific pose on the camera</param>
        /// <param name="pose">Pose definition to override the camera with</param>
        /// <returns>True if the override was successful</returns>
        public static bool OverrideExternalCameraStaticPose(int cameraId, bool useOverriddenPose, Posef pose)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                bool retValue = true;
                Result result = ovrp_OverrideExternalCameraStaticPose(cameraId, useOverriddenPose ? Bool.True : Bool.False, ref pose);
                if (result != Result.Success)
                {
                    retValue = false;
                }
                return retValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Is the current Pose for the selected camera overridden
        /// </summary>
        /// <param name="cameraId">Camera ID to query</param>
        /// <returns>True of the camera Pose is overridden</returns>
        public static bool GetUseOverriddenExternalCameraStaticPose(int cameraId)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                bool retValue = true;
                Bool useOverriddenStaticPose = Bool.False;
                Result result = ovrp_GetUseOverriddenExternalCameraStaticPose(cameraId, out useOverriddenStaticPose);
                if (result != Result.Success)
                {
                    retValue = false;
                }
                if (useOverriddenStaticPose == Bool.False)
                {
                    retValue = false;
                }
                return retValue;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Reset any overrides placed on the default camera
        /// </summary>
        /// <returns>True of the camera was reset</returns>
        public static bool ResetDefaultExternalCamera()
        {
            if (Version >= OVRP_1_44_0_version)
            {
                Result result = ovrp_ResetDefaultExternalCamera();
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Set the default external camera runtime behaviour
        /// </summary>
        /// <param name="cameraName">name to apply to the default camera</param>
        /// <param name="cameraIntrinsics">(Out) Intrinsics defintion for the sepected camera</param>
        /// <param name="cameraExtrinsics">(Out) Extrinsics defintion for the sepected camera</param>
        /// <returns>True if the camera was set as default</returns>
        public static bool SetDefaultExternalCamera(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics)
        {
            if (Version >= OVRP_1_44_0_version)
            {
                Result result = ovrp_SetDefaultExternalCamera(cameraName, ref cameraIntrinsics, ref cameraExtrinsics);
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Function to query the device to check if fixed foveated rendering is supported
        /// </summary>
        public static bool fixedFoveatedRenderingSupported
        {
            get
            {
                Bool supported;
                Result result = ovrp_GetTiledMultiResSupported(out supported);
                if (result == Result.Success)
                {
                    return supported == Bool.True;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Function to query the device to report on the fixed foveated rendering level applied
        /// </summary>
        public static FixedFoveatedRenderingLevel fixedFoveatedRenderingLevel
        {
            get
            {
                if (fixedFoveatedRenderingSupported)
                {
                    FixedFoveatedRenderingLevel level;
                    Result result = ovrp_GetTiledMultiResLevel(out level);
                    return level;
                }
                else
                {
                    return FixedFoveatedRenderingLevel.Off;
                }
            }
            set
            {
                if (fixedFoveatedRenderingSupported)
                {
                    Result result = ovrp_SetTiledMultiResLevel(value);
                }
            }
        }

        /// <summary>
        /// Function to inform the device to enable dynamic foveated rendering
        /// </summary>
        public static bool useDynamicFixedFoveatedRendering
        {
            get
            {
                if (Version >= OVRP_1_46_0_version && fixedFoveatedRenderingSupported)
                {
                    Bool isDynamic = Bool.False;
                    Result result = ovrp_GetTiledMultiResDynamic(out isDynamic);
                    return isDynamic != Bool.False;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (Version >= OVRP_1_46_0_version && fixedFoveatedRenderingSupported)
                {
                    Result result = ovrp_SetTiledMultiResDynamic(value ? Bool.True : Bool.False);
                }
            }
        }

        /// <summary>
        /// Set the default camera
        /// </summary>
        /// <param name="cameraName">name to apply to the default camera</param>
        /// <param name="cameraIntrinsics">(Out) Intrinsics defintion for the sepected camera</param>
        /// <param name="cameraExtrinsics">(Out) Extrinsics defintion for the sepected camera</param>
        /// <returns>True if the camera was set as default</returns>
        public static bool SetExternalCameraProperties(string cameraName, ref CameraIntrinsics cameraIntrinsics, ref CameraExtrinsics cameraExtrinsics)
        {
            if (Version >= OVRP_1_48_0_version)
            {
                Result result = ovrp_SetExternalCameraProperties(cameraName, ref cameraIntrinsics, ref cameraExtrinsics);
                if (result != Result.Success)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets the current headset controller pose
        /// </summary>
        /// <param name="headsetPose">Headset pose to apply</param>
        /// <param name="leftControllerPose">Left controller pose to apply</param>
        /// <param name="rightControllerPose">Right controller pose to apply</param>
        /// <returns>Returns true if the pose was applied successfully</returns>
        public static bool SetMrcHeadsetControllerPose(Posef headsetPose, Posef leftControllerPose, Posef rightControllerPose)
        {
            if (Version >= OVRP_1_49_0_version)
            {
                Result res = ovrp_Media_SetHeadsetControllerPose(headsetPose, leftControllerPose, rightControllerPose);
                return res == Result.Success;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Sets a new color description for the client
        /// </summary>
        /// <param name="colorSpace">New color space to apply</param>
        /// <returns></returns>
        public static bool SetClientColorDesc(ColorSpace colorSpace)
        {
            if (Version >= OVRP_1_49_0_version)
            {
#if UNITY_ANDROID
                if (colorSpace == ColorSpace.Unknown)
                {
                    colorSpace = ColorSpace.Quest;
                }
#endif
                return ovrp_SetClientColorDesc(colorSpace) == Result.Success;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Get the current color description for the headset
        /// </summary>
        /// <returns>Returns a ColorSpace definition</returns>
        public static ColorSpace GetHmdColorDesc()
        {
            ColorSpace colorSpace = ColorSpace.Unknown;
            if (Version >= OVRP_1_49_0_version)
            {
                Result res = ovrp_GetHmdColorDesc(ref colorSpace);
                if (res != Result.Success)
                {
                    Debug.LogError("GetHmdColorDesc: Failed to get Hmd color description");
                }
                return colorSpace;
            }
            else
            {
                Debug.LogError("GetHmdColorDesc: Not supported on this version of OVRPlugin");
                return colorSpace;
            }
        }

        /// <summary>
        /// Polls the current data buffer from the environment
        /// </summary>
        /// <param name="eventDataBuffer">(out) Populated EventBuffer</param>
        /// <returns>Returns true if the poll was successful</returns>
        public static bool PollEvent(ref EventDataBuffer eventDataBuffer)
        {
#if OVRPLUGIN_UNSUPPORTED_PLATFORM
		eventDataBuffer = default(EventDataBuffer);
		return false;
#else
            if (Version >= OVRP_1_55_1_version)
            {
                IntPtr DataPtr = IntPtr.Zero;
                if (eventDataBuffer.EventData == null)
                {
                    eventDataBuffer.EventData = new byte[EventDataBufferSize];
                }
                Result result = ovrp_PollEvent2(ref eventDataBuffer.EventType, ref DataPtr);

                if (result != Result.Success || DataPtr == IntPtr.Zero)
                {
                    return false;
                }

                Marshal.Copy(DataPtr, eventDataBuffer.EventData, 0, EventDataBufferSize);
                return true;
            }
            else if (Version >= OVRP_1_55_0_version)
            {
                return ovrp_PollEvent(ref eventDataBuffer) == Result.Success;
            }
            else
            {
                eventDataBuffer = default(EventDataBuffer);
                return false;
            }
#endif
        }

        /// <summary>
        /// Gets the curent instance of the XR session
        /// </summary>
        /// <returns>Returns the indiex of the current native buffer</returns>
        public static UInt64 GetNativeOpenXRInstance()
        {
            if (Version >= OVRP_1_55_0_version)
            {
                UInt64 instance, session;
                if (ovrp_GetNativeOpenXRHandles(out instance, out session) == Result.Success)
                {
                    return instance;
                }
            }
            return 0;
        }

        /// <summary>
        /// Gets the native pointer reference the the existing session
        /// </summary>
        /// <returns>Returns the indiex of the current native buffer</returns>
        public static UInt64 GetNativeOpenXRSession()
        {
            if (Version >= OVRP_1_55_0_version)
            {
                UInt64 instance, session;
                if (ovrp_GetNativeOpenXRHandles(out instance, out session) == Result.Success)
                {
                    return session;
                }
            }
            return 0;
        }


        public static bool SetKeyboardOverlayUV(Vector2f uv)
        {
            if (Version >= OVRP_1_57_0_version)
            {
                Result result = ovrp_SetKeyboardOverlayUV(uv);
                return (result == Result.Success);
            }
            else
            {
                return false;
            }
        }

        public static bool eyeFovPremultipliedAlphaModeEnabled
        {
            get
            {
                Bool isEnabled = Bool.True;
                if (Version >= OVRP_1_57_0_version)
                {
                    ovrp_GetEyeFovPremultipliedAlphaMode(ref isEnabled);
                }
                return isEnabled == Bool.True ? true : false;
            }
            set
            {
                if (Version >= OVRP_1_57_0_version)
                {
                    ovrp_SetEyeFovPremultipliedAlphaMode(ToBool(value));
                }
            }
        }

        #endregion
    }
}