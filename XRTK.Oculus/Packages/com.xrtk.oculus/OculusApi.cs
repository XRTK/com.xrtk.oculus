// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using XRTK.Definitions.Utilities;

namespace XRTK.Oculus
{
    public static class OculusApi
    {
        #region Oculus API Properties

        private static Version _versionZero = new Version(0, 0, 0);
        private static readonly Version OVRP_1_38_0_version = new Version(1, 38, 0);
        private const string pluginName = "OVRPlugin";

        private static Version _version;

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
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log($"Oculus API version detected was - [{_version.ToString()}]");
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
                        new OVRControllerTouchpad(),
                        new OVRControllerLTrackedRemote(),
                        new OVRControllerRTrackedRemote(),
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

        public static bool Initialized
        {
            get
            {
                return ovrp_GetInitialized() == Bool.True;
            }
        }

        public static float EyeDepth
        {
            get
            {
                if (!Initialized)
                    return 0.0f;

                return ovrp_GetUserEyeDepth();
            }
            set
            {
                ovrp_SetUserEyeDepth(value);
            }
        }

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

        public static bool UserPresent
        {
            get
            {
                return Initialized && ovrp_GetUserPresent() == Bool.True;
            }
        }

        #endregion Oculus API Properties

        #region Oculus API import

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
            Failure_InsufficientSize = -1007
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
        /// Oculus API native Vector2
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Vector2f
        {
            public float x;
            public float y;
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
                return $"{x}, {y}, {z}";
            }
        }

        /// <summary>
        /// Oculus API native Quaternion
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
                return $"{x}, {y}, {z}, {w}";
            }
        }

        /// <summary>
        /// Oculus API native Pose (Position + Rotation)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Posef
        {
            public Quatf Orientation;
            public Vector3f Position;
            public static readonly Posef identity = new Posef { Orientation = Quatf.identity, Position = Vector3f.zero };
            public override string ToString()
            {
                return $"Position ({Position}), Orientation({Orientation})";
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
            Touchpad = 0x08000000,
            LTrackedRemote = 0x01000000,
            RTrackedRemote = 0x02000000,
            Active = unchecked((int)0x80000000),
            All = ~None
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
            Controllers = 0x40000000,
            IgnoreAll = unchecked((int)0x80000000),
            Count
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
        /// Type of headset detected by the Oculus API
        /// </summary>
        public enum SystemHeadset
        {
            None = 0,
            GearVR_R320, // Note4 Innovator
            GearVR_R321, // S6 Innovator
            GearVR_R322, // Commercial 1
            GearVR_R323, // Commercial 2 (USB Type C)
            GearVR_R324, // Commercial 3 (USB Type C)
            GearVR_R325, // Commercial 4 (USB Type C)
            Oculus_Go,
            Oculus_Quest,

            Rift_DK1 = 0x1000,
            Rift_DK2,
            Rift_CV1,
            Rift_CB,
            Rift_S
        }

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
                RecenterTrackingOrigin(RecenterFlags.Controllers);
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

        private class OVRControllerTouchpad : OVRControllerBase
        {
            public OVRControllerTouchpad()
            {
                controllerType = Controller.Touchpad;
            }
        }

        private class OVRControllerLTrackedRemote : OVRControllerBase
        {
            public OVRControllerLTrackedRemote()
            {
                controllerType = Controller.LTrackedRemote;
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

        private class OVRControllerRTrackedRemote : OVRControllerBase
        {
            public OVRControllerRTrackedRemote()
            {
                controllerType = Controller.RTrackedRemote;
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
                case Controller.LTrackedRemote:
                    return GetNodePose(Node.HandLeft, stepType).GetPosePosition();
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                    return GetNodePose(Node.HandRight, stepType).GetPosePosition();
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
                return a;
            return b;
        }

        internal static float CalculateAbsMax(float a, float b)
        {
            float absA = (a >= 0) ? a : -a;
            float absB = (b >= 0) ? b : -b;

            if (absA >= absB)
                return a;
            return b;
        }

        internal static Vector2 CalculateDeadzone(Vector2 a, float deadzone)
        {
            if (a.sqrMagnitude <= (deadzone * deadzone))
                return Vector2.zero;

            a *= ((a.magnitude - deadzone) / (1.0f - deadzone));

            if (a.sqrMagnitude > 1.0f)
                return a.normalized;
            return a;
        }

        internal static float CalculateDeadzone(float a, float deadzone)
        {
            float mag = (a >= 0) ? a : -a;

            if (mag <= deadzone)
                return 0.0f;

            a *= (mag - deadzone) / (1.0f - deadzone);

            if ((a * a) > 1.0f)
                return (a >= 0) ? 1.0f : -1.0f;
            return a;
        }

        #endregion Oculus Input Functions

        #region XRTKExtensions

        /// <summary>
        /// Extension method to convert a Oculus Pose to an XRTK MixedRealityPose
        /// </summary>
        /// <param name="xrtkPose"></param>
        /// <param name="pose">Extension (this) base Oculus PoseF type</param>
        /// <returns>Returns an XRTK MixedRealityPose</returns>
        public static MixedRealityPose ToMixedRealityPose(this MixedRealityPose xrtkPose, Posef pose, bool adjustForEyeHeight = false)
        {
            var position = xrtkPose.Position;

            position.x = pose.Position.x;
            position.y = adjustForEyeHeight ? pose.Position.y + EyeHeight : pose.Position.y;
            position.z = -pose.Position.z;

            xrtkPose.Position = position;

            var rotation = xrtkPose.Rotation;

            rotation.x = -pose.Orientation.x;
            rotation.y = -pose.Orientation.y;
            rotation.z = pose.Orientation.z;
            rotation.w = pose.Orientation.w;

            xrtkPose.Rotation = rotation;

            return xrtkPose;
        }

        /// <summary>
        /// Gets a <see cref="UnityEngine.Vector3"/> position from the <see cref="Posef"/>.
        /// </summary>
        /// <param name="pose"></param>
        public static Vector3 GetPosePosition(this Posef pose)
        {
            return new Vector3(pose.Position.x, pose.Position.y, -pose.Position.z);
        }

        #endregion XRTKExtensions
    }
}