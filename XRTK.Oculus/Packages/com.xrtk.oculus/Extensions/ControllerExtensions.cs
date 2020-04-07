// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;

namespace XRTK.Oculus.Extensions
{
    public static class ControllerExtensions
    {
        /// <summary>
        /// Converts an <see cref="OculusApi.Controller"/> mask to a XRTK <see cref="SupportedControllerType"/>.
        /// </summary>
        /// <param name="controller">Controller mask.</param>
        /// <returns>Controller type.</returns>
        public static SupportedControllerType ToControllerType(this OculusApi.Controller controller)
        {
            switch (controller)
            {
                case OculusApi.Controller.LTouch:
                case OculusApi.Controller.RTouch:
                case OculusApi.Controller.Touch:
                    return SupportedControllerType.OculusTouch;
                case OculusApi.Controller.Remote:
                    return SupportedControllerType.OculusRemote;
                case OculusApi.Controller.LTrackedRemote:
                case OculusApi.Controller.RTrackedRemote:
                    return SupportedControllerType.OculusGo;
                case OculusApi.Controller.LHand:
                case OculusApi.Controller.RHand:
                case OculusApi.Controller.Hands:
                    return SupportedControllerType.Hand;
            }

            // If we couldn't map it then the provided controller type is not supported
            // and we ignore it.
            return SupportedControllerType.None;
        }

        /// <summary>
        /// Converts an <see cref="OculusApi.Controller"/> mask to a XRTK <see cref="Handedness"/>.
        /// </summary>
        /// <param name="controller">Controller mask.</param>
        /// <returns>Handedness.</returns>
        public static Handedness ToHandedness(this OculusApi.Controller controller)
        {
            switch (controller)
            {
                case OculusApi.Controller.LTrackedRemote:
                case OculusApi.Controller.LTouch:
                    return Handedness.Left;
                case OculusApi.Controller.RTrackedRemote:
                case OculusApi.Controller.RTouch:
                    return Handedness.Right;
                case OculusApi.Controller.Touchpad:
                case OculusApi.Controller.Gamepad:
                case OculusApi.Controller.Remote:
                    return Handedness.Both;
                case OculusApi.Controller.LHand:
                    return Handedness.Left;
                case OculusApi.Controller.RHand:
                    return Handedness.Right;
                case OculusApi.Controller.Hands:
                    return Handedness.Both;
            }

            Debug.LogWarning($"{controller} does not have a defined controller handedness, falling back to {Handedness.None}");
            return Handedness.None;
        }
    }
}