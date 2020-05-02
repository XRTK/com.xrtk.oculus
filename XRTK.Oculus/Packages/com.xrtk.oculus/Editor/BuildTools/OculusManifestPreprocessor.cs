/************************************************************************************

Copyright   :   Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.

Licensed under the Oculus SDK License Version 3.4.1 (the "License");
you may not use the Oculus SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

https://developer.oculus.com/licenses/sdk-3.4.1

Unless required by applicable law or agreed to in writing, the Oculus SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

using System;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace XRTK.Oculus.Editor.Build
{
    public class OculusManifestPreprocessor
    {
        [MenuItem("Mixed Reality Toolkit/Tools/Create Oculus Quest compatible AndroidManifest.xml", false, 100000)]
        public static void GenerateManifestForSubmission()
        {
            var so = ScriptableObject.CreateInstance(typeof(OculusPathFinder));
            var script = MonoScript.FromScriptableObject(so);
            var assetPath = AssetDatabase.GetAssetPath(script);
            var editorDir = Directory.GetParent(assetPath).FullName;
            var srcFile = $"{editorDir}/BuildTools/AndroidManifest.OVRSubmission.xml";

            if (!File.Exists(srcFile))
            {
                Debug.LogError("Cannot find Android manifest template for submission. Please delete the OVR folder and reimport the Oculus Utilities.");
                return;
            }

            var manifestFolder = $"{Application.dataPath}/Plugins/Android";

            if (!Directory.Exists(manifestFolder))
            {
                Directory.CreateDirectory(manifestFolder);
            }

            var dstFile = $"{manifestFolder}/AndroidManifest.xml";

            if (File.Exists(dstFile))
            {
                Debug.LogWarning($"Cannot create Oculus store-compatible manifest due to conflicting file: \"{dstFile}\". Please remove it and try again.");
                return;
            }

            var manifestText = File.ReadAllText(srcFile);
            var dofTextIndex = manifestText.IndexOf("<!-- Request the headset DoF mode -->", StringComparison.Ordinal);

            if (dofTextIndex != -1)
            {
                //Forces Quest configuration.  Needs flip for Go/Gear viewer
                const string headTrackingFeatureText = "<uses-feature android:name=\"oculus.software.handtracking\" android:version=\"1\" android:required=\"true\" />";
                manifestText = manifestText.Insert(dofTextIndex, headTrackingFeatureText);
            }
            else
            {
                Debug.LogWarning("Manifest error: unable to locate headset DoF mode");
            }

            var handTrackingTextIndex = manifestText.IndexOf("<!-- Request the headset handtracking mode -->", StringComparison.Ordinal);

            if (handTrackingTextIndex != -1)
            {
                bool handTrackingEntryNeeded = true; // (targetHandTrackingSupport != OVRProjectConfig.HandTrackingSupport.ControllersOnly);
                bool handTrackingRequired = false; // (targetHandTrackingSupport == OVRProjectConfig.HandTrackingSupport.HandsOnly);

                // TODO add back in?
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (handTrackingEntryNeeded)
                {
                    string handTrackingFeatureText = $"<uses-feature android:name=\"oculus.software.handtracking\" android:required=\"{(handTrackingRequired ? "true" : "false")}\" />";
                    const string handTrackingPermissionText = "<uses-permission android:name=\"com.oculus.permission.HAND_TRACKING\" />";

                    manifestText = manifestText.Insert(handTrackingTextIndex, handTrackingPermissionText);
                    manifestText = manifestText.Insert(handTrackingTextIndex, handTrackingFeatureText);
                }
            }
            else
            {
                Debug.LogWarning("Manifest error: unable to locate headset hand tracking mode");
            }

            System.IO.File.WriteAllText(dstFile, manifestText);
            AssetDatabase.Refresh();
        }

        [MenuItem("Mixed Reality Toolkit/Tools/Remove AndroidManifest.xml", false, 100001)]
        public static void RemoveAndroidManifest()
        {
            AssetDatabase.DeleteAsset("Assets/Plugins/Android/AndroidManifest.xml");
            AssetDatabase.Refresh();
        }
    }
}
