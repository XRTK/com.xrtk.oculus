// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Devices;
using XRTK.Definitions.InputSystem;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Interfaces.Providers.Controllers;

namespace XRTK.Oculus.Controllers
{
    public class OculusGoController : BaseOculusController
    {
        /// <inheritdoc />
        public OculusGoController(IMixedRealityControllerDataProvider dataProvider, TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
                : base(dataProvider, trackingState, controllerHandedness, OculusApi.Controller.None, OculusApi.Node.None, inputSource, interactions)
        {
        }

        /// <inheritdoc />
        /// <remarks> Note, MUST use RAW button types as that is what the API works with, DO NOT use Virtual!</remarks>
        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, "LIndexTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping(2, "Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerTouch),
            new MixedRealityInteractionMapping(3, "Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping(4, "Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, "LHandTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger Press", AxisType.Digital, "LHandTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping(6, "Axis2D.PrimaryTouchpad", AxisType.DualAxis, "LTouchpad", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping(7, "Button.PrimaryTouchpad Touch", AxisType.Digital, "LTouchpad", DeviceInputType.ThumbStickTouch),
            new MixedRealityInteractionMapping(8, "Button.PrimaryTouchpad Press", AxisType.Digital, "LTouchpad", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping(9, "Button.Start Press", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(10, "Button.Back Touch", AxisType.Digital, "Back", DeviceInputType.ButtonTouch),
            new MixedRealityInteractionMapping(11, "Button.DpadUp", AxisType.Digital, "DpadUp", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(12, "Button.DpadDown", AxisType.Digital, "DpadDown", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(13, "Button.DpadLeft", AxisType.Digital, "DpadLeft", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(14, "Button.DpadRight", AxisType.Digital, "DpadRight", DeviceInputType.ButtonPress),
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer, MixedRealityInputAction.None),
            new MixedRealityInteractionMapping(1, "Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, "RIndexTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping(2, "Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerTouch),
            new MixedRealityInteractionMapping(3, "Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping(4, "Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, "RHandTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping(5, "Axis1D.PrimaryHandTrigger Press", AxisType.Digital, "RHandTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping(6, "Axis2D.PrimaryTouchpad", AxisType.DualAxis, "RTouchpad", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping(7, "Button.PrimaryTouchpad Touch", AxisType.Digital, "RTouchpad", DeviceInputType.ThumbStickTouch),
            new MixedRealityInteractionMapping(8, "Button.PrimaryTouchpad Press", AxisType.Digital, "RTouchpad", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping(9, "Button.Start Press", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(10, "Button.Back Touch", AxisType.Digital, "Back", DeviceInputType.ButtonTouch),
            new MixedRealityInteractionMapping(11, "Button.DpadUp", AxisType.Digital, "DpadUp", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(12, "Button.DpadDown", AxisType.Digital, "DpadDown", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(13, "Button.DpadLeft", AxisType.Digital, "DpadLeft", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping(14, "Button.DpadRight", AxisType.Digital, "DpadRight", DeviceInputType.ButtonPress),
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(controllerHandedness == Handedness.Left ? DefaultLeftHandedInteractions : DefaultRightHandedInteractions);
        }
    }
}