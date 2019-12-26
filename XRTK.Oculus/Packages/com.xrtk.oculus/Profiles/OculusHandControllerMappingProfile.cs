// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Controllers;
using XRTK.Providers.Controllers;

namespace XRTK.Oculus.Profiles
{
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Mappings/Oculus Hand Controller Mapping Profile", fileName = "OculusHandControllerMappingProfile")]
    public class OculusHandControllerMappingProfile : BaseMixedRealityControllerMappingProfile
    {
        /// <inheritdoc />
        public override SupportedControllerType ControllerType => SupportedControllerType.Hand;

        /// <inheritdoc />
        public override string TexturePath => $"{base.TexturePath}Hand";

        protected override void Awake()
        {
            if (!HasSetupDefaults)
            {
                ControllerMappings = new[]
                {
                    new MixedRealityControllerMapping("Oculus Hand Controller Left", typeof(OculusHandController), Handedness.Left),
                    new MixedRealityControllerMapping("Oculus Hand Controller Left", typeof(OculusHandController), Handedness.Right)
                };
            }

            base.Awake();
        }
    }
}