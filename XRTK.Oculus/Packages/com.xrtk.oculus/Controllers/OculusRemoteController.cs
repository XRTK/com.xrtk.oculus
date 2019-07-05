// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;

namespace XRTK.Oculus.Controllers
{
    public class OculusRemoteController : BaseOculusController
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="trackingState"></param>
        /// <param name="controllerHandedness"></param>
        /// <param name="inputSource"></param>
        /// <param name="interactions"></param>
        public OculusRemoteController(TrackingState trackingState, Handedness controllerHandedness, IMixedRealityInputSource inputSource = null, MixedRealityInteractionMapping[] interactions = null)
                : base(trackingState, controllerHandedness, inputSource, interactions)
        {
        }

        /// <inheritdoc />
        public override MixedRealityInteractionMapping[] DefaultInteractions => new[]
        {
            new MixedRealityInteractionMapping(0, "Button.DpadUp", AxisType.Digital, DeviceInputType.ButtonPress, "DpadUp"),
            new MixedRealityInteractionMapping(1, "Button.DpadDown", AxisType.Digital, DeviceInputType.ButtonPress, "DpadDown"),
            new MixedRealityInteractionMapping(2, "Button.DpadLeft", AxisType.Digital, DeviceInputType.ButtonPress, "DpadLeft"),
            new MixedRealityInteractionMapping(3, "Button.DpadRight", AxisType.Digital, DeviceInputType.ButtonPress, "DpadRight"),
            new MixedRealityInteractionMapping(4, "Button.One", AxisType.Digital, DeviceInputType.ButtonPress, "One"),
            new MixedRealityInteractionMapping(5, "Button.Two", AxisType.Digital, DeviceInputType.ButtonPress, "Two"),
            new MixedRealityInteractionMapping(6, "Button.Start", AxisType.Digital, DeviceInputType.ButtonPress, "Start"),
            new MixedRealityInteractionMapping(7, "Button.Back", AxisType.Digital, DeviceInputType.ButtonPress, "Back"),
        };

        /// <inheritdoc />
        public override void SetupDefaultInteractions(Handedness controllerHandedness)
        {
            AssignControllerMappings(DefaultInteractions);
        }
    }
}
