// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;

namespace XRTK.Oculus.Profiles
{
    public class OculusTouchControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetControllerDefinitions()
        {
            // new MixedRealityControllerMapping("Oculus Touch Controller Left", typeof(OculusTouchController), Handedness.Left),
            // new MixedRealityControllerMapping("Oculus Touch Controller Right", typeof(OculusTouchController), Handedness.Right),
            throw new System.NotImplementedException();
        }
    }
}