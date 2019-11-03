// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Controllers;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Providers.Controllers;
using XRTK.Services;

namespace XRTK.Oculus.Controllers
{
    public class OculusControllerDataProvider : BaseControllerDataProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="priority"></param>
        /// <param name="profile"></param>
        public OculusControllerDataProvider(string name, uint priority, BaseMixedRealityControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var eyeHeight = OculusApi.EyeHeight;
            if (MixedRealityToolkit.CameraSystem != null)
            {
                MixedRealityToolkit.CameraSystem.HeadHeight = eyeHeight;
            }
        }

        private const float DeviceRefreshInterval = 3.0f;

        /// <summary>
        /// Dictionary to capture all active controllers detected
        /// </summary>
        private readonly Dictionary<OculusApi.Controller, BaseOculusController> activeControllers = new Dictionary<OculusApi.Controller, BaseOculusController>();

        private float deviceRefreshTimer;
        private OculusApi.Controller lastDeviceList;

        private int fixedUpdateCount = 0;

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

            var currentControllerType = GetCurrentControllerType(controllerMask);
            Type controllerType = null;

            switch (currentControllerType)
            {
                case SupportedControllerType.OculusTouch:
                    controllerType = typeof(OculusTouchController);
                    break;
                case SupportedControllerType.OculusGo:
                    controllerType = typeof(OculusGoController);
                    break;
                case SupportedControllerType.OculusRemote:
                    controllerType = typeof(OculusRemoteController);
                    break;
            }

            var controllingHand = Handedness.Any;

            //Determine Handedness of the current controller
            switch (controllerMask)
            {
                case OculusApi.Controller.LTrackedRemote:
                case OculusApi.Controller.LTouch:
                    controllingHand = Handedness.Left;
                    break;
                case OculusApi.Controller.RTrackedRemote:
                case OculusApi.Controller.RTouch:
                    controllingHand = Handedness.Right;
                    break;
                case OculusApi.Controller.Touchpad:
                case OculusApi.Controller.Gamepad:
                case OculusApi.Controller.Remote:
                    controllingHand = Handedness.Both;
                    break;
            }

            var nodeType = OculusApi.Node.None;

            switch (controllingHand)
            {
                case Handedness.Left:
                    nodeType = OculusApi.Node.HandLeft;
                    break;
                case Handedness.Right:
                    nodeType = OculusApi.Node.HandRight;
                    break;
            }

            var pointers = RequestPointers(typeof(BaseOculusController), controllingHand);
            var inputSource = MixedRealityToolkit.InputSystem?.RequestNewGenericInputSource($"Oculus Controller {controllingHand}", pointers);
            var detectedController = new BaseOculusController(TrackingState.NotTracked, controllingHand, controllerMask, nodeType, inputSource);

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

        private SupportedControllerType GetCurrentControllerType(OculusApi.Controller controllerMask)
        {
            switch (controllerMask)
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
            }

            Debug.LogWarning($"{controllerMask} does not have a defined controller type, falling back to generic controller type");

            return SupportedControllerType.GenericOpenVR;
        }
    }
}