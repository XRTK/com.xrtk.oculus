// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.Providers.Controllers;

namespace XRTK.Oculus.Controllers
{
    public class OculusTouchController : BaseOculusController
    {
        public OculusTouchController() : base() { }

        /// <inheritdoc />
        public OculusTouchController(IMixedRealityControllerDataProvider controllerDataProvider, TrackingState trackingState, Handedness controllerHandedness, MixedRealityControllerMappingProfile controllerMappingProfile, OculusApi.Controller controllerType = OculusApi.Controller.None, OculusApi.Node nodeType = OculusApi.Node.None)
            : base(controllerDataProvider, trackingState, controllerHandedness, controllerMappingProfile, controllerType, nodeType)
        {
        }

        /// <inheritdoc />
        /// <remarks> Note, MUST use RAW button types as that is what the API works with, DO NOT use Virtual!</remarks>
        public override MixedRealityInteractionMapping[] DefaultLeftHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping("Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping("Axis1D.PrimaryIndexTrigger", AxisType.SingleAxis, "LIndexTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis1D.PrimaryIndexTrigger Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerTouch),
            new MixedRealityInteractionMapping("Axis1D.PrimaryIndexTrigger Near Touch", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerNearTouch),
            new MixedRealityInteractionMapping("Axis1D.PrimaryIndexTrigger Press", AxisType.Digital, "LIndexTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping("Axis1D.PrimaryHandTrigger", AxisType.SingleAxis, "LHandTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis1D.PrimaryHandTrigger Press", AxisType.Digital, "LHandTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping("Axis2D.PrimaryThumbstick", AxisType.DualAxis, "LThumbstick", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping("Button.PrimaryThumbstick Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickTouch),
            new MixedRealityInteractionMapping("Button.PrimaryThumbstick Near Touch", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbNearTouch),
            new MixedRealityInteractionMapping("Button.PrimaryThumbstick Press", AxisType.Digital, "LThumbstick", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Button.Three Press", AxisType.Digital, "X", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Four Press", AxisType.Digital, "Y", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Start Press", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Three Touch", AxisType.Digital, "X", DeviceInputType.ButtonTouch),
            new MixedRealityInteractionMapping("Button.Four Touch", AxisType.Digital, "Y", DeviceInputType.ButtonTouch),
            new MixedRealityInteractionMapping("Axis2D.PrimaryThumbRest", AxisType.DualAxis, "LTouchpad", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping("Touch.PrimaryThumbRest Touch", AxisType.Digital, "LThumbRest", DeviceInputType.ThumbTouch),
            new MixedRealityInteractionMapping("Touch.PrimaryThumbRest Near Touch", AxisType.Digital, "LThumbRest", DeviceInputType.ThumbNearTouch)
        };

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultRightHandedInteractions => new[]
        {
            new MixedRealityInteractionMapping("Spatial Pointer", AxisType.SixDof, DeviceInputType.SpatialPointer),
            new MixedRealityInteractionMapping("Axis1D.SecondaryIndexTrigger", AxisType.SingleAxis, "RIndexTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis1D.SecondaryIndexTrigger Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerTouch),
            new MixedRealityInteractionMapping("Axis1D.SecondaryIndexTrigger Near Touch", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerNearTouch),
            new MixedRealityInteractionMapping("Axis1D.SecondaryIndexTrigger Press", AxisType.Digital, "RIndexTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping("Axis1D.SecondaryHandTrigger", AxisType.SingleAxis, "RHandTrigger", DeviceInputType.Trigger),
            new MixedRealityInteractionMapping("Axis1D.SecondaryHandTrigger Press", AxisType.Digital, "RHandTrigger", DeviceInputType.TriggerPress),
            new MixedRealityInteractionMapping("Axis2D.SecondaryThumbstick", AxisType.DualAxis, "RThumbstick", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping("Button.SecondaryThumbstick Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickTouch),
            new MixedRealityInteractionMapping("Button.SecondaryThumbstick Near Touch", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbNearTouch),
            new MixedRealityInteractionMapping("Button.SecondaryThumbstick Press", AxisType.Digital, "RThumbstick", DeviceInputType.ThumbStickPress),
            new MixedRealityInteractionMapping("Button.One Press", AxisType.Digital, "A", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Two Press", AxisType.Digital, "B", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.One Touch", AxisType.Digital, "A", DeviceInputType.ButtonTouch),
            new MixedRealityInteractionMapping("Button.Two Touch", AxisType.Digital, "B", DeviceInputType.ButtonTouch),
            new MixedRealityInteractionMapping("Axis2D.SecondaryThumbRest", AxisType.DualAxis, "RTouchpad", DeviceInputType.ThumbStick),
            new MixedRealityInteractionMapping("Touch.SecondaryThumbRest Touch", AxisType.Digital, "RThumbRest", DeviceInputType.ThumbTouch),
            new MixedRealityInteractionMapping("Touch.SecondaryThumbRest Near Touch", AxisType.Digital, "RThumbRest", DeviceInputType.ThumbNearTouch)
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(controllerHandedness == Handedness.Left ? DefaultLeftHandedInteractions : DefaultRightHandedInteractions);
        }
    }
}