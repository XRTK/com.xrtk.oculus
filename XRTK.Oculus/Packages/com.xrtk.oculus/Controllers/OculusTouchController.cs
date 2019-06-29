// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.InputSystem;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;

namespace XRTK.Oculus.Controllers
{
    public class OculusTouchController : BaseOculusController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackingState"></param>
        /// <param name="controllerHandedness"></param>
        /// <param name="inputSource"></param>
        /// <param name="interactions"></param>
        public OculusTouchController(TrackingState trackingState, Handedness controllerHandedness,
            IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "PrimaryIndexTrigger"),
            new MixedRealityInteractionMapping(2, "Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch, "PrimaryIndexTrigger"),
            new MixedRealityInteractionMapping(3, "Axis1D.PrimaryIndexTrigger Near Touch", AxisType.Digital, DeviceInputType.TriggerNearTouch, "PrimaryIndexTrigger"),
            new MixedRealityInteractionMapping(4, "Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "LIndexTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "PrimaryHandTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger Press", AxisType.Digital, DeviceInputType.Trigger, "LHandTrigger"),
            new MixedRealityInteractionMapping(6, "Axis2D.PrimaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, "PrimaryThumbstick"),
            new MixedRealityInteractionMapping(7, "Button.PrimaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch, "PrimaryThumbstick"),
            new MixedRealityInteractionMapping(8, "Button.PrimaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "PrimaryThumbButtons"),
            new MixedRealityInteractionMapping(9, "Button.PrimaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress, "LThumbstick"),
            new MixedRealityInteractionMapping(10, "Button.Three Press", AxisType.Digital, DeviceInputType.ButtonPress, "X"),
            new MixedRealityInteractionMapping(11, "Button.Four Press", AxisType.Digital, DeviceInputType.ButtonPress, "Y"),
            new MixedRealityInteractionMapping(12, "Button.Start Press", AxisType.Digital, DeviceInputType.ButtonPress, "Start"),
            new MixedRealityInteractionMapping(13, "Button.Three Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "Three"),
            new MixedRealityInteractionMapping(14, "Button.Four Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "Four"),
            new MixedRealityInteractionMapping(15, "Touch.PrimaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch, "PrimaryThumbRest"),
            new MixedRealityInteractionMapping(16, "Touch.PrimaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "PrimaryThumbButtons")
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Axis1D.SecondaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "SecondaryIndexTrigger"),
            new MixedRealityInteractionMapping(2, "Axis1D.SecondaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch, "SecondaryIndexTrigger"),
            new MixedRealityInteractionMapping(3, "Axis1D.SecondaryIndexTrigger Near Touch", AxisType.Digital, DeviceInputType.TriggerNearTouch, "SecondaryIndexTrigger"),
            new MixedRealityInteractionMapping(4, "Axis1D.SecondaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "RIndexTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.SecondaryHandTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "SecondaryHandTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.SecondaryHandTrigger Press", AxisType.Digital, DeviceInputType.Trigger, "RHandTrigger"),
            new MixedRealityInteractionMapping(6, "Axis2D.SecondaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, "SecondaryThumbstick"),
            new MixedRealityInteractionMapping(7, "Button.SecondaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch, "SecondaryThumbstick"),
            new MixedRealityInteractionMapping(8, "Button.SecondaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "SecondaryThumbButtons"),
            new MixedRealityInteractionMapping(9, "Button.SecondaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress, "RThumbstick"),
            new MixedRealityInteractionMapping(10, "Button.One Press", AxisType.Digital, DeviceInputType.ButtonPress, "A"),
            new MixedRealityInteractionMapping(11, "Button.Two Press", AxisType.Digital, DeviceInputType.ButtonPress, "B"),
            new MixedRealityInteractionMapping(12, "Button.One Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "One"),
            new MixedRealityInteractionMapping(13, "Button.Two Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "Two"),
            new MixedRealityInteractionMapping(14, "Touch.SecondaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch, "SecondaryThumbRest"),
            new MixedRealityInteractionMapping(15, "Touch.SecondaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "SecondaryThumbButtons")
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(controllerHandedness == Handedness.Left ? DefaultLeftHandedInteractions : DefaultRightHandedInteractions);
        }
    }
}