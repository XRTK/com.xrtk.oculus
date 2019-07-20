// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Devices;
using XRTK.Definitions.InputSystem;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;

namespace XRTK.Oculus.Controllers
{
    public class OculusGoController : BaseOculusController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackingState"></param>
        /// <param name="controllerHandedness"></param>
        /// <param name="inputSource"></param>
        /// <param name="interactions"></param>
        public OculusGoController(TrackingState trackingState, SupportedControllerType controllerType, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
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
            new MixedRealityInteractionMapping(3, "Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "LIndexTrigger"),
            new MixedRealityInteractionMapping(4, "Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "LHandTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "LHandTrigger"),
            new MixedRealityInteractionMapping(6, "Axis2D.PrimaryTouchpad", AxisType.DualAxis, DeviceInputType.ThumbStick, "LTouchpad"),
            new MixedRealityInteractionMapping(7, "Button.PrimaryTouchpad Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch, "LTouchpad"),
            new MixedRealityInteractionMapping(8, "Button.PrimaryTouchpad Press", AxisType.Digital, DeviceInputType.ThumbStickPress, "LTouchpad"),
            new MixedRealityInteractionMapping(9, "Button.Start Press", AxisType.Digital, DeviceInputType.ButtonPress, "Start"),
            new MixedRealityInteractionMapping(10, "Button.Back Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "Back"),
            new MixedRealityInteractionMapping(11, "Button.DpadUp", AxisType.Digital, DeviceInputType.ButtonPress, "DpadUp"),
            new MixedRealityInteractionMapping(12, "Button.DpadDown", AxisType.Digital, DeviceInputType.ButtonPress, "DpadDown"),
            new MixedRealityInteractionMapping(13, "Button.DpadLeft", AxisType.Digital, DeviceInputType.ButtonPress, "DpadLeft"),
            new MixedRealityInteractionMapping(14, "Button.DpadRight", AxisType.Digital, DeviceInputType.ButtonPress, "DpadRight"),
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "RIndexTrigger"),
            new MixedRealityInteractionMapping(2, "Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, DeviceInputType.TriggerTouch, "RIndexTrigger"),
            new MixedRealityInteractionMapping(3, "Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "RIndexTrigger"),
            new MixedRealityInteractionMapping(4, "Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, DeviceInputType.Trigger, "RHandTrigger"),
            new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger Press", AxisType.Digital, DeviceInputType.TriggerPress, "RHandTrigger"),
            new MixedRealityInteractionMapping(6, "Axis2D.PrimaryTouchpad", AxisType.DualAxis, DeviceInputType.ThumbStick, "RTouchpad"),
            new MixedRealityInteractionMapping(7, "Button.PrimaryTouchpad Touch", AxisType.Digital, DeviceInputType.ThumbStickTouch, "RTouchpad"),
            new MixedRealityInteractionMapping(8, "Button.PrimaryTouchpad Press", AxisType.Digital, DeviceInputType.ThumbStickPress, "RTouchpad"),
            new MixedRealityInteractionMapping(9, "Button.Start Press", AxisType.Digital, DeviceInputType.ButtonPress, "Start"),
            new MixedRealityInteractionMapping(10, "Button.Back Touch", AxisType.Digital, DeviceInputType.ButtonTouch, "Back"),
            new MixedRealityInteractionMapping(11, "Button.DpadUp", AxisType.Digital, DeviceInputType.ButtonPress, "DpadUp"),
            new MixedRealityInteractionMapping(12, "Button.DpadDown", AxisType.Digital, DeviceInputType.ButtonPress, "DpadDown"),
            new MixedRealityInteractionMapping(13, "Button.DpadLeft", AxisType.Digital, DeviceInputType.ButtonPress, "DpadLeft"),
            new MixedRealityInteractionMapping(14, "Button.DpadRight", AxisType.Digital, DeviceInputType.ButtonPress, "DpadRight"),
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(controllerHandedness == Handedness.Left ? DefaultLeftHandedInteractions : DefaultRightHandedInteractions);
        }
    }
}