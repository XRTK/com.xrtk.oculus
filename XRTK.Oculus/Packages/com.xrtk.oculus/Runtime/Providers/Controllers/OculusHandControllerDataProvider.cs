// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem;
using XRTK.Oculus.Plugins;
using XRTK.Oculus.Profiles;
using XRTK.Providers.Controllers.Hands;
using XRTK.Services;

namespace XRTK.Oculus.Providers.Controllers
{
    [RuntimePlatform(typeof(OculusPlatform))]
    [System.Runtime.InteropServices.Guid("EA666456-BAEF-4412-A829-A4C7132E98C3")]
    public class OculusHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <inheritdoc />
        public OculusHandControllerDataProvider(string name, uint priority, OculusHandControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
            MinConfidenceRequired = (OculusApi.TrackingConfidence)profile.MinConfidenceRequired;
        }

        private readonly OculusHandDataConverter leftHandConverter = new OculusHandDataConverter(Handedness.Left);
        private readonly OculusHandDataConverter rightHandConverter = new OculusHandDataConverter(Handedness.Right);
        private readonly Dictionary<Handedness, MixedRealityHandController> activeControllers = new Dictionary<Handedness, MixedRealityHandController>();

        private OculusApi.HandState leftHandState = default;
        private OculusApi.HandState rightHandState = default;

        /// <summary>
        /// The minimum required tracking confidence for hands to be registered.
        /// </summary>
        public OculusApi.TrackingConfidence MinConfidenceRequired { get; }

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            OculusHandDataConverter.HandMeshingEnabled = HandMeshingEnabled;
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            var step = OculusApi.Step.Render;

            bool isLeftHandTracked = OculusApi.GetHandState(step, OculusApi.Hand.HandLeft, ref leftHandState) &&
                                     leftHandState.HandConfidence >= MinConfidenceRequired &&
                                     (leftHandState.Status & OculusApi.HandStatus.HandTracked) != 0;

            if (isLeftHandTracked)
            {
                var controller = GetOrAddController(Handedness.Left);
                controller?.UpdateController(leftHandConverter.GetHandData());
            }
            else
            {
                RemoveController(Handedness.Left);
            }

            bool isRightHandTracked = OculusApi.GetHandState(step, OculusApi.Hand.HandRight, ref rightHandState) &&
                                      rightHandState.HandConfidence >= MinConfidenceRequired &&
                                      (rightHandState.Status & OculusApi.HandStatus.HandTracked) != 0;

            if (isRightHandTracked)
            {
                var controller = GetOrAddController(Handedness.Right);
                controller?.UpdateController(rightHandConverter.GetHandData());
            }
            else
            {
                RemoveController(Handedness.Right);
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            foreach (var activeController in activeControllers)
            {
                RemoveController(activeController.Key, false);
            }

            activeControllers.Clear();
        }

        private bool TryGetController(Handedness handedness, out MixedRealityHandController controller)
        {
            if (activeControllers.ContainsKey(handedness))
            {
                var existingController = activeControllers[handedness];
                Debug.Assert(existingController != null, $"Hand Controller {handedness} has been destroyed but remains in the active controller registry.");
                controller = existingController;
                return true;
            }

            controller = null;
            return false;
        }

        private MixedRealityHandController GetOrAddController(Handedness handedness)
        {
            // If a device is already registered with the handedness, just return it.
            if (TryGetController(handedness, out var existingController))
            {
                return existingController;
            }

            MixedRealityHandController detectedController;
            try
            {
                detectedController = new MixedRealityHandController(this, TrackingState.Tracked, handedness, GetControllerMappingProfile(typeof(MixedRealityHandController), handedness));
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {nameof(MixedRealityHandController)}!\n{e}");
                return null;
            }

            detectedController.TryRenderControllerModel();
            AddController(detectedController);
            activeControllers.Add(handedness, detectedController);
            MixedRealityToolkit.InputSystem?.RaiseSourceDetected(detectedController.InputSource, detectedController);

            return detectedController;
        }

        private void RemoveController(Handedness handedness, bool removeFromRegistry = true)
        {
            if (TryGetController(handedness, out var controller))
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);

                if (removeFromRegistry)
                {
                    RemoveController(controller);
                    activeControllers.Remove(handedness);
                }
            }
        }
    }
}