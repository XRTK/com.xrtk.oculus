// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace XRTK.Oculus
{
    /// <summary>
    /// Shamelessly lifted from the UnityEngine.XR.MagicLeap packages, but Unity had to make their class internal. Boo.
    /// </summary>
    public static class OculusApi
    {
        #region Oculus API Properties

	    private static System.Version _versionZero = new System.Version(0, 0, 0);
        private static readonly System.Version OVRP_1_38_0_version = new System.Version(1, 38, 0);
        private const string pluginName = "OVRPlugin";


        private static System.Version _version;

        public static System.Version version
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
                            _version = new System.Version(pluginVersion);
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


        internal static OculusApi.Controller activeControllerType = OculusApi.Controller.None;
        internal static OculusApi.Controller connectedControllerTypes = OculusApi.Controller.None;

        #endregion Oculus API Properties

        #region Oculus API import - tbc

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "ovrp_GetVersion")]
        private static extern IntPtr _ovrp_GetVersion();
        public static string ovrp_GetVersion() { return Marshal.PtrToStringAnsi(_ovrp_GetVersion()); }

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_GetControllerState4(uint controllerMask, ref ControllerState4 controllerState);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern PoseStatef ovrp_GetNodePoseState(Step stepId, Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_GetDominantHand(out Handedness dominantHand);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetNodePresent(Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetNodeOrientationTracked(Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetNodePositionTracked(Node nodeId);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_GetNodeOrientationValid(Node nodeId, ref Bool nodeOrientationValid);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Result ovrp_GetNodePositionValid(Node nodeId, ref Bool nodePositionValid);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetBoundaryGeometry2(BoundaryType boundaryType, IntPtr points, ref int pointsCount);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Vector3f ovrp_GetBoundaryDimensions(BoundaryType boundaryType);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_GetBoundaryVisible();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_SetBoundaryVisible(bool value);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern SystemHeadset ovrp_GetSystemHeadsetType();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Controller ovrp_GetActiveController();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Controller ovrp_GetConnectedControllers();

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_Update2(int stateId, int frameIndex, double predictionSeconds);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern HapticsDesc ovrp_GetControllerHapticsDesc(uint controllerMask);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern HapticsState ovrp_GetControllerHapticsState(uint controllerMask);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer);

        [DllImport(pluginName, CallingConvention = CallingConvention.Cdecl)]
        public static extern Bool ovrp_RecenterTrackingOrigin(uint flags);

        #endregion Oculus API import - tbc

        #region Oculus Data Types

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
        }

        public enum Bool
        {
            False = 0,
            True
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector2f
        {
            public float x;
            public float y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vector3f
        {
            public float x;
            public float y;
            public float z;
            public static readonly Vector3f zero = new Vector3f { x = 0.0f, y = 0.0f, z = 0.0f };
            public override string ToString()
            {
                return string.Format("{0}, {1}, {2}", x, y, z);
            }
        }

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
                return string.Format("{0}, {1}, {2}, {3}", x, y, z, w);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Posef
        {
            public Quatf Orientation;
            public Vector3f Position;
            public static readonly Posef identity = new Posef { Orientation = Quatf.identity, Position = Vector3f.zero };
            public override string ToString()
            {
                return string.Format("Position ({0}), Orientation({1})", Position, Orientation);
            }
        }

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
            Count,
        }

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

        [StructLayout(LayoutKind.Sequential)]
        public struct HapticsBuffer
        {
            public IntPtr Samples;
            public int SamplesCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HapticsState
        {
            public int SamplesAvailable;
            public int SamplesQueued;
        }

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

        [Flags]
        /// Raw button mappings that can be used to directly query the state of a controller.
        public enum RawButton
        {
            None = 0,          ///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            A = 0x00000001, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: A], [LTrackedRemote: LIndexTrigger], [RTrackedRemote: RIndexTrigger], [LTouch, Touchpad, Remote: None]
            B = 0x00000002, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: B], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            X = 0x00000100, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: X], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Y = 0x00000200, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: Y], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Start = 0x00100000, ///< Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Start], [RTouch: None]
            Back = 0x00200000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Back], [Touch, LTouch, RTouch: None]
            LShoulder = 0x00000800, ///< Maps to Physical Button: [Gamepad: LShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LIndexTrigger = 0x10000000, ///< Maps to Physical Button: [Gamepad, Touch, LTouch, LTrackedRemote: LIndexTrigger], [RTouch, RTrackedRemote, Touchpad, Remote: None]
            LHandTrigger = 0x20000000, ///< Maps to Physical Button: [Touch, LTouch: LHandTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstick = 0x00000400, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstick], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickUp = 0x00000010, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickUp], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickDown = 0x00000020, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickDown], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickLeft = 0x00000040, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickLeft], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstickRight = 0x00000080, ///< Maps to Physical Button: [Gamepad, Touch, LTouch: LThumbstickRight], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LTouchpad = 0x40000000, ///< Maps to Physical Button: [LTrackedRemote: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Touchpad, Remote: None]
            RShoulder = 0x00000008, ///< Maps to Physical Button: [Gamepad: RShoulder], [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RIndexTrigger = 0x04000000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch, RTrackedRemote: RIndexTrigger], [LTouch, LTrackedRemote, Touchpad, Remote: None]
            RHandTrigger = 0x08000000, ///< Maps to Physical Button: [Touch, RTouch: RHandTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstick = 0x00000004, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstick], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickUp = 0x00001000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickUp], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickDown = 0x00002000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickDown], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickLeft = 0x00004000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickLeft], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstickRight = 0x00008000, ///< Maps to Physical Button: [Gamepad, Touch, RTouch: RThumbstickRight], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RTouchpad = unchecked((int)0x80000000),///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            DpadUp = 0x00010000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadUp], [Touch, LTouch, RTouch: None]
            DpadDown = 0x00020000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadDown], [Touch, LTouch, RTouch: None]
            DpadLeft = 0x00040000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadLeft], [Touch, LTouch, RTouch: None]
            DpadRight = 0x00080000, ///< Maps to Physical Button: [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: DpadRight], [Touch, LTouch, RTouch: None]
            Any = ~None,      ///< Maps to Physical Button: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: Any]
        }

        [Flags]
        /// Raw capacitive touch mappings that can be used to directly query the state of a controller.
        public enum RawTouch
        {
            None = 0,                            ///< Maps to Physical Touch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            A = RawButton.A,                  ///< Maps to Physical Touch: [Touch, RTouch: A], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            B = RawButton.B,                  ///< Maps to Physical Touch: [Touch, RTouch: B], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            X = RawButton.X,                  ///< Maps to Physical Touch: [Touch, LTouch: X], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Y = RawButton.Y,                  ///< Maps to Physical Touch: [Touch, LTouch: Y], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LIndexTrigger = 0x00001000,                   ///< Maps to Physical Touch: [Touch, LTouch: LIndexTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstick = RawButton.LThumbstick,        ///< Maps to Physical Touch: [Touch, LTouch: LThumbstick], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbRest = 0x00000800,                   ///< Maps to Physical Touch: [Touch, LTouch: LThumbRest], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LTouchpad = RawButton.LTouchpad,          ///< Maps to Physical Touch: [LTrackedRemote, Touchpad: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Remote: None]
            RIndexTrigger = 0x00000010,                   ///< Maps to Physical Touch: [Touch, RTouch: RIndexTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbstick = RawButton.RThumbstick,        ///< Maps to Physical Touch: [Touch, RTouch: RThumbstick], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbRest = 0x00000008,                   ///< Maps to Physical Touch: [Touch, RTouch: RThumbRest], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RTouchpad = RawButton.RTouchpad,          ///< Maps to Physical Touch: [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, Touchpad, Remote: None]
            Any = ~None,                        ///< Maps to Physical Touch: [Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad: Any], [Gamepad, Remote: None]
        }

        [Flags]
        /// Raw near touch mappings that can be used to directly query the state of a controller.
        public enum RawNearTouch
        {
            None = 0,          ///< Maps to Physical NearTouch: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LIndexTrigger = 0x00000001, ///< Maps to Physical NearTouch: [Touch, LTouch: Implies finger is in close proximity to LIndexTrigger.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbButtons = 0x00000002, ///< Maps to Physical NearTouch: [Touch, LTouch: Implies thumb is in close proximity to LThumbstick OR X/Y buttons.], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RIndexTrigger = 0x00000004, ///< Maps to Physical NearTouch: [Touch, RTouch: Implies finger is in close proximity to RIndexTrigger.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RThumbButtons = 0x00000008, ///< Maps to Physical NearTouch: [Touch, RTouch: Implies thumb is in close proximity to RThumbstick OR A/B buttons.], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Any = ~None,      ///< Maps to Physical NearTouch: [Touch, LTouch, RTouch: Any], [Gamepad, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
        }

        [Flags]
        /// Raw 1-dimensional axis (float) mappings that can be used to directly query the state of a controller.
        public enum RawAxis1D
        {
            None = 0,     ///< Maps to Physical Axis1D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LIndexTrigger = 0x01,  ///< Maps to Physical Axis1D: [Gamepad, Touch, LTouch: LIndexTrigger], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LHandTrigger = 0x04,  ///< Maps to Physical Axis1D: [Touch, LTouch: LHandTrigger], [Gamepad, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RIndexTrigger = 0x02,  ///< Maps to Physical Axis1D: [Gamepad, Touch, RTouch: RIndexTrigger], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RHandTrigger = 0x08,  ///< Maps to Physical Axis1D: [Touch, RTouch: RHandTrigger], [Gamepad, LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            Any = ~None, ///< Maps to Physical Axis1D: [Gamepad, Touch, LTouch, RTouch: Any], [LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
        }

        [Flags]
        /// Raw 2-dimensional axis (Vector2) mappings that can be used to directly query the state of a controller.
        public enum RawAxis2D
        {
            None = 0,     ///< Maps to Physical Axis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LThumbstick = 0x01,  ///< Maps to Physical Axis2D: [Gamepad, Touch, LTouch: LThumbstick], [RTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            LTouchpad = 0x04,  ///< Maps to Physical Axis2D: [LTrackedRemote, Touchpad: LTouchpad], [Gamepad, Touch, LTouch, RTouch, RTrackedRemote, Remote: None]
            RThumbstick = 0x02,  ///< Maps to Physical Axis2D: [Gamepad, Touch, RTouch: RThumbstick], [LTouch, LTrackedRemote, RTrackedRemote, Touchpad, Remote: None]
            RTouchpad = 0x08,  ///< Maps to Physical Axis2D: [RTrackedRemote: RTouchpad], [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, Touchpad, Remote: None]
            Any = ~None, ///< Maps to Physical Axis2D: [Gamepad, Touch, LTouch, RTouch, LTrackedRemote, RTrackedRemote: Any], [Touchpad, Remote: None]
        }

        [Flags]
        /// Identifies a controller which can be used to query the virtual or raw input state.
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
            All = ~None,
        }

        public enum Handedness
        {
            Unsupported = 0,
            LeftHanded = 1,
            RightHanded = 2,
        }

        public enum Step
        {
            Render = -1,
            Physics = 0,
        }


        public enum TrackingOrigin
        {
            EyeLevel = 0,
            FloorLevel = 1,
            Stage = 2,
            Count,
        }

        public enum RecenterFlags
        {
            Default = 0,
            Controllers = 0x40000000,
            IgnoreAll = unchecked((int)0x80000000),
            Count,
        }

        public enum BatteryStatus
        {
            Charging = 0,
            Discharging,
            Full,
            NotCharging,
            Unknown,
        }

        public enum BoundaryType
        {
            OuterBoundary = 0x0001,
            PlayArea = 0x0100,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BoundaryTestResult
        {
            public Bool IsTriggering;
            public float ClosestDistance;
            public Vector3f ClosestPoint;
            public Vector3f ClosestPointNormal;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BoundaryGeometry
        {
            public BoundaryType BoundaryType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public Vector3f[] Points;
            public int PointsCount;
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
                OculusApi.RecenterTrackingOrigin(RecenterFlags.Controllers);
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

        public static ControllerState4 GetControllerState4(uint controllerMask)
        {
                ControllerState4 controllerState = new ControllerState4();
                ovrp_GetControllerState4(controllerMask, ref controllerState);
                return controllerState;
        }

        public static Posef GetNodePose(Node nodeId, Step stepId)
        {
                return ovrp_GetNodePoseState(stepId, nodeId).Pose;
        }

        public static Vector3f GetNodeVelocity(Node nodeId, Step stepId)
        {
                return ovrp_GetNodePoseState(stepId, nodeId).Velocity;
        }

        /// <summary>
        /// Gets the position of the given Controller local to its tracking space.
        /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
        /// </summary>
        public static Vector3 GetLocalControllerPosition(Controller controllerType)
        {
            switch (controllerType)
            {
                case Controller.LTouch:
                case Controller.LTrackedRemote:
                        return GetNodePose(Node.HandLeft, stepType).ToMixedRealityPose().Position;
                    //else
                    //{
                    //    Vector3 retVec;
                    //    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.LeftHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandLeft, stepType, out retVec))
                    //        return retVec;
                    //    return Vector3.zero;                //Will never be hit, but is a final fallback.
                    //}
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                        return GetNodePose(Node.HandRight, stepType).ToMixedRealityPose().Position;
                    //{
                    //    Vector3 retVec;
                    //    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.RightHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandRight, stepType, out retVec))
                    //        return retVec;
                    //    return Vector3.zero;
                    //}
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the dominant hand that the user has specified in settings, for mobile devices.
        /// </summary>
        public static Handedness GetDominantHand()
        {
            Handedness dominantHand;

            if (ovrp_GetDominantHand(out dominantHand) == Result.Success)
            {
                return dominantHand;
            }

            return Handedness.Unsupported;
        }

        public static bool GetNodePresent(Node nodeId)
        {
            return ovrp_GetNodePresent(nodeId) == Bool.True;
        }

        public static bool GetNodeOrientationTracked(Node nodeId)
        {
            return ovrp_GetNodeOrientationTracked(nodeId) == Bool.True;
        }

        public static bool GetNodeOrientationValid(Node nodeId)
        {
            if (version >= OVRP_1_38_0_version)
            {
                Bool orientationValid = Bool.False;
                Result result = ovrp_GetNodeOrientationValid(nodeId, ref orientationValid);
                return result == Result.Success && orientationValid == Bool.True;
            }
            else
            {
                return GetNodeOrientationTracked(nodeId);
            }
        }

        public static bool GetNodePositionTracked(Node nodeId)
        {
            return ovrp_GetNodePositionTracked(nodeId) == Bool.True;
        }

        public static bool GetNodePositionValid(Node nodeId)
        {
            if (version >= OVRP_1_38_0_version)
            {
                Bool positionValid = Bool.False;
                Result result = ovrp_GetNodePositionValid(nodeId, ref positionValid);
                return result == Result.Success && positionValid == Bool.True;
            }
            else
            {
                return GetNodePositionTracked(nodeId);
            }
        }

        public static bool UpdateNodePhysicsPoses(int frameIndex, double predictionSeconds)
        {
                return ovrp_Update2((int)Step.Physics, frameIndex, predictionSeconds) == Bool.True;
        }

        #endregion Oculus Positional Tracking

        #region Oculus Controller Interactions

        public static Vector3f GetBoundaryDimensions(BoundaryType boundaryType)
        {
                return ovrp_GetBoundaryDimensions(boundaryType);
        }

        public static bool GetBoundaryVisible()
        {

                return ovrp_GetBoundaryVisible() == Bool.True;
        }

        public static bool SetBoundaryVisible(bool value)
        {
                return ovrp_SetBoundaryVisible(value) == Bool.True;
        }

        public static SystemHeadset GetSystemHeadsetType()
        {
                return ovrp_GetSystemHeadsetType();
        }

        public static Controller GetActiveController()
        {
                return ovrp_GetActiveController();

        }

        public static Controller GetConnectedControllers()
        {
                return ovrp_GetConnectedControllers();
        }

        public static bool SetControllerVibration(uint controllerMask, float frequency, float amplitude)
        {
            return false;
            //return OVRP_0_1_2.ovrp_SetControllerVibration(controllerMask, frequency, amplitude) == Bool.True;
        }


        public static HapticsDesc GetControllerHapticsDesc(uint controllerMask)
        {
            return ovrp_GetControllerHapticsDesc(controllerMask);
        }

        public static HapticsState GetControllerHapticsState(uint controllerMask)
        {
                return ovrp_GetControllerHapticsState(controllerMask);
        }

        public static bool SetControllerHaptics(uint controllerMask, HapticsBuffer hapticsBuffer)
        {
                return ovrp_SetControllerHaptics(controllerMask, hapticsBuffer) == Bool.True;
        }

        public static bool RecenterTrackingOrigin(RecenterFlags flags)
        { 
            return ovrp_RecenterTrackingOrigin((uint)flags) == Bool.True;
        }

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
            Rift_S,
        }

        public static bool GetBoundaryGeometry(BoundaryType boundaryType, IntPtr points, ref int pointsCount)
        {
                return ovrp_GetBoundaryGeometry2(boundaryType, points, ref pointsCount) == Bool.True;
        }

        #endregion Oculus Controller Interactions

        #region Oculus Input Validation

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

        #endregion Oculus Input Validation

        #region XRTKExtensions

        public static Definitions.Utilities.MixedRealityPose ToMixedRealityPose(this Posef p)
        {
            return new Definitions.Utilities.MixedRealityPose
            (
                position: new Vector3(p.Position.x, p.Position.y, -p.Position.z),
                rotation: new Quaternion(-p.Orientation.x, -p.Orientation.y, p.Orientation.z, p.Orientation.w)
            );
        }

        #endregion
    }
}