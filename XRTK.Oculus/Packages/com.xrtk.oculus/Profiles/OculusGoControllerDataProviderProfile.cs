// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;

namespace XRTK.Oculus.Profiles
{
    public class OculusGoControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetControllerDefinitions()
        {
            // new MixedRealityControllerMapping("Oculus Go Controller Left", typeof(OculusGoController), Handedness.Left),
            // new MixedRealityControllerMapping("Oculus Go Controller Right", typeof(OculusGoController), Handedness.Right),
            throw new System.NotImplementedException();
        }
    }
}