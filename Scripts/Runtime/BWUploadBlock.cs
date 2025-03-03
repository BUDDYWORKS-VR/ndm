#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

namespace WTF.BUDDYWORKS.NDM
{
    [AddComponentMenu("BUDDYWORKS/Upload Blocker")]
    public class BWUploadBlock : MonoBehaviour, IEditorOnly
    {
        public bool allowWindows = false;
        public bool allowAndroid = false;
        public bool allowIOS = false;
    }

    public class UploadBlockProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -100000001;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            // Get the BWUploadBlock component from the avatar or its children
            BWUploadBlock[] blockers = avatarGameObject.GetComponentsInChildren<BWUploadBlock>(true);

            if (blockers.Length == 0)
            {
                return true;
            }

            if (blockers.Length > 1)
            {
                EditorUtility.DisplayDialog("BUDDYWORKS UploadBlock tripped", 
                    "You have multiple UploadBlock components on this avatar.\nThis is not allowed, remove all extra components and try again.", "OK");
                Debug.LogError(BWStrings.bwUB + BWStrings.logAbort + "Upload blocked: Multiple BWUploadBlock components found.");
                return false; // Abort upload due to multiple blockers
            }

            BWUploadBlock blocker = blockers[0];
            
            // Get the current Unity build target
            BuildTarget currentTarget = EditorUserBuildSettings.activeBuildTarget;

            // Check if the current build target is allowed
            bool isAllowed = (blocker.allowWindows && (currentTarget == BuildTarget.StandaloneWindows ||
                                                       currentTarget == BuildTarget.StandaloneWindows64)) ||
                             (blocker.allowAndroid && currentTarget == BuildTarget.Android) ||
                             (blocker.allowIOS && currentTarget == BuildTarget.iOS);

            if (isAllowed)
            {
                return true;
            }

            EditorUtility.DisplayDialog("BUDDYWORKS UploadBlock tripped", 
                $"This avatar has a Blocker attached which doesn't allow building for your current build target.\n\nCurrent build target: {currentTarget}", "OK");
            Debug.LogError(BWStrings.bwUB + BWStrings.logAbort + $"Upload blocked: {currentTarget} is not an allowed build target.");
            return false;
        }
    }
}
#endif