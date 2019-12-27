// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Utilities;
using XRTK.Oculus.Profiles;
using XRTK.Providers.Controllers.Hands;

namespace XRTK.Oculus.Controllers
{
    public class OculusHandControllerDataProvider : BaseHandControllerDataProvider<OculusHandController>
    {
        private readonly OculusHandControllerDataProviderProfile profile;

        /// <summary>
        /// Creates a new instance of the data provider.
        /// </summary>
        /// <param name="name">Name of the data provider as assigned in the configuration profile.</param>
        /// <param name="priority">Data provider priority controls the order in the service registry.</param>
        /// <param name="profile">Hand controller data provider profile assigned to the provider instance in the configuration inspector.</param>
        public OculusHandControllerDataProvider(string name, uint priority, OculusHandControllerDataProviderProfile profile)
            : base(name, priority, profile)
        {
            this.profile = profile;
        }

        /// <inheritdoc />
        protected override void RefreshActiveControllers()
        {
            OculusApi.HandState handStateLeft = new OculusApi.HandState();
            bool isLeftHandTracked = OculusApi.GetHandState(OculusApi.Step.Render, OculusApi.Hand.HandLeft, ref handStateLeft)
                && (handStateLeft.Status & OculusApi.HandStatus.HandTracked) != 0
            && handStateLeft.HandConfidence == profile.MinConfidenceRequired;

            if (isLeftHandTracked)
            {
                GetOrAddController(Handedness.Left);
            }
            else
            {
                RemoveController(Handedness.Left);
            }

            OculusApi.HandState handStateRight = new OculusApi.HandState();
            bool isRightHandTracked = OculusApi.GetHandState(OculusApi.Step.Render, OculusApi.Hand.HandRight, ref handStateRight)
                && (handStateRight.Status & OculusApi.HandStatus.HandTracked) != 0
            && handStateRight.HandConfidence == profile.MinConfidenceRequired;

            if (isRightHandTracked)
            {
                GetOrAddController(Handedness.Right);
            }
            else
            {
                RemoveController(Handedness.Right);
            }
        }
    }
}