// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.Providers.Controllers;
using XRTK.Oculus.Plugins;

namespace XRTK.Oculus.Providers.Controllers
{
    public class OculusRemoteController : BaseOculusController
    {
        public OculusRemoteController() : base() { }

        /// <inheritdoc />
        public OculusRemoteController(IMixedRealityControllerDataProvider controllerDataProvider, TrackingState trackingState, Handedness controllerHandedness, MixedRealityControllerMappingProfile controllerMappingProfile, OculusApi.Controller controllerType = OculusApi.Controller.None, OculusApi.Node nodeType = OculusApi.Node.None)
            : base(controllerDataProvider, trackingState, controllerHandedness, controllerMappingProfile, controllerType, nodeType)
        {
        }

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping("Button.DpadUp", AxisType.Digital, "DpadUp", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.DpadDown", AxisType.Digital, "DpadDown", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.DpadLeft", AxisType.Digital, "DpadLeft", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.DpadRight", AxisType.Digital, "DpadRight", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.One", AxisType.Digital, "One", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Two", AxisType.Digital, "Two", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Start", AxisType.Digital, "Start", DeviceInputType.ButtonPress),
            new MixedRealityInteractionMapping("Button.Back", AxisType.Digital, "Back", DeviceInputType.ButtonPress),
        };
    }
}