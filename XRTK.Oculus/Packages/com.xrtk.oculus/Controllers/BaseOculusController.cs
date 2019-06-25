// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.InputSystem;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Oculus.Controllers
{
    public class BaseOculusController : BaseController
    {
        public BaseOculusController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Trigger Position", AxisType.SingleAxis, DeviceInputType.Trigger, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(2, "Trigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(3, "Trigger Press (Select)", AxisType.Digital, DeviceInputType.Select, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(4, "Bumper Press", AxisType.Digital, DeviceInputType.ButtonPress, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(5, "Home Press", AxisType.Digital, DeviceInputType.ButtonPress, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(6, "Touchpad Position", AxisType.DualAxis, DeviceInputType.Touchpad, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(7, "Touchpad Press", AxisType.SingleAxis, DeviceInputType.TouchpadPress, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(8, "Touchpad Touch", AxisType.SingleAxis, DeviceInputType.TouchpadTouch, MixedRealityInputAction.None),
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => DefaultInteractions;

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => DefaultInteractions;

        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(DefaultInteractions);
        }

        //internal MLInputController MlControllerReference { get; set; }
        //internal LuminControllerGestureSettings ControllerGestureSettings { get; set; }

        internal bool IsHomePressed = false;

        private MixedRealityPose currentPointerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose lastControllerPose = MixedRealityPose.ZeroIdentity;
        private MixedRealityPose currentControllerPose = MixedRealityPose.ZeroIdentity;
        private Vector2 dualAxisPosition = Vector2.zero;

        /// <summary>
        /// Updates the controller's interaction mappings and ready the current input values.
        /// </summary>
        public void UpdateController()
        {
            if (!Enabled) { return; }

            UpdateControllerData();

            if (Interactions == null)
            {
                Debug.LogError($"No interaction configuration for Windows Mixed Reality Motion Controller {ControllerHandedness}");
                Enabled = false;
            }

            for (int i = 0; i < Interactions?.Length; i++)
            {
                switch (Interactions[i].InputType)
                {
                    case DeviceInputType.SpatialPointer:
                        UpdatePoseData(Interactions[i]);
                        break;
                    case DeviceInputType.ButtonPress:
                        UpdateButtonData(Interactions[i]);
                        break;
                    case DeviceInputType.Select:
                    case DeviceInputType.Trigger:
                    case DeviceInputType.TriggerTouch:
                    case DeviceInputType.TriggerPress:
                    case DeviceInputType.TouchpadTouch:
                    case DeviceInputType.TouchpadPress:
                        UpdateSingleAxisData(Interactions[i]);
                        break;
                    case DeviceInputType.Touchpad:
                        UpdateDualAxisData(Interactions[i]);
                        break;
                    default:
                        Debug.LogError($"Input [{Interactions[i].InputType}] is not handled for this controller [{GetType().Name}]");
                        break;
                }
            }
        }

        private void UpdateControllerData()
        {
            var lastState = TrackingState;

            lastControllerPose = currentControllerPose;

            //if (MlControllerReference.Type == MLInputControllerType.Control)
            //{
            //    // The source is either a hand or a controller that supports pointing.
            //    // We can now check for position and rotation.
            //    IsPositionAvailable = MlControllerReference.Dof != MLInputControllerDof.None;

            //    if (IsPositionAvailable)
            //    {
            //        IsPositionApproximate = MlControllerReference.CalibrationAccuracy <= MLControllerCalibAccuracy.Medium;
            //    }
            //    else
            //    {
            //        IsPositionApproximate = false;
            //    }

            //    IsRotationAvailable = MlControllerReference.Dof == MLInputControllerDof.Dof6;

            //    // Devices are considered tracked if we receive position OR rotation data from the sensors.
            //    TrackingState = (IsPositionAvailable || IsRotationAvailable) ? TrackingState.Tracked : TrackingState.NotTracked;
            //}
            //else
            //{
            //    // The input source does not support tracking.
            //    TrackingState = TrackingState.NotApplicable;
            //}

            //currentControllerPose.Position = MlControllerReference.Position;
            //currentControllerPose.Rotation = MlControllerReference.Orientation;

            //// Raise input system events if it is enabled.
            //if (lastState != TrackingState)
            //{
            //    MixedRealityToolkit.InputSystem?.RaiseSourceTrackingStateChanged(InputSource, this, TrackingState);
            //}

            //if (TrackingState == TrackingState.Tracked && lastControllerPose != currentControllerPose)
            //{
            //    if (IsPositionAvailable && IsRotationAvailable)
            //    {
            //        MixedRealityToolkit.InputSystem?.RaiseSourcePoseChanged(InputSource, this, currentControllerPose);
            //    }
            //    else if (IsPositionAvailable && !IsRotationAvailable)
            //    {
            //        MixedRealityToolkit.InputSystem?.RaiseSourcePositionChanged(InputSource, this, currentControllerPose.Position);
            //    }
            //    else if (!IsPositionAvailable && IsRotationAvailable)
            //    {
            //        MixedRealityToolkit.InputSystem?.RaiseSourceRotationChanged(InputSource, this, currentControllerPose.Rotation);
            //    }
            //}
        }

        private void UpdateButtonData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.Digital);

            var isHomeButton = interactionMapping.Description.Contains("Home");

            // Update the interaction data source
            //if (!isHomeButton)
            //{
            //    interactionMapping.BoolData = MlControllerReference.State.ButtonState[(int)MLInputControllerButton.Bumper] > 0;
            //}
            //else
            //{
            //    interactionMapping.BoolData = IsHomePressed;
            //    IsHomePressed = false;
            //}

            //// If our value changed raise it.
            //if (interactionMapping.Changed)
            //{
            //    // Raise input system Event if it enabled
            //    if (interactionMapping.BoolData)
            //    {
            //        MixedRealityToolkit.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
            //    }
            //    else
            //    {
            //        MixedRealityToolkit.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
            //    }
            //}

            //if (interactionMapping.Updated)
            //{
            //    MixedRealityToolkit.InputSystem?.RaiseOnInputPressed(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
            //}
        }

        private void UpdateSingleAxisData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SingleAxis || interactionMapping.AxisType == AxisType.Digital);

            float singleAxisValue = 0;
                //interactionMapping.Description.Contains("Touchpad")
                //? MlControllerReference.Touch1PosAndForce.z
                //: MlControllerReference.TriggerValue;

            switch (interactionMapping.InputType)
            {
                case DeviceInputType.Select:
                case DeviceInputType.TriggerPress:
                case DeviceInputType.TouchpadPress:
                    // Update the interaction data source
                    interactionMapping.BoolData = singleAxisValue.Equals(1f);
                    break;
                case DeviceInputType.TriggerTouch:
                case DeviceInputType.TouchpadTouch:
                case DeviceInputType.TriggerNearTouch:
                    // Update the interaction data source
                    interactionMapping.BoolData = !singleAxisValue.Equals(0f);
                    break;
                case DeviceInputType.Trigger:
                    // Update the interaction data source
                    interactionMapping.FloatData = singleAxisValue;

                    // If our value changed raise it.
                    if (interactionMapping.Updated)
                    {
                        // Raise input system Event if it enabled
                        MixedRealityToolkit.InputSystem?.RaiseOnInputPressed(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.FloatData);
                    }
                    return;
                default:
                    Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                    return;
            }

            // If our value changed raise it.
            if (interactionMapping.Changed)
            {
                // Raise input system Event if it enabled
                if (interactionMapping.BoolData)
                {
                    MixedRealityToolkit.InputSystem?.RaiseOnInputDown(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
                }
                else
                {
                    MixedRealityToolkit.InputSystem?.RaiseOnInputUp(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction);
                }
            }

            if (interactionMapping.Updated)
            {
                MixedRealityToolkit.InputSystem?.RaiseOnInputPressed(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, singleAxisValue);
            }
        }

        private void UpdateDualAxisData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.DualAxis);

            //if (MlControllerReference.Touch1PosAndForce.z > 0f)
            //{
            //    dualAxisPosition.x = MlControllerReference.Touch1PosAndForce.x;
            //    dualAxisPosition.y = MlControllerReference.Touch1PosAndForce.y;
            //}
            //else
            //{
            //    dualAxisPosition.x = 0f;
            //    dualAxisPosition.y = 0f;
            //}

            // Update the interaction data source
            interactionMapping.Vector2Data = dualAxisPosition;

            // If our value changed raise it.
            if (interactionMapping.Updated)
            {
                // Raise input system Event if it enabled
                MixedRealityToolkit.InputSystem?.RaisePositionInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.Vector2Data);
            }
        }

        private void UpdatePoseData(MixedRealityInteractionMapping interactionMapping)
        {
            Debug.Assert(interactionMapping.AxisType == AxisType.SixDof);

            if (interactionMapping.InputType == DeviceInputType.SpatialPointer)
            {
                //currentPointerPose.Position = MlControllerReference.Position;
                //currentPointerPose.Rotation = MlControllerReference.Orientation;
            }
            else
            {
                Debug.LogError($"Input [{interactionMapping.InputType}] is not handled for this controller [{GetType().Name}]");
                return;
            }

            // Update the interaction data source
            interactionMapping.PoseData = currentPointerPose;

            // If our value changed raise it.
            if (interactionMapping.Updated)
            {
                // Raise input system Event if it enabled 
                MixedRealityToolkit.InputSystem?.RaisePoseInputChanged(InputSource, ControllerHandedness, interactionMapping.MixedRealityInputAction, interactionMapping.PoseData);
            }
        }

    }
}
