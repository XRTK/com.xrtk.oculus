// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.CameraSystem;
using XRTK.Interfaces.CameraSystem;
using XRTK.Oculus.Plugins;
using XRTK.Providers.CameraSystem;

namespace XRTK.Oculus.Providers.CameraSystem
{
    public class OculusCameraDataProvider : BaseCameraDataProvider
    {
        /// <inheritdoc />
        public OculusCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

        /// <inheritdoc />
        protected override void ApplySettingsForDefaultHeadHeight()
        {
            HeadHeight = OculusApi.EyeHeight;
            ResetRigTransforms();
            SyncRigTransforms();
        }
    }
}
