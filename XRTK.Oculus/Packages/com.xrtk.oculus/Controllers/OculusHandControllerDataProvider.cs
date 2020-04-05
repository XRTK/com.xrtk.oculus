// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Profiles;
using XRTK.Providers.Controllers.Hands;
using XRTK.Services;

namespace XRTK.Oculus.Controllers
{
    public class OculusHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public OculusHandControllerDataProvider(string name, uint priority, OculusHandControllerDataProviderProfile profile)
            : base(name, priority, profile)
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
        public OculusApi.TrackingConfidence MinConfidenceRequired { get; } = OculusApi.TrackingConfidence.Low;

        /// <inheritdoc />
        public override void Initialize()
        {
            base.Initialize();

            if (MixedRealityToolkit.CameraSystem != null)
            {
                MixedRealityToolkit.CameraSystem.HeadHeight = OculusApi.EyeHeight;
            }

            OculusHandDataConverter.HandMeshingEnabled = HandMeshingEnabled;
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            var step = OculusApi.Step.Render;

            bool isLeftHandTracked =
                OculusApi.GetHandState(step, OculusApi.Hand.HandLeft, ref leftHandState) &&
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

            bool isRightHandTracked =
                OculusApi.GetHandState(step, OculusApi.Hand.HandRight, ref rightHandState) &&
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

            var controllerType = typeof(MixedRealityHandController);
            var pointers = RequestPointers(controllerType, handedness, true);
            var inputSource = MixedRealityToolkit.InputSystem.RequestNewGenericInputSource($"{handedness} Hand Controller", pointers);
            var detectedController = new MixedRealityHandController(TrackingState.Tracked, handedness, inputSource);

            if (!detectedController.SetupConfiguration(controllerType))
            {
                // Controller failed to be setup correctly.
                // Return null so we don't raise the source detected.
                return null;
            }

            for (int i = 0; i < detectedController.InputSource?.Pointers?.Length; i++)
            {
                detectedController.InputSource.Pointers[i].Controller = detectedController;
            }

            detectedController.TryRenderControllerModel(controllerType);

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
                    activeControllers.Remove(handedness);
                }
            }
        }
    }
}