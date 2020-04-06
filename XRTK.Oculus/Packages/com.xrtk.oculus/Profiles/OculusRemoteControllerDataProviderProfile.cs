// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;

namespace XRTK.Oculus.Profiles
{
    public class OculusRemoteControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetControllerDefinitions()
        {
            // new MixedRealityControllerMapping("Oculus Remote Controller", typeof(OculusRemoteController)),
            throw new System.NotImplementedException();
        }
    }
}