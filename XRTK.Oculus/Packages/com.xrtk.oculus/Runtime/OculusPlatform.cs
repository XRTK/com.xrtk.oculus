using XRTK.Definitions.Platforms;

namespace XRTK.Oculus
{
    public class OculusPlatform : BasePlatform
    {
        private static System.Version noVersion = new System.Version();

        /// <inheritdoc />
        public override bool IsAvailable => !UnityEngine.Application.isEditor && OculusApi.Version > noVersion && OculusApi.Initialized;

        /// <inheritdoc />
        public override bool IsBuildTargetAvailable
        {
            get
            {
#if UNITY_EDITOR
                return (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android ||
                        UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows ||
                        UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneWindows64) &&
                        OculusApi.Version > noVersion;
#else
                return false;
#endif
            }
        }
    }
}