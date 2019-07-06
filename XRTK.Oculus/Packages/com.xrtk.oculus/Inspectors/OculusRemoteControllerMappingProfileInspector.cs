/// <inheritdoc />
/// <remarks> Note, MUST use RAW button types as that is what the API works with, DO NOT use Virtual!</remarks>

using UnityEditor;
using XRTK.Inspectors.Profiles;
using XRTK.Oculus.Profiles;

namespace XRTK.Oculus.Inspectors
{
    [CustomEditor(typeof(OculusRemoteControllerMappingProfile))]
    public class OculusRemoteControllerMappingProfileInspector : BaseMixedRealityControllerMappingProfileInspector { }
}