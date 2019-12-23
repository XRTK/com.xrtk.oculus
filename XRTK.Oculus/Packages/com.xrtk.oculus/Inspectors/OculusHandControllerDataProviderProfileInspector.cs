﻿// Copyright (c) XRTK. All rights reserved.
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

        protected override void OnEnable()
        {
            base.OnEnable();

            handTrackingEnabled = serializedObject.FindProperty("handTrackingEnabled");
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

                EditorGUILayout.EndVertical();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}