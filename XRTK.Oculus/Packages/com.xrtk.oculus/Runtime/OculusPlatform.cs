using XRTK.Definitions.Platforms;

namespace XRTK.Oculus
{
    public class OculusPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable => !UnityEngine.Application.isEditor && OculusApi.Version != null && OculusApi.Initialized;

        /// <inheritdoc />
        public override bool IsBuildTargetAvailable
        {
            get
            {
#if UNITY_EDITOR
                return (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ||
                        UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows ||
                        UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64) &&
                       OculusApi.Version != null && OculusApi.Initialized;
#else
                return false;
#endif
            }
        }
    }
}
