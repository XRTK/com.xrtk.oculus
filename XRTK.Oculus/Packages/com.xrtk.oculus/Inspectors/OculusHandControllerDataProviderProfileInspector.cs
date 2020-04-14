// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Inspectors.Extensions;
using XRTK.Inspectors.Profiles.InputSystem.Controllers;
using XRTK.Oculus.Profiles;

namespace XRTK.Oculus.Inspectors
{
    /// <summary>
    /// Default inspector for <see cref="OculusHandControllerDataProviderProfile"/>.
    /// </summary>
    [CustomEditor(typeof(OculusHandControllerDataProviderProfile))]
    public class OculusHandControllerDataProviderProfileInspector : BaseMixedRealityHandControllerDataProviderProfileInspector
    {
        private SerializedProperty minConfidenceRequired;

        private GUIContent confidenceContent;

        private bool showOculusHandTrackingSettings = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            minConfidenceRequired = serializedObject.FindProperty(nameof(minConfidenceRequired));
            confidenceContent = new GUIContent(minConfidenceRequired.displayName, minConfidenceRequired.tooltip);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            showOculusHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showOculusHandTrackingSettings, new GUIContent("Oculus Hand Tracking Settings"), true);
            if (showOculusHandTrackingSettings)
            {
                EditorGUI.indentLevel++;
                minConfidenceRequired.intValue = (int)(OculusApi.TrackingConfidence)EditorGUILayout.EnumPopup(confidenceContent, (OculusApi.TrackingConfidence)minConfidenceRequired.intValue);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}