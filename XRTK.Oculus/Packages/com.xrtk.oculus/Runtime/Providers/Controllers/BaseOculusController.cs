// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Extensions;
using XRTK.Interfaces.Providers.Controllers;
using XRTK.Oculus.Extensions;
using XRTK.Oculus.Plugins;
using XRTK.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Oculus.Providers.Controllers
{
    public abstract class BaseOculusController : BaseController
    {
        /// <inheritdoc />
        protected BaseOculusController() { }

        /// <inheritdoc />
        protected BaseOculusController(IMixedRealityControllerDataProvider controllerDataProvider, TrackingState trackingState, Handedness controllerHandedness, MixedRealityControllerMappingProfile controllerMappingProfile, OculusApi.Controller controllerType = OculusApi.Controller.None, OculusApi.Node nodeType = OculusApi.Node.None)
            : base(controllerDataProvider, trackingState, controllerHandedness, controllerMappingProfile)
        {
            ControllerType = controllerType;
            NodeType = nodeType;
        }

        /// <summary>
        /// The Oculus Node Type.
        /// </summary>
        private OculusApi.Node NodeType { get; }

        /// <inheritdoc />
        protected override MixedRealityPose GripPoseOffset => new MixedRealityPose(Vector3.zero, Quaternion.Euler(0f, 0f, -90f));

        /// <summary>
        /// The Oculus Controller Type.
        /// </summary>
        private OculusApi.Controller ControllerType { get; }

        private OculusApi.ControllerState4 previousState = new OculusApi.ControllerState4();
        private OculusApi.ControllerState4 currentState = new OculusApi.ControllerState4();

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping("Button.A Press", AxisType.Digital, "A", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.B Press", AxisType.Digital, "B", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.X Press", AxisType.Digital, "X", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Y Press", AxisType.Digital, "Y", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Start Press", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Back Press", AxisType.Digital, "Back", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.LShoulder Press", AxisType.Digital, "LShoulder", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Axis1D.LIndexTrigger", AxisType.SingleAxis, "LIndexTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis1D.LIndexTrigger Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerTouch),
            new MixedRealityInteractionMapping("Axis1D.LIndexTrigger Near Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerNearTouch),
            new MixedRealityInteractionMapping("Axis1D.LIndexTrigger Press", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping("Axis1D.LHandTrigger Press", AxisType.SingleAxis, "LHandTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis2D.LThumbstick", AxisType.DualAxis, "PrimaryThumbstick", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping("Button.LThumbstick Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickTouch),
            new MixedRealityInteractionMapping("Button.LThumbstick Near Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbNearTouch),
            new MixedRealityInteractionMapping("Button.LThumbstick Press", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Button.RShoulder Press", AxisType.Digital, "RShoulder", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Axis1D.RIndexTrigger", AxisType.SingleAxis, "RIndexTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis1D.RIndexTrigger Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerTouch),
            new MixedRealityInteractionMapping("Axis1D.RIndexTrigger Near Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerNearTouch),
            new MixedRealityInteractionMapping("Axis1D.RIndexTrigger Press", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping("Axis1D.RHandTrigger Press", AxisType.SingleAxis, "RHandTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis2D.RThumbstick", AxisType.DualAxis, "SecondaryThumbstick", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping("Button.RThumbstick Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickTouch),
            new MixedRealityInteractionMapping("Button.RThumbstick Near Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbNearTouch),
            new MixedRealityInteractionMapping("Button.RThumbstick Press", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Axis2D.Dpad", AxisType.DualAxis, DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping("Button.DpadUp Press", AxisType.Digital, "DpadUp", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Button.DpadDown Press", AxisType.Digital, "DpadDown", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Button.DpadLeft Press", AxisType.Digital, "DpadLeft", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Button.DpadRight Press", AxisType.Digital, "DpadRight", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Button.RTouchpad", AxisType.Digital, "RTouchpad", DeviceInputType.ThumbTouch),
            new MixedRealityInteractionMapping("Grip Pose", AxisType.SixDof, DeviceInputType.SpatialGrip)
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        private MixedRealityPose currentPointerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose lastControllerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose currentControllerPose = MixedRealityPose.ZeroIdentity;
        private OculusApi.PoseStatef currentControllerVelocity = new OculusApi.PoseStatef();
        private float singleAxisValue = 0.0f;
        private Vector2 dualAxisPosition = Vector2.zero;

        /// <inheritdoc />
        public override void UpdateController()
        {
            if (!Enabled) { return; }

            base.UpdateController();

            UpdateControllerData();

            if (Interactions == null)
            {
                Debug.LogError($"No interaction configuration for Oculus Controller {ControllerHandedness}");
                Enabled = false;
            }

            for (int i = 0; i < Interactions?.Length; i++)
            {
                var interactionMapping = Interactions[i];

                switch (interactionMapping.InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        UpdatePoseData(interactionMapping);
                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.ButtonPress:
                    case DeviceInputType.TriggerPress:
                    case DeviceInputType.ThumbStickPress:
                        UpdateButtonDataPress(interactionMapping);
                        break;
                    case DeviceInputType.ButtonTouch:
                    case DeviceInputType.TriggerTouch:
                    case DeviceInputType.ThumbTouch:
                    case DeviceInputType.TouchpadTouch:
                    case DeviceInputType.ThumbStickTouch:
                        UpdateButtonDataTouch(interactionMapping);
                        break;
                    case DeviceInputType.ButtonNearTouch:
                    case DeviceInputType.TriggerNearTouch:
                    case DeviceInputType.ThumbNearTouch:
                    case DeviceInputType.TouchpadNearTouch:
                    case DeviceInputType.ThumbStickNearTouch:
                        UpdateButtonDataNearTouch(interactionMapping);
                        break;
                    case DeviceInputType.Trigger:
                        UpdateSingleAxisData(interactionMapping);
                        break;
                    case DeviceInputType.ThumbStick:
                    case DeviceInputType.Touchpad:
                        UpdateDualAxisData(interactionMapping);
                        break;
                    case DeviceInputType.SpatialGrip:
                        UpdateSpatialGripData(interactionMapping);
                        break;
                    default:
                        Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                        break;
                }

                interactionMapping.RaiseInputAction(InputSource, ControllerHandedness);
            }
        }

        private void UpdateControllerData()
        {
            var lastState = TrackingState;
            lastControllerPose = currentControllerPose;
            previousState = currentState;

            currentState = OculusApi.GetControllerState4((uint)ControllerType);

            if (currentState.LIndexTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LIndexTrigger;
            }

            if (currentState.LHandTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LHandTrigger;
            }

            if (currentState.LThumbstick.y >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickUp;
            }

            if (currentState.LThumbstick.y <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickDown;
            }

            if (currentState.LThumbstick.x <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickLeft;
            }

            if (currentState.LThumbstick.x >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.LThumbstickRight;
            }

            if (currentState.RIndexTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RIndexTrigger;
            }

            if (currentState.RHandTrigger >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RHandTrigger;
            }

            if (currentState.RThumbstick.y >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickUp;
            }

            if (currentState.RThumbstick.y <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickDown;
            }

            if (currentState.RThumbstick.x <= -OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickLeft;
            }

            if (currentState.RThumbstick.x >= OculusApi.AXIS_AS_BUTTON_THRESHOLD)
            {
                currentState.Buttons |= (uint)OculusApi.RawButton.RThumbstickRight;
            }

            if (IsTrackedController(ControllerType))
            {
                // The source is either a hand or a controller that supports pointing.
                // We can now check for position and rotation.
                IsPositionAvailable = OculusApi.GetNodePositionTracked(NodeType);
                IsPositionApproximate = IsPositionAvailable && OculusApi.GetNodePositionValid(NodeType);
                IsRotationAvailable = OculusApi.GetNodeOrientationTracked(NodeType);

                // Devices are considered tracked if we receive position OR rotation data from the sensors.
                TrackingState = (IsPositionAvailable || IsRotationAvailable) ? TrackingState.Tracked : TrackingState.NotTracked;
            }
            else
            {
                // The input source does not support tracking.
                TrackingState = TrackingState.NotApplicable;
            }

            currentControllerPose = OculusApi.GetNodePose(NodeType, OculusApi.stepType).ToMixedRealityPose(true);
            currentControllerVelocity = OculusApi.GetNodeState(NodeType, OculusApi.stepType);
            Velocity = currentControllerVelocity.Velocity.ToVector3();
            AngularVelocity = currentControllerVelocity.AngularVelocity.ToVector3();

            // Raise input system events if it is enabled.
            if (lastState != TrackingState)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceTrackingStateChanged(InputSource, this, TrackingState);
            }

            if (TrackingState == TrackingState.Tracked && lastControllerPose != currentControllerPose)
            {
                if (IsPositionAvailable && IsRotationAvailable)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourcePoseChanged(InputSource, this, currentControllerPose);
                }
                else if (IsPositionAvailable && !IsRotationAvailable)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourcePositionChanged(InputSource, this, currentControllerPose.Position);
                }
                else if (!IsPositionAvailable && IsRotationAvailable)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourceRotationChanged(InputSource, this, currentControllerPose.Rotation);
                }
            }
        }

        private static bool IsTrackedController(OculusApi.Controller controller)
        {
            return controller == OculusApi.Controller.Touch ||
                   controller == OculusApi.Controller.LTouch ||
                   controller == OculusApi.Controller.RTouch;
        }

        private void UpdateButtonDataPress(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            Enum.TryParse<OculusApi.RawButton>(interactionMapping.InputName, out var interactionButton);

            if (interactionButton != OculusApi.RawButton.None)
            {
                if (((OculusApi.RawButton)currentState.Buttons & interactionButton) != 0)
                {
                    interactionMapping.BoolData = true;
                }
                else if (((OculusApi.RawButton)currentState.Buttons & interactionButton) == 0 &&
                        ((OculusApi.RawButton)previousState.Buttons & interactionButton) != 0)
                {
                    interactionMapping.BoolData = false;
                }
            }
        }

        private void UpdateButtonDataTouch(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            Enum.TryParse<OculusApi.RawTouch>(interactionMapping.InputName, out var interactionButton);

            if (interactionButton != OculusApi.RawTouch.None)
            {
                if (((OculusApi.RawTouch)currentState.Touches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = true;
                }
                else if (((OculusApi.RawTouch)currentState.Touches & interactionButton) == 0 &&
                        ((OculusApi.RawTouch)previousState.Touches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = false;
                }
            }
        }

        private void UpdateButtonDataNearTouch(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            Enum.TryParse<OculusApi.RawNearTouch>(interactionMapping.InputName, out var interactionButton);

            if (interactionButton != OculusApi.RawNearTouch.None)
            {
                if (((OculusApi.RawNearTouch)currentState.NearTouches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = true;
                }
                else if (((OculusApi.RawNearTouch)currentState.NearTouches & interactionButton) == 0 &&
                        ((OculusApi.RawNearTouch)previousState.NearTouches & interactionButton) != 0)
                {
                    interactionMapping.BoolData = false;
                }
            }
        }

        private void UpdateSingleAxisData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SingleAxis);

            Enum.TryParse<OculusApi.RawAxis1D>(interactionMapping.InputName, out var interactionAxis1D);

            if (interactionAxis1D != OculusApi.RawAxis1D.None)
            {
                switch (interactionAxis1D)
                {
                    case OculusApi.RawAxis1D.LIndexTrigger:
                        singleAxisValue = currentState.LIndexTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                    case OculusApi.RawAxis1D.LHandTrigger:
                        singleAxisValue = currentState.LHandTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                    case OculusApi.RawAxis1D.RIndexTrigger:
                        singleAxisValue = currentState.RIndexTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                    case OculusApi.RawAxis1D.RHandTrigger:
                        singleAxisValue = currentState.RHandTrigger;

                        singleAxisValue = OculusApi.CalculateAbsMax(0, singleAxisValue);
                        break;
                }
            }

            // Update the interaction data source
            interactionMapping.FloatData = singleAxisValue;
        }

        private void UpdateSpatialGripData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);
            interactionMapping.PoseData = new MixedRealityPose(
                currentControllerPose.Position + currentControllerPose.Rotation * GripPoseOffset.Position,
                currentControllerPose.Rotation * GripPoseOffset.Rotation);
        }

        private void UpdateDualAxisData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.DualAxis);

            Enum.TryParse<OculusApi.RawAxis2D>(interactionMapping.InputName, out var interactionAxis2D);

            if (interactionAxis2D != OculusApi.RawAxis2D.None)
            {
                switch (interactionAxis2D)
                {
                    case OculusApi.RawAxis2D.LThumbstick:
                        dualAxisPosition.x = currentState.LThumbstick.x;
                        dualAxisPosition.y = currentState.LThumbstick.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                    case OculusApi.RawAxis2D.LTouchpad:
                        dualAxisPosition.x = currentState.LTouchpad.x;
                        dualAxisPosition.y = currentState.LTouchpad.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                    case OculusApi.RawAxis2D.RThumbstick:
                        dualAxisPosition.x = currentState.RThumbstick.x;
                        dualAxisPosition.y = currentState.RThumbstick.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                    case OculusApi.RawAxis2D.RTouchpad:
                        dualAxisPosition.x = currentState.RTouchpad.x;
                        dualAxisPosition.y = currentState.RTouchpad.y;

                        dualAxisPosition = OculusApi.CalculateAbsMax(Vector2.zero, dualAxisPosition);
                        break;
                }
            }

            // Update the interaction data source
            interactionMapping.Vector2Data = dualAxisPosition;
        }

        private void UpdatePoseData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);

            if (interactionMapping.InputType != DeviceInputType.SpatialPointer)
            {
                Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                return;
            }

            currentPointerPose = currentControllerPose;

            // Update the interaction data source
            interactionMapping.PoseData = currentPointerPose;
        }
    }
}
