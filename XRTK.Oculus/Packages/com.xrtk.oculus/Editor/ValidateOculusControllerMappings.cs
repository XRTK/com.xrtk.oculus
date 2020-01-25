// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEditor;
using UnityEngine;
using XRTK.Oculus.Controllers;
using XRTK.Utilities;

namespace XRTK.Oculus
{
    [InitializeOnLoad]
    public class ValidateOculusControllerMappings
    {
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
            // Test if there is an Native Oculus controller profile registered
            if (ValidateConfiguration.ValidateControllerProfiles(controllerTypes))
            {
                //if there is profile, also check if there are any compatible mappings.  Left tests both Left and Right
                result = ValidateConfiguration.ValidateControllerMappings(new[] { typeof(OculusTouchController) }, XRTK.Definitions.Utilities.Handedness.Left);
            }
        }
    }
}