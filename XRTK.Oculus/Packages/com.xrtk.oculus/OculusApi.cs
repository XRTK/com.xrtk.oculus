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

        private const string UNITY_MAGIC_LEAP_DLL = "UnityMagicLeap";

        internal static readonly float AXIS_AS_BUTTON_THRESHOLD = 0.5f;
        internal static readonly float AXIS_DEADZONE_THRESHOLD = 0.2f;

        internal static OVRPlugin.Step stepType = OVRPlugin.Step.Render;

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

        //[DllImport(UNITY_MAGIC_LEAP_DLL)]
        //public static extern void UnityMagicLeap_MeshingUpdateSettings(MeshingSettings newSettings);
        #endregion Oculus API import - tbc

        #region Oculus Data Types

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
            None = OVRPlugin.Controller.None,           ///< Null controller.
            LTouch = OVRPlugin.Controller.LTouch,         ///< Left Oculus Touch controller. Virtual input mapping differs from the combined L/R Touch mapping.
            RTouch = OVRPlugin.Controller.RTouch,         ///< Right Oculus Touch controller. Virtual input mapping differs from the combined L/R Touch mapping.
            Touch = OVRPlugin.Controller.Touch,          ///< Combined Left/Right pair of Oculus Touch controllers.
            Remote = OVRPlugin.Controller.Remote,         ///< Oculus Remote controller.
            Gamepad = OVRPlugin.Controller.Gamepad,        ///< Xbox 360 or Xbox One gamepad on PC. Generic gamepad on Android.
            Touchpad = OVRPlugin.Controller.Touchpad,       ///< GearVR touchpad on Android.
            LTrackedRemote = OVRPlugin.Controller.LTrackedRemote, ///< Left GearVR tracked remote on Android.
            RTrackedRemote = OVRPlugin.Controller.RTrackedRemote, ///< Right GearVR tracked remote on Android.
            Active = OVRPlugin.Controller.Active,         ///< Default controller. Represents the controller that most recently registered a button press from the user.
            All = OVRPlugin.Controller.All,            ///< Represents the logical OR of all controllers.
        }

        public enum Handedness
        {
            Unsupported = OVRPlugin.Handedness.Unsupported,
            LeftHanded = OVRPlugin.Handedness.LeftHanded,
            RightHanded = OVRPlugin.Handedness.RightHanded,
        }

        public enum Step
        {
            Render = -1,
            Physics = 0,
        }

        #endregion Oculus Data Types

        #region Oculus Controller Definition

        internal abstract class OVRControllerBase
        {
            public Controller controllerType = Controller.None;
            public OVRPlugin.ControllerState4 previousState = new OVRPlugin.ControllerState4();
            public OVRPlugin.ControllerState4 currentState = new OVRPlugin.ControllerState4();
            public bool shouldApplyDeadzone = true;

            public virtual void SetControllerVibration(float frequency, float amplitude)
            {
                OVRPlugin.SetControllerVibration((uint)controllerType, frequency, amplitude);
            }

            public virtual void RecenterController()
            {
                OVRPlugin.RecenterTrackingOrigin(OVRPlugin.RecenterFlags.Controllers);
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
                        return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, stepType).ToOVRPose().position;
                    //else
                    //{
                    //    Vector3 retVec;
                    //    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(Node.LeftHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandLeft, stepType, out retVec))
                    //        return retVec;
                    //    return Vector3.zero;                //Will never be hit, but is a final fallback.
                    //}
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                        return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, stepType).ToOVRPose().position;
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
        /// Gets the linear velocity of the given Controller local to its tracking space.
        /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
        /// </summary>
        public static Vector3 GetLocalControllerVelocity(Controller controllerType)
        {
            Vector3 velocity = Vector3.zero;

            switch (controllerType)
            {
                case Controller.LTouch:
                case Controller.LTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.LeftHand, NodeStatePropertyType.Velocity, OVRPlugin.Node.HandLeft, stepType, out velocity))
                    {
                        return velocity;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.RightHand, NodeStatePropertyType.Velocity, OVRPlugin.Node.HandRight, stepType, out velocity))
                    {
                        return velocity;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the linear acceleration of the given Controller local to its tracking space.
        /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
        /// </summary>
        public static Vector3 GetLocalControllerAcceleration(Controller controllerType)
        {
            Vector3 accel = Vector3.zero;

            switch (controllerType)
            {
                case Controller.LTouch:
                case Controller.LTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.LeftHand, NodeStatePropertyType.Acceleration, OVRPlugin.Node.HandLeft, stepType, out accel))
                    {
                        return accel;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.RightHand, NodeStatePropertyType.Acceleration, OVRPlugin.Node.HandRight, stepType, out accel))
                    {
                        return accel;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the rotation of the given Controller local to its tracking space.
        /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Quaternion.identity.
        /// </summary>
        public static Quaternion GetLocalControllerRotation(Controller controllerType)
        {
            switch (controllerType)
            {
                case Controller.LTouch:
                case Controller.LTrackedRemote:
                        return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, stepType).ToOVRPose().orientation;
                    //{
                    //    Quaternion retQuat;
                    //    if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.LeftHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandLeft, stepType, out retQuat))
                    //        return retQuat;
                    //    return Quaternion.identity;
                    //}
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                        return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, stepType).ToOVRPose().orientation;
                    //{
                    //    Quaternion retQuat;
                    //    if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(Node.RightHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandRight, stepType, out retQuat))
                    //        return retQuat;
                    //    return Quaternion.identity;
                    //}
                default:
                    return Quaternion.identity;
            }
        }

        /// <summary>
        /// Gets the angular velocity of the given Controller local to its tracking space in radians per second around each axis.
        /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
        /// </summary>
        public static Vector3 GetLocalControllerAngularVelocity(Controller controllerType)
        {
            Vector3 velocity = Vector3.zero;

            switch (controllerType)
            {
                case Controller.LTouch:
                case Controller.LTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.LeftHand, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.HandLeft, stepType, out velocity))
                    {
                        return velocity;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.RightHand, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.HandRight, stepType, out velocity))
                    {
                        return velocity;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the angular acceleration of the given Controller local to its tracking space in radians per second per second around each axis.
        /// Only supported for Oculus LTouch and RTouch controllers. Non-tracked controllers will return Vector3.zero.
        /// </summary>
        public static Vector3 GetLocalControllerAngularAcceleration(Controller controllerType)
        {
            Vector3 accel = Vector3.zero;

            switch (controllerType)
            {
                case Controller.LTouch:
                case Controller.LTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.LeftHand, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.HandLeft, stepType, out accel))
                    {
                        return accel;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                case Controller.RTouch:
                case Controller.RTrackedRemote:
                    if (OVRNodeStateProperties.GetNodeStatePropertyVector3(UnityEngine.XR.XRNode.RightHand, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.HandRight, stepType, out accel))
                    {
                        return accel;
                    }
                    else
                    {
                        return Vector3.zero;
                    }
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Gets the dominant hand that the user has specified in settings, for mobile devices.
        /// </summary>
        public static Handedness GetDominantHand()
        {
            return (Handedness)OVRPlugin.GetDominantHand();
        }

        #endregion Oculus Positional Tracking

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

        public static Definitions.Utilities.MixedRealityPose ToMixedRealityPose(this OVRPlugin.Posef p)
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