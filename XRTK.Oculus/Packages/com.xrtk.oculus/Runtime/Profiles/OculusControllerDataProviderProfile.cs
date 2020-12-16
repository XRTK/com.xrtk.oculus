// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Providers.Controllers;

namespace XRTK.Oculus.Profiles
{
    public class OculusControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(OculusRemoteController)),
                new ControllerDefinition(typeof(OculusTouchController), Handedness.Left),
                new ControllerDefinition(typeof(OculusTouchController), Handedness.Right)
            };
        }
    }
}