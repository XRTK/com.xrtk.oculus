// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Interfaces.InputSystem;
using XRTK.Oculus.Extensions;
using XRTK.Oculus.Plugins;
using XRTK.Oculus.Profiles;
using XRTK.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Oculus.Providers.InputSystem.Controllers
{
    public class OculusControllerDataProvider : BaseControllerDataProvider
    {
        /// <inheritdoc />
        public OculusControllerDataProvider(string name, uint priority, OculusControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

        private const float DEVICE_REFRESH_INTERVAL = 3.0f;

        /// <summary>
        /// Dictionary to capture all active controllers detected
        /// </summary>
        private readonly Dictionary<OculusApi.Controller, BaseOculusController> activeControllers = new Dictionary<OculusApi.Controller, BaseOculusController>();

        private int fixedUpdateCount = 0;
        private float deviceRefreshTimer;
        private OculusApi.Controller lastDeviceList;

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            OculusApi.stepType = OculusApi.Step.Render;
            fixedUpdateCount = 0;

            deviceRefreshTimer += Time.unscaledDeltaTime;

            if (deviceRefreshTimer >= DEVICE_REFRESH_INTERVAL)
            {
                deviceRefreshTimer = 0.0f;
                RefreshDevices();
            }

            foreach (var controller in activeControllers)
            {
                controller.Value?.UpdateController();
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

        private BaseOculusController GetOrAddController(OculusApi.Controller controllerMask, bool addController = true)
        {
            //If a device is already registered with the ID provided, just return it.
            if (activeControllers.ContainsKey(controllerMask))
            {
                var controller = activeControllers[controllerMask];
                Debug.Assert(controller != null);
                return controller;
            }

            if (!addController) { return null; }

            var controllerType = GetCurrentControllerType(controllerMask);

            var handedness = controllerMask.ToHandedness();
            var nodeType = handedness.ToNode();

            BaseOculusController detectedController;

            try
            {
                detectedController = Activator.CreateInstance(controllerType, this, TrackingState.NotTracked, handedness, GetControllerMappingProfile(controllerType, handedness), controllerMask, nodeType) as BaseOculusController;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create {controllerType.Name} controller!\n{e}");
                return null;
            }

            if (detectedController == null)
            {
                Debug.LogError($"Failed to create {controllerType.Name} controller!");
                return null;
            }

            detectedController.TryRenderControllerModel();

            activeControllers.Add(controllerMask, detectedController);
            AddController(detectedController);
            return detectedController;
        }

        /// <remarks>
        /// Noticed that the "active" controllers also mark the Tracked state.
        /// </remarks>
        private void RefreshDevices()
        {
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
                    for (var i = 0; i < controllers.Length; i++)
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

            for (var i = 0; i < OculusApi.Controllers.Length; i++)
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

        private static Type GetCurrentControllerType(OculusApi.Controller controllerMask)
        {
            switch (controllerMask)
            {
                case OculusApi.Controller.LTouch:
                case OculusApi.Controller.RTouch:
                case OculusApi.Controller.Touch:
                    return typeof(OculusTouchController);
                case OculusApi.Controller.Remote:
                    return typeof(OculusRemoteController);
                case OculusApi.Controller.LTrackedRemote:
                case OculusApi.Controller.RTrackedRemote:
                    return typeof(OculusGoController);
                default:
                    return null;
            }
        }
    }
}