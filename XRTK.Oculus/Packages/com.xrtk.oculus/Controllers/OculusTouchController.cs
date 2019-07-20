// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        public OculusTouchController(TrackingState trackingState, SupportedControllerType controllerType, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
            : base(trackingState, controllerType, controllerHandedness, inputSource, interactions)
        {
        }

        /// <inheritdoc />
        /// <remarks> Note, MUST use RAW button types as that is what the API works with, DO NOT use Virtual!</remarks>
        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "LIndexTrigger"),
            new MixedRealityInteractionMapping(2, "Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch, "LIndexTrigger"),
            new MixedRealityInteractionMapping(3, "Axis1D.PrimaryIndexTrigger Near Touch", AxisType.Digital, DeviceInputType.TriggerNearTouch, "LIndexTrigger"),
            new MixedRealityInteractionMapping(4, "Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "LIndexTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "LHandTrigger"),
            new MixedRealityInteractionMapping(6, "Axis1D.PrimaryHandTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "LHandTrigger"),
            new MixedRealityInteractionMapping(7, "Axis2D.PrimaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, "LThumbstick"),
            new MixedRealityInteractionMapping(8, "Button.PrimaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch, "LThumbstick"),
            new MixedRealityInteractionMapping(9, "Button.PrimaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "LThumbstick"),
            new MixedRealityInteractionMapping(10, "Button.PrimaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress, "LThumbstick"),
            new MixedRealityInteractionMapping(11, "Button.Three Press", AxisType.Digital, DeviceInputType.ButtonPress, "X"),
            new MixedRealityInteractionMapping(12, "Button.Four Press", AxisType.Digital, DeviceInputType.ButtonPress, "Y"),
            new MixedRealityInteractionMapping(13, "Button.Start Press", AxisType.Digital, DeviceInputType.ButtonPress, "Start"),
            new MixedRealityInteractionMapping(14, "Button.Three Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "X"),
            new MixedRealityInteractionMapping(15, "Button.Four Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "Y"),
            new MixedRealityInteractionMapping(16, "Axis2D.PrimaryThumbRest", AxisType.DualAxis, DeviceInputType.ThumbStick, "LTouchpad"),
            new MixedRealityInteractionMapping(17, "Touch.PrimaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch, "LThumbRest"),
            new MixedRealityInteractionMapping(18, "Touch.PrimaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "LThumbRest")
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Axis1D.SecondaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "RIndexTrigger"),
            new MixedRealityInteractionMapping(2, "Axis1D.SecondaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch, "RIndexTrigger"),
            new MixedRealityInteractionMapping(3, "Axis1D.SecondaryIndexTrigger Near Touch", AxisType.Digital, DeviceInputType.TriggerNearTouch, "RIndexTrigger"),
            new MixedRealityInteractionMapping(4, "Axis1D.SecondaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "RIndexTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.SecondaryHandTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "RHandTrigger"),
            new MixedRealityInteractionMapping(6, "Axis1D.SecondaryHandTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "RHandTrigger"),
            new MixedRealityInteractionMapping(7, "Axis2D.SecondaryThumbstick", AxisType.DualAxis, DeviceInputType.ThumbStick, "RThumbstick"),
            new MixedRealityInteractionMapping(8, "Button.SecondaryThumbstick Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch, "RThumbstick"),
            new MixedRealityInteractionMapping(9, "Button.SecondaryThumbstick Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "RThumbstick"),
            new MixedRealityInteractionMapping(10, "Button.SecondaryThumbstick Press", AxisType.Digital, DeviceInputType.ThumbStickPress, "RThumbstick"),
            new MixedRealityInteractionMapping(11, "Button.One Press", AxisType.Digital, DeviceInputType.ButtonPress, "A"),
            new MixedRealityInteractionMapping(12, "Button.Two Press", AxisType.Digital, DeviceInputType.ButtonPress, "B"),
            new MixedRealityInteractionMapping(13, "Button.One Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "A"),
            new MixedRealityInteractionMapping(14, "Button.Two Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "B"),
            new MixedRealityInteractionMapping(15, "Axis2D.SecondaryThumbRest", AxisType.DualAxis, DeviceInputType.ThumbStick, "RTouchpad"),
            new MixedRealityInteractionMapping(16, "Touch.SecondaryThumbRest Touch", AxisType.Digital, DeviceInputType.ThumbTouch, "RThumbRest"),
            new MixedRealityInteractionMapping(17, "Touch.SecondaryThumbRest Near Touch", AxisType.Digital, DeviceInputType.ThumbNearTouch, "RThumbRest")
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(controllerHandedness == Handedness.Left ? DefaultLeftHandedInteractions : DefaultRightHandedInteractions);
        }
    }
}