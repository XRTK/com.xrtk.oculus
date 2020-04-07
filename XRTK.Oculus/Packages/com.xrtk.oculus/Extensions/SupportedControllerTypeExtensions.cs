// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using XRTK.Oculus.Controllers;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Oculus.Extensions
{
    public static class SupportedControllerTypeExtensions
    {
        /// <summary>
        /// Gets the concrete imlementation <see cref="Type"/> for a XRTK <see cref="Definitions.Devices.SupportedControllerType"/>
        /// on the Oculus platform.
        /// </summary>
        /// <param name="supportedControllerType">The <see cref="Definitions.Devices.SupportedControllerType"/> to lookup an implementation for.</param>
        /// <returns><see cref="Type"/> of the controller implementation.</returns>
        public static Type ToImplementation(this Definitions.Devices.SupportedControllerType supportedControllerType)
        {
            switch (supportedControllerType)
            {
                case Definitions.Devices.SupportedControllerType.OculusTouch:
                    return typeof(OculusTouchController);
                case Definitions.Devices.SupportedControllerType.OculusGo:
                    return typeof(OculusGoController);
                case Definitions.Devices.SupportedControllerType.OculusRemote:
                    return typeof(OculusRemoteController);
                case Definitions.Devices.SupportedControllerType.Hand:
                    return typeof(MixedRealityHandController);
                case Definitions.Devices.SupportedControllerType.None:
                default:
                    return null;
            }
        }
    }
}