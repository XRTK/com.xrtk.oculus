// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using UnityEditor;
using UnityEngine;
using XRTK.Definitions.Platforms;
using XRTK.Inspectors.Profiles.InputSystem;
using XRTK.Inspectors.Utilities;
using XRTK.Oculus.Profiles;
using XRTK.Services;

namespace XRTK.Oculus.Inspectors
{
    /// <summary>
    /// Default inspector for <see cref="OculusHandControllerDataProviderProfile"/>.
    /// </summary>
    [CustomEditor(typeof(OculusHandControllerDataProviderProfile))]
    public class OculusHandControllerDataProviderProfileInspector : BaseMixedRealityHandDataProviderProfileInspector
    {
        private SerializedProperty minConfidenceRequired;

        protected override void OnEnable()
        {
            base.OnEnable();

            minConfidenceRequired = serializedObject.FindProperty(nameof(minConfidenceRequired));
        }

        public override void OnInspectorGUI()
        {
            MixedRealityInspectorUtility.RenderMixedRealityToolkitLogo();

            if (ThisProfile.ParentProfile != null &&
                GUILayout.Button("Back To Configuration Profile"))
            {
                Selection.activeObject = ThisProfile.ParentProfile;
            }

            ThisProfile.CheckProfileLock();

            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.Space();

            if (MixedRealityToolkit.AvailablePlatforms.Any(platform => platform is EditorPlatform || platform is AndroidPlatform))
            {
                EditorGUILayout.PropertyField(minConfidenceRequired);
            }

            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
    }
}