// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using XRTK.Definitions.InputSystem;
using XRTK.Definitions.Utilities;
using XRTK.Oculus.Controllers;
using XRTK.Utilities;

namespace XRTK.Oculus
{
    [InitializeOnLoad]
    public class ValidateOculusControllerMappings
    {
        private static Type[] dataProviderTypes = new[] { typeof(OculusControllerDataProvider) };
        private static ControllerDataProviderConfiguration[] dataProviderConfiguration = new [] {new ControllerDataProviderConfiguration(typeof(OculusControllerDataProvider), "Oculus Controller Data Provider", 2, SupportedPlatforms.WindowsStandalone | SupportedPlatforms.Editor, null) };
        private static Type[] controllerTypes = new[] { typeof(OculusTouchController) };

        /// <summary>
        /// Constructor.
        /// </summary>
        static ValidateOculusControllerMappings()
        {
            if (!Application.isBatchMode)
            {
                EditorApplication.delayCall += CheckControllerMappings;
            }
        }

        public static void CheckControllerMappings()
        {
            bool result = false;
            if (ValidateConfiguration.ValidateDataProviders(dataProviderTypes, dataProviderConfiguration))
            {
                // Test if there is an Native Oculus controller profile registered
                if (ValidateConfiguration.ValidateControllerProfiles(controllerTypes))
                {
                    //if there is profile, also check if there are any compatible mappings.  Left tests both Left and Right
                    result = ValidateConfiguration.ValidateControllerMappings(new[] { typeof(OculusTouchController) }, XRTK.Definitions.Utilities.Handedness.Left);
                }
            }

        }
    }
}