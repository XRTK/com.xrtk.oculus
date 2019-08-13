// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Controllers;
using XRTK.Definitions.Controllers;
using XRTK.Providers.Controllers;

namespace XRTK.Oculus.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Mappings/Native Oculus Go Controller Mapping Profile", fileName = "OculusGoControllerMappingProfile")]
    public class OculusGoControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.OculusGo;

        /// <inheritdoc />
        public override string TexturePath => $"{base.TexturePath}OculusGoController";

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Oculus Go Controller Left", typeof(OculusGoController), Handedness.Left),
                    new MixedRealityControllerMapping("Oculus Go Controller Right", typeof(OculusGoController), Handedness.Right),
                };
            }

            base.Awake();
        }
    }
}