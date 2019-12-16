// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;
using XRTK.Inspectors.Profiles.InputSystem.Controllers;
using XRTK.Oculus.Profiles;

namespace XRTK.Oculus.Inspectors
{
    [CustomEditor(typeof(OculusRemoteControllerMappingProfile))]
    public class OculusRemoteControllerMappingProfileInspector : BaseMixedRealityControllerMappingProfileInspector { }
}