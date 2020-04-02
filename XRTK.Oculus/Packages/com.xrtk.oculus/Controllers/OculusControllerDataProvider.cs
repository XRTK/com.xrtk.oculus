// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.InputSystem.Controllers.Hands;
using XRTK.Oculus.Extensions;
using XRTK.Oculus.Profiles;
using XRTK.Providers.Controllers;
using XRTK.Providers.Controllers.Hands;
using XRTK.Services;

namespace XRTK.Oculus.Controllers
{
    public class OculusControllerDataProvider : BaseControllerDataProvider, IMixedRealityHandControllerDataProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public OculusControllerDataProvider(string name, uint priority, OculusControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
            HandPhysicsEnabled = profile.HandPhysicsEnabled;
            UseTriggers = profile.UseTriggers;
            BoundsMode = profile.BoundsMode;
            HandMeshingEnabled = profile.HandMeshingEnabled;
            MinConfidenceRequired = (OculusApi.TrackingConfidence)profile.MinConfidenceRequired;
        }

        private const float DeviceRefreshInterval = 3.0f;

        private readonly Dictionary<OculusApi.Controller, BaseController> activeControllers = new Dictionary<OculusApi.Controller, BaseController>();
        private readonly OculusHandDataConverter leftHandConverter = new OculusHandDataConverter(Handedness.Left);
        private readonly OculusHandDataConverter rightHandConverter = new OculusHandDataConverter(Handedness.Right);

        private int fixedUpdateCount = 0;
        private float deviceRefreshTimer;
        private OculusApi.Controller lastDeviceList;

        /// <inheritdoc />
        public bool HandPhysicsEnabled { get; }

        /// <inheritdoc />
        public bool UseTriggers { get; }

        /// <inheritdoc />
        public HandBoundsMode BoundsMode { get; }

        /// <inheritdoc />
        public bool HandMeshingEnabled { get; }

        /// <summary>
        /// The minimum required tracking confidence for hands to be registered.
        /// </summary>
        public OculusApi.TrackingConfidence MinConfidenceRequired { get; }

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

            OculusApi.stepType = OculusApi.Step.Render;
            fixedUpdateCount = 0;

            deviceRefreshTimer += Time.unscaledDeltaTime;

            if (deviceRefreshTimer >= DeviceRefreshInterval)
            {
                deviceRefreshTimer = 0.0f;
                RefreshDevices();
            }

            foreach (var controller in activeControllers)
            {
                if (controller.Value is MixedRealityHandController handController)
                {
                    handController.UpdateController(handController.ControllerHandedness == Handedness.Left
                        ? leftHandConverter.GetHandData()
                        : rightHandConverter.GetHandData());
                }
                else
                {
                    controller.Value?.UpdateController();
                }
            }
        }

        /// <inheritdoc />
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            OculusApi.stepType = OculusApi.Step.Physics;

            double predictionSeconds = (double)fixedUpdateCount * Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 1e-6f);
            fixedUpdateCount++;

            OculusApi.UpdateNodePhysicsPoses(0, predictionSeconds);
        }

        /// <inheritdoc />
        public override void Disable()
        {
            foreach (var activeController in activeControllers)
            {
                RaiseSourceLost(activeController.Key, false);
            }

            activeControllers.Clear();
        }

        private BaseController GetOrAddController(OculusApi.Controller controllerMask, bool addController = true)
        {
            //If a device is already registered with the ID provided, just return it.
            if (activeControllers.ContainsKey(controllerMask))
            {
                var controller = activeControllers[controllerMask];
                Debug.Assert(controller != null);
                return controller;
            }

            if (!addController) { return null; }

            var controllerType = controllerMask.ToControllerType().ToImplementation();
            if (controllerType == null)
            {
                // If we could not map the controller to a type supported by this 
                // XRTK platform it's ignored. This is intended behaviour.
                return null;
            }

            var controllingHand = controllerMask.ToHandedness();
            var nodeType = controllingHand.ToNode();
            var pointers = RequestPointers(typeof(BaseOculusController), controllingHand);
            var inputSource = MixedRealityToolkit.InputSystem?.RequestNewGenericInputSource($"Oculus Controller {controllingHand}", pointers);

            BaseController detectedController;
            if (Equals(controllerType, typeof(MixedRealityHandController)))
            {
                detectedController = new MixedRealityHandController(TrackingState.Tracked, controllingHand, inputSource, null);
            }
            else
            {
                detectedController = new BaseOculusController(TrackingState.NotTracked, controllingHand, controllerMask, nodeType, inputSource);
            }

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

            activeControllers.Add(controllerMask, detectedController);
            AddController(detectedController);
            return detectedController;
        }

        private bool RefreshHands()
        {
            OculusApi.HandState leftHandState = default;
            bool isLeftHandTracked = leftHandState.HandConfidence == MinConfidenceRequired &&
                                     (leftHandState.Status & OculusApi.HandStatus.HandTracked) != 0 &&
                                     OculusApi.GetHandState(OculusApi.stepType, OculusApi.Hand.HandLeft, ref leftHandState);

            if (isLeftHandTracked)
            {
                if (!activeControllers.ContainsKey(OculusApi.Controller.LHand))
                {
                    RaiseSourceDetected(OculusApi.Controller.LHand);
                }
            }
            else
            {
                RaiseSourceLost(OculusApi.Controller.LHand);
            }

            OculusApi.HandState rightHandState = default;
            bool isRightHandTracked = rightHandState.HandConfidence == MinConfidenceRequired &&
                                      (rightHandState.Status & OculusApi.HandStatus.HandTracked) != 0 &&
                                      OculusApi.GetHandState(OculusApi.stepType, OculusApi.Hand.HandRight, ref rightHandState);

            if (isRightHandTracked)
            {
                if (!activeControllers.ContainsKey(OculusApi.Controller.RHand))
                {
                    RaiseSourceDetected(OculusApi.Controller.RHand);
                }
            }
            else
            {
                RaiseSourceLost(OculusApi.Controller.RHand);
            }

            return isLeftHandTracked || isRightHandTracked;
        }

        private void RefreshDevices()
        {
            if (RefreshHands())
            {
                // TODO: Revisit this for proper integration where hands and other controller types are
                // treated the same. For simplicity we skip any other controller monitoring while the Oculus API
                // reports hands to be active.
                return;
            }

            // override locally derived active and connected controllers if plugin provides more accurate data
            OculusApi.connectedControllerTypes = OculusApi.GetConnectedControllers();
            OculusApi.activeControllerType = OculusApi.GetActiveController();

            if (OculusApi.connectedControllerTypes == OculusApi.Controller.None) { return; }

            if (activeControllers.Count > 0)
            {
                var controllers = new OculusApi.Controller[activeControllers.Count];
                activeControllers.Keys.CopyTo(controllers, 0);

                if (lastDeviceList != OculusApi.Controller.None && OculusApi.connectedControllerTypes != lastDeviceList)
                {
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        var activeController = controllers[i];

                        switch (activeController)
                        {
                            case OculusApi.Controller.Touch
                                when ((OculusApi.Controller.LTouch & OculusApi.connectedControllerTypes) != OculusApi.Controller.LTouch):
                                RaiseSourceLost(OculusApi.Controller.LTouch);
                                break;
                            case OculusApi.Controller.Touch
                                when ((OculusApi.Controller.RTouch & OculusApi.connectedControllerTypes) != OculusApi.Controller.RTouch):
                                RaiseSourceLost(OculusApi.Controller.RTouch);
                                break;
                            default:
                                if ((activeController & OculusApi.connectedControllerTypes) != activeController)
                                {
                                    RaiseSourceLost(activeController);
                                }

                                break;
                        }
                    }
                }
            }

            for (int i = 0; i < OculusApi.Controllers.Length; i++)
            {
                if (OculusApi.ShouldResolveController(OculusApi.Controllers[i].controllerType, OculusApi.connectedControllerTypes))
                {
                    if (OculusApi.Controllers[i].controllerType == OculusApi.Controller.Touch)
                    {
                        if (!activeControllers.ContainsKey(OculusApi.Controller.LTouch))
                        {
                            RaiseSourceDetected(OculusApi.Controller.LTouch);
                        }

                        if (!activeControllers.ContainsKey(OculusApi.Controller.RTouch))
                        {
                            RaiseSourceDetected(OculusApi.Controller.RTouch);
                        }
                    }
                    else if (!activeControllers.ContainsKey(OculusApi.Controllers[i].controllerType))
                    {
                        RaiseSourceDetected(OculusApi.Controllers[i].controllerType);
                    }
                }
            }

            lastDeviceList = OculusApi.connectedControllerTypes;
        }

        private void RaiseSourceDetected(OculusApi.Controller controllerType)
        {
            var controller = GetOrAddController(controllerType);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
            }
        }

        private void RaiseSourceLost(OculusApi.Controller activeController, bool clearFromRegistry = true)
        {
            var controller = GetOrAddController(activeController, false);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
                RemoveController(controller);
            }

            if (clearFromRegistry)
            {
                activeControllers.Remove(activeController);
            }
        }
    }
}