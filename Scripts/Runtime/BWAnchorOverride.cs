#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using UnityEditor;

namespace WTF.BUDDYWORKS.NDM
{
    [AddComponentMenu("BUDDYWORKS/Anchor Override Changer")]
    public class BWAnchorOverride : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Drag the desired anchor override object here.")]
        public Transform anchorOverrideTransform;
        
        [Tooltip("Skip meshes that already have an anchor override set.")]
        public bool skipAlreadyAnchoredMeshes = true;
        
        [Tooltip("Specify meshes to be ignored.")]
        public List<GameObject> ignoredMeshes = new List<GameObject>();

        [Tooltip("Ignore all children of the ignored meshes recursively.")]
        public bool ignoreRecursively = false;
    }

    public class AnchorOverrideProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -100000000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var anchorOverrides = avatarGameObject.GetComponentsInChildren<BWAnchorOverride>();

            if (anchorOverrides.Length == 0)
            {
                return true;
            }
            
            if (anchorOverrides.Length > 1)
            {
                EditorUtility.DisplayDialog("Anchor Override Error", 
                    "You have extra BUDDYWORKS Anchor Override components attached to your avatar. You can only have one, please remove the others.", "OK");
                Debug.LogError(BWStrings.bwAOC + BWStrings.logAbort + "Multiple AnchorOverride components detected on the avatar. Aborting build process.");
                return false;
            }

            var anchorOverrideSetter = anchorOverrides.FirstOrDefault();
            var avatarDescriptor = avatarGameObject.GetComponent<VRCAvatarDescriptor>();
            if (anchorOverrideSetter?.anchorOverrideTransform == null || avatarDescriptor == null)
            {
                Debug.LogWarning(BWStrings.bwAOC + BWStrings.logInfo + "No valid anchor or avatar descriptor found.");
                return true;
            }
    
            Transform anchorOverride = anchorOverrideSetter.anchorOverrideTransform;
            var skinnedMeshes = avatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshRenderers = avatarDescriptor.GetComponentsInChildren<MeshRenderer>(true);
            bool skipAlreadyAnchored = anchorOverrideSetter.skipAlreadyAnchoredMeshes;
            bool ignoreRecursively = anchorOverrideSetter.ignoreRecursively;

            // Initialize ignoredSet directly as a HashSet
            HashSet<GameObject> ignoredSet = new HashSet<GameObject>(anchorOverrideSetter.ignoredMeshes);
            List<string> changedMeshes = new List<string>(); // List to store names of changed meshes
            List<string> ignoredMeshesLog = new List<string>(); // List to store names of ignored meshes
            List<string> skippedMeshesLog = new List<string>(); // List to store names of skipped meshes
            
            if (ignoreRecursively)
            {
                foreach (var obj in anchorOverrideSetter.ignoredMeshes)
                {
                    AddChildrenToIgnore(obj.transform, ignoredSet);
                }
            }
    
            foreach (var renderer in skinnedMeshes.Concat<Renderer>(meshRenderers))
            {
                if (ignoredSet.Contains(renderer.gameObject))
                {
                    ignoredMeshesLog.Add(renderer.name); // Log ignored mesh
                    continue;
                }

                if (skipAlreadyAnchored && renderer.probeAnchor != null)
                {
                    skippedMeshesLog.Add(renderer.name); // Log skipped mesh
                    continue;
                }
                
                renderer.probeAnchor = anchorOverride;
                changedMeshes.Add(renderer.name); // Add the name of the changed mesh
            }

            // Log ignored meshes if any
            if (ignoredMeshesLog.Count > 0)
            {
                Debug.Log(BWStrings.bwAOC + BWStrings.logInfo + "Ignored meshes: " + string.Join(", ", ignoredMeshesLog));
            }

            // Log skipped meshes if any
            if (skippedMeshesLog.Count > 0)
            {
                Debug.Log(BWStrings.bwAOC + BWStrings.logInfo + "Skipped already set meshes: " + string.Join(", ", skippedMeshesLog));
            }

            // Log the success message with the names of the changed meshes
            if (changedMeshes.Count > 0)
            {
                Debug.Log(BWStrings.bwAOC + BWStrings.logSuccess + $"Applied anchor override to {changedMeshes.Count} meshes: {string.Join(", ", changedMeshes)}.");
            }
            else
            {
                Debug.Log(BWStrings.bwAOC + BWStrings.logInfo + "No meshes were changed.");
            }

            return true;
        }
    
        private void AddChildrenToIgnore(Transform parent, HashSet<GameObject> ignoredSet)
        {
            foreach (Transform child in parent)
            {
                ignoredSet.Add(child.gameObject);
                AddChildrenToIgnore(child, ignoredSet);
            }
        }
    }
}
#endif
