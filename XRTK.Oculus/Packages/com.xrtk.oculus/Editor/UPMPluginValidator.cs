
// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.XR;
using XRTK.Utilities.Async;

namespace XRTK.Oculus.Editor
{
    [InitializeOnLoad]
    public class UPMPluginValidator
    {
        /// <summary>
        /// Is the package utility running a check?
        /// </summary>
        public static bool IsRunningCheck { get; private set; }

        private static readonly string autoRemoveOculusPackages = "XRTK_OculusUPMRemoveAuto";
        private static bool autoRemoveOculusPackagesEnabled
        {
            get { return PlayerPrefs.GetInt(autoRemoveOculusPackages, 1) == 1; }
            set { PlayerPrefs.SetInt(autoRemoveOculusPackages, value ? 1 : 0); }
        }

        static UPMPluginValidator()
        {
            PlayerPrefs.SetInt(autoRemoveOculusPackages,1);
            //Removing Oculus packages as they use a lower version of the Oculus API
           CheckPackageManifestAndRemoveOculus();
        }

        /// <summary>
        /// Check the projects manifest and determine if any Oculus packages are installed.  If found as user if they can be removed.
        /// </summary>
        public static async void CheckPackageManifestAndRemoveOculus()
        {
            PackageCollection installedPackages;
            bool OculusFound = false;

            try
            {
                installedPackages = await GetInstalledPackagesAsync();
                foreach (var installedPackage in installedPackages)
                {
                    if (installedPackage.name.Contains("com.unity.xr.oculus"))
                    {
                        OculusFound = true;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
                IsRunningCheck = false;
                return;
            }

            if (OculusFound)
            {
                bool userAcceptsUpdate = false;

                string dialogBody = "The XRTK Oculus extension has detected that Unity Oculus UPM packages are installed.\n"
                        + " These Unity packages will override the Oculus API version required by the extension.\n\n"
                        + " Do you want XRTK to automatically remove them?";

                int dialogResult = EditorUtility.DisplayDialogComplex("Remove Unity Oculus Packages", dialogBody, "Yes", "No, Don't Ask Again", "No");

                switch (dialogResult)
                {
                    case 0: // "Yes"
                        userAcceptsUpdate = true;
                        autoRemoveOculusPackagesEnabled = true;
                        break;
                    case 1: // "No, Don't Ask Again"
                        autoRemoveOculusPackagesEnabled = false;

                        EditorUtility.DisplayDialog("If you have issues with the Unity Oculus packages",
                            "Either remove the packages manually using the package manager, or use the following menu option:\n\n"
                                + "[Mixed Reality Toolkit -> Tools -> Remove Unity Oculus UPM Packages]",
                            "Ok",
                            "");
                        return;
                    case 2: // "No"
                        return;
                }

                if (userAcceptsUpdate)
                {
                    foreach (var installedPackage in installedPackages)
                    {
                        if (installedPackage.name.Contains("com.unity.xr.oculus"))
                        {
                            await RemovePackageAsync(installedPackage.name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the currently installed upm xrtk packages.
        /// </summary>
        /// <exception cref="TimeoutException">A <see cref="TimeoutException"/> can occur if the packages are not returned in 10 seconds.</exception>
        internal static async Task<PackageCollection> GetInstalledPackagesAsync()
        {
            var upmPackageListRequest = Client.List(true);

            await upmPackageListRequest.WaitUntil(request => request.IsCompleted, timeout: 30);

            return upmPackageListRequest.Result;
        }

        private static async Task RemovePackageAsync(string packageName)
        {
            var removeRequest = Client.Remove($"{packageName}");
            await removeRequest.WaitUntil(request => request.IsCompleted, timeout: 30);

            if (removeRequest.Error?.errorCode == ErrorCode.NotFound)
            {
                Debug.LogError($"Package Error({removeRequest.Error?.errorCode}): {removeRequest.Error?.message}");
            }
        }

        [MenuItem("Mixed Reality Toolkit/Tools/Remove Unity Oculus UPM Packages")]
        private static void RunPluginUpdate()
        {
            autoRemoveOculusPackagesEnabled = true;
            CheckPackageManifestAndRemoveOculus();
        }
    }
}