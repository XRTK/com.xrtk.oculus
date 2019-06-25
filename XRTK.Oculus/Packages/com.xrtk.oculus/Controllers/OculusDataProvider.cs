// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using XRTK.Definitions.Devices;
using XRTK.Definitions.Utilities;
using XRTK.Interfaces.Providers.Controllers;
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

        private const float DeviceRefreshInterval = 3.0f;

        /// <summary>
        /// Dictionary to capture all active controllers detected
        /// </summary>
        protected static readonly Dictionary<OculusApi.Controller, BaseOculusController> ActiveControllers = new Dictionary<OculusApi.Controller, BaseOculusController>();

        private float deviceRefreshTimer;
        private OculusApi.Controller lastDeviceList;

        private int fixedUpdateCount = 0;

        /// <inheritdoc/>
        public override IMixedRealityController[] GetActiveControllers()
        {
            var list = new List<IMixedRealityController>();

            foreach (var controller in ActiveControllers.Values)
            {
                list.Add(controller);
            }

            return list.ToArray();
        }

        /// <inheritdoc />
        public override void Enable()
        {
            //if (!MLInput.IsStarted)
            //{
            //    var config = new MLInputConfiguration();
            //    var result = MLInput.Start(config);

            //    if (!result.IsOk)
            //    {
            //        Debug.LogError($"Error: failed starting MLInput: {result}");
            //        return;
            //    }
            //}

            //for (byte i = 0; i < 3; i++)
            //{
            //    // Currently no way to know what controllers are already connected.
            //    // Just guessing there could be no more than 3: Two Spatial Controllers and Mobile App Controller.
            //    var controller = GetController(i);

            //    if (controller != null)
            //    {
            //        MixedRealityToolkit.InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
            //    }
            //}

        }

        /// <inheritdoc />
        public override void Update()
        {
            OculusApi.stepType = OculusApi.Step.Render;
            fixedUpdateCount = 0;

            deviceRefreshTimer += Time.unscaledDeltaTime;

            if (deviceRefreshTimer >= DeviceRefreshInterval)
            {
                deviceRefreshTimer = 0.0f;
                RefreshDevices();
            }

            foreach (var controller in ActiveControllers)
            {
                controller.Value?.UpdateController();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            OculusApi.stepType = OculusApi.Step.Physics;

            double predictionSeconds = (double)fixedUpdateCount * Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 1e-6f);
            fixedUpdateCount++;

            OVRPlugin.UpdateNodePhysicsPoses(0, predictionSeconds);
        }

        /// <inheritdoc />
        public override void Disable()
        {

            foreach (var activeController in ActiveControllers)
            {
                var controller = GetOrAddController(activeController.Key, false);

                if (controller != null)
                {
                    MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
                }
            }

            ActiveControllers.Clear();
        }

        private BaseOculusController GetOrAddController(OculusApi.Controller controllerMask, bool addController = true)
        {
            //If a device is already registered with the ID provided, just return it.
            if (ActiveControllers.ContainsKey(controllerMask))
            {
                var controller = ActiveControllers[controllerMask];
                Debug.Assert(controller != null);
                return controller;
            }

            if (!addController) { return null; }

            //var mlController = MLInput.GetController(controllerId);

            //if (mlController == null) { return null; }

            //if (mlController.Type == MLInputControllerType.None) { return null; }
            var currentControllerType = GetCurrentControllerType(controllerMask);
            Type controllerType = null;

            var controllingHand = Handedness.Any;

            //if (mlController.Type == MLInputControllerType.Control)
            //{
            //    switch (mlController.Hand)
            //    {
            //        case MLInput.Hand.Left:
            //            controllingHand = Handedness.Left;
            //            break;
            //        case MLInput.Hand.Right:
            //            controllingHand = Handedness.Right;
            //            break;
            //    }
            //}

            var pointers = RequestPointers(typeof(BaseOculusController), controllingHand);
            var inputSource = MixedRealityToolkit.InputSystem?.RequestNewGenericInputSource($"Oculus Controller {controllingHand}", pointers);
            var detectedController = new BaseOculusController(TrackingState.NotTracked, controllingHand, inputSource);

            if (!detectedController.SetupConfiguration(typeof(BaseOculusController)))
            {
                // Controller failed to be setup correctly.
                // Return null so we don't raise the source detected.
                Debug.LogError($"Failed to Setup {controllerType.Name} controller");
                return null;
            }

            for (int i = 0; i < detectedController.InputSource?.Pointers?.Length; i++)
            {
                detectedController.InputSource.Pointers[i].Controller = detectedController;
            }

            //detectedController.TryRenderControllerModel(controllerType);

            ActiveControllers.Add(controllerMask, detectedController);
            return detectedController;
        }

        private void RefreshDevices()
        {
            // override locally derived active and connected controllers if plugin provides more accurate data
            OculusApi.connectedControllerTypes = (OculusApi.Controller)OVRPlugin.GetConnectedControllers();
            OculusApi.activeControllerType = (OculusApi.Controller)OVRPlugin.GetActiveController();

            Debug.LogError($"Connected =[{OculusApi.connectedControllerTypes}] - Active = [{OculusApi.activeControllerType}]");

            if (OculusApi.connectedControllerTypes == OculusApi.Controller.None) { return; }

            if (ActiveControllers.Count > 0)
            {
                OculusApi.Controller[] activeControllers = null;
                ActiveControllers.Keys.CopyTo(activeControllers, 0);

                if (lastDeviceList != OculusApi.Controller.None && OculusApi.connectedControllerTypes == lastDeviceList)
                {
                    foreach (var activeController in activeControllers)
                    {
                        if ((activeController & OculusApi.connectedControllerTypes) == activeController)
                        {
                            var controller = GetOrAddController(activeController);

                            if (controller != null)
                            {
                                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
                            }

                            ActiveControllers.Remove(activeController);
                        }
                    }
                }
            }

            for (var i = 0; i < OculusApi.Controllers.Length; i++)
            {
                if (OculusApi.ShouldResolveController(OculusApi.Controllers[i].controllerType, OculusApi.connectedControllerTypes))
                {
                    if (!ActiveControllers.ContainsKey(OculusApi.Controllers[i].controllerType))
                    {
                        var controller = GetOrAddController(OculusApi.Controllers[i].controllerType);

                        if (controller != null)
                        {
                            MixedRealityToolkit.InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
                        }
                    }
                }
            }

            lastDeviceList = OculusApi.connectedControllerTypes;
        }


        #region Controller Events

        private void OnControllerConnected(OculusApi.Controller controllerMask)
        {
            var controller = GetOrAddController(controllerMask);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceDetected(controller.InputSource, controller);
                controller.UpdateController();
            }
        }

        private void OnControllerDisconnected(OculusApi.Controller controllerMask)
        {
            var controller = GetOrAddController(controllerMask, false);

            if (controller != null)
            {
                MixedRealityToolkit.InputSystem?.RaiseSourceLost(controller.InputSource, controller);
            }

            ActiveControllers.Remove(controllerMask);
        }

        //private void MlInputOnControllerButtonDown(byte controllerId, MLInputControllerButton button)
        //{
        //    if (activeControllers.TryGetValue(controllerId, out var controller))
        //    {
        //        switch (button)
        //        {
        //            case MLInputControllerButton.HomeTap:
        //                controller.IsHomePressed = true;
        //                break;
        //        }
        //    }
        //}

        #endregion Controller Events

        private SupportedControllerType GetCurrentControllerType(OculusApi.Controller controllerMask)
        {
            switch (controllerMask)
            {

                case OculusApi.Controller.LTouch:
                case OculusApi.Controller.RTouch:
                    Debug.LogError($"{controllerMask} found - assuming Rift?");
                    return SupportedControllerType.OculusTouch;
                case OculusApi.Controller.Touch:
                    Debug.LogError($"{controllerMask} found - assuming Quest?");
                    return SupportedControllerType.OculusQuest; // Guessing
                case OculusApi.Controller.Remote:
                    Debug.LogError($"{controllerMask} found - assuming Remote?");
                    return SupportedControllerType.OculusRemote;
                case OculusApi.Controller.LTrackedRemote:
                case OculusApi.Controller.RTrackedRemote:
                    Debug.LogError($"{controllerMask} found - assuming Go?");
                    return SupportedControllerType.OculusGo;
                default:
                    break;
            }

            Debug.LogWarning($"{controllerMask} does not have a defined controller type, falling back to generic controller type");

            return SupportedControllerType.GenericOpenVR;
        }
    }
}
