// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Oculus.Controllers;
using XRTK.Definitions.Controllers;
using XRTK.Providers.Controllers;

namespace XRTK.Oculus.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Mappings/Native Oculus Remote Controller Mapping Profile", fileName = "OculusRemoteControllerMappingProfile")]
    public class OculusRemoteControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.OculusRemote;

        /// <inheritdoc />
        public override string TexturePath => $"{base.TexturePath}OculusRemoteController";

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Oculus Remote Controller", typeof(OculusRemoteController)),
                };
            }

            base.Awake();
        }
    }
}