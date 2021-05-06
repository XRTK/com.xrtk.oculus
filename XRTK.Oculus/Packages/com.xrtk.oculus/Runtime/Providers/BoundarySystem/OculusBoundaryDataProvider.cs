// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using XRTK.Attributes;
using XRTK.Definitions;
using XRTK.Definitions.BoundarySystem;
using XRTK.Interfaces.BoundarySystem;
using XRTK.Oculus.Extensions;
using XRTK.Oculus.Plugins;
using XRTK.Services;

namespace XRTK.Oculus.Providers.BoundarySystem
{
    [RuntimePlatform(typeof(OculusPlatform))]
    [System.Runtime.InteropServices.Guid("8EF0CAB5-A37C-4912-AD5E-1E57E92A314D")]
    public class OculusBoundaryDataProvider : BaseDataProvider, IMixedRealityBoundaryDataProvider
    {
        /// <inheritdoc />
        public OculusBoundaryDataProvider(string name, uint priority, BaseMixedRealityProfile profile, IMixedRealityBoundarySystem parentService)
            : base(name, priority, profile, parentService)
        {
            boundarySystem = parentService;
        }

        private readonly IMixedRealityBoundarySystem boundarySystem;

        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            boundarySystem.SetupBoundary(this);
        }

        #region IMixedRealityBoundaryDataProvider Implementation

        /// <inheritdoc />
        public BoundaryVisibility Visibility => BoundaryVisibility.Unknown;

        /// <inheritdoc />
        public bool IsPlatformConfigured
        {
            get
            {
                return OculusApi.GetBoundaryConfigured();
            }
        }

        /// <inheritdoc />
        public bool TryGetBoundaryGeometry(ref List<Vector3> geometry)
        {
            geometry.Clear();
            Vector3[] oculusGeometry = GetGeometry(OculusApi.BoundaryType.PlayArea);
            if (oculusGeometry == null || oculusGeometry.Length == 0)
            {
                return false;
            }

            geometry.AddRange(oculusGeometry);
            return true;
        }

        #endregion  IMixedRealityBoundaryDataProvider Implementation

        #region Oculus Utils

        private static int cachedVector3fSize = Marshal.SizeOf(typeof(OculusApi.Vector3f));
        private static OculusApi.OVRNativeBuffer cachedGeometryNativeBuffer = new OculusApi.OVRNativeBuffer(0);
        private static float[] cachedGeometryManagedBuffer = new float[0];
        private List<Vector3> cachedGeometryList = new List<Vector3>();

        /// <summary>
        /// Returns an array of 3d points (in clockwise order) that define the specified boundary type.
        /// All points are returned in local tracking space shared by tracked nodes and accessible through OVRCameraRig's trackingSpace anchor.
        /// </summary>
        public Vector3[] GetGeometry(OculusApi.BoundaryType boundaryType)
        {
            int pointsCount = 0;
            if (OculusApi.GetBoundaryGeometry((OculusApi.BoundaryType)boundaryType, IntPtr.Zero, ref pointsCount))
            {
                if (pointsCount > 0)
                {
                    int requiredNativeBufferCapacity = pointsCount * cachedVector3fSize;
                    if (cachedGeometryNativeBuffer.GetCapacity() < requiredNativeBufferCapacity)
                    {
                        cachedGeometryNativeBuffer.Reset(requiredNativeBufferCapacity);
                    }

                    int requiredManagedBufferCapacity = pointsCount * 3;
                    if (cachedGeometryManagedBuffer.Length < requiredManagedBufferCapacity)
                    {
                        cachedGeometryManagedBuffer = new float[requiredManagedBufferCapacity];
                    }

                    if (OculusApi.GetBoundaryGeometry((OculusApi.BoundaryType)boundaryType, cachedGeometryNativeBuffer.GetPointer(), ref pointsCount))
                    {
                        Marshal.Copy(cachedGeometryNativeBuffer.GetPointer(), cachedGeometryManagedBuffer, 0, requiredManagedBufferCapacity);

                        Vector3[] points = new Vector3[pointsCount];

                        for (int i = 0; i < pointsCount; i++)
                        {
                            points[i] = new OculusApi.Vector3f()
                            {
                                x = cachedGeometryManagedBuffer[3 * i + 0],
                                y = cachedGeometryManagedBuffer[3 * i + 1],
                                z = cachedGeometryManagedBuffer[3 * i + 2],
                            }.FromFlippedZVector3f();
                        }

                        return points;
                    }
                }
            }

            return new Vector3[0];
        }

        /// <summary>
        /// Returns a vector that indicates the spatial dimensions of the specified boundary type. (x = width, y = height, z = depth)
        /// </summary>
        public Vector3 GetDimensions(OculusApi.BoundaryType boundaryType)
        {
            return OculusApi.GetBoundaryDimensions((OculusApi.BoundaryType)boundaryType).ToVector3();
        }

        #endregion
    }
}
