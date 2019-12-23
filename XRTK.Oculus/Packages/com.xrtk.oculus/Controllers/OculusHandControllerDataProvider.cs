// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Oculus.Profiles;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Oculus.Controllers
{
    public class OculusHandControllerDataProvider : BaseHandControllerDataProvider
    {
        /// <summary>
        /// Creates a new instance of the data provider.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Hand controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public OculusHandControllerDataProvider(string name, uint priority, OculusHandControllerDataProviderProfile profile)
            : base(name, priority, profile) { }
    }
}