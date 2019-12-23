// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using UnityEngine;
using XRTK.Definitions.Utilities;
using XRTK.Inspectors.Profiles;
using XRTK.Inspectors.Utilities;
using XRTK.Oculus.Profiles;

namespace XRTK.Oculus.Inspectors
{
    [CustomEditor(typeof(OculusHandControllerDataProviderProfile))]
    public class OculusHandControllerDataProviderProfileInspector : BaseMixedRealityProfileInspector
    {
        private SerializedProperty handTrackingEnabled;
        private SerializedProperty minConfidenceRequired;

        protected override void OnEnable()
        {
            base.OnEnable();

            handTrackingEnabled = serializedObject.FindProperty(nameof(handTrackingEnabled));
            minConfidenceRequired = serializedObject.FindProperty(nameof(minConfidenceRequired));
        }

        public override void OnInspectorGUI()
        {
            MixedRealityInspectorUtility.RenderMixedRealityToolkitLogo();

            if (thisProfile.ParentProfile != null &&
                GUILayout.Button("Back to Configuration Profile"))
            {
                Selection.activeObject = thisProfile.ParentProfile;
            }

            EditorGUILayout.Space();
            thisProfile.CheckProfileLock();

            if (MixedRealityInspectorUtility.CheckProfilePlatform(SupportedPlatforms.Android | SupportedPlatforms.Editor))
            {
                serializedObject.Update();

                EditorGUILayout.BeginVertical("Label");
                EditorGUILayout.PropertyField(handTrackingEnabled);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quality Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(minConfidenceRequired);

                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}