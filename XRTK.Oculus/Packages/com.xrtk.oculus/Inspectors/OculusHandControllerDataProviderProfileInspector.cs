// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Definitions.Utilities;
using XRTK.Inspectors.Profiles.InputSystem.Controllers.Hands;
using XRTK.Inspectors.Utilities;
using XRTK.Oculus.Profiles;

namespace XRTK.Oculus.Inspectors
{
    /// <summary>
    /// Default inspector for <see cref="OculusHandControllerDataProviderProfile"/>.
    /// </summary>
    [CustomEditor(typeof(OculusHandControllerDataProviderProfile))]
    public class OculusHandControllerDataProviderProfileInspector : MixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty minConfidenceRequired;

        protected override void OnEnable()
        {
            base.OnEnable();

            minConfidenceRequired = serializedObject.FindProperty(nameof(minConfidenceRequired));
        }

        protected override void OnPlatformInspectorGUI()
        {
            EditorGUILayout.LabelField("Oculus Specific Hand Settings", EditorStyles.boldLabel);
            SupportedPlatforms supportedPlatforms = SupportedPlatforms.Android | SupportedPlatforms.Editor;
            if (MixedRealityInspectorUtility.CheckProfilePlatform(supportedPlatforms,
                $"You can't edit platform specific hand configuration with the current build target. Please switch to {supportedPlatforms}."))
            {
                EditorGUILayout.PropertyField(minConfidenceRequired);
            }
        }
    }
}