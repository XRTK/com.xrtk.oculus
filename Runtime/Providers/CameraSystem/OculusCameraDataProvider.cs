﻿// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.CameraSystem;
using XRTK.Interfaces.CameraSystem;
using XRTK.Oculus.Plugins;
using XRTK.Providers.CameraSystem;

namespace XRTK.Oculus.Providers.CameraSystem
{
    [RuntimePlatform(typeof(OculusPlatform))]
    [System.Runtime.InteropServices.Guid("83EFF552-ADF4-47C8-AD53-DF7406856D3F")]
    public class OculusCameraDataProvider : BaseCameraDataProvider
    {
        /// <inheritdoc />
        public OculusCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }

#if XRTK_USE_LEGACYVR
        /// <inheritdoc />
        protected override void ApplySettingsForDefaultHeadHeight()
        {
            HeadHeight = OculusApi.EyeHeight;
            ResetRigTransforms();
            SyncRigTransforms();
        }
#endif

        /// <inheritdoc />
        public override void Update()
        {
            OculusApi.UpdateHMDEvents();
            OculusApi.UpdateUserEvents();
        }
    }
}
