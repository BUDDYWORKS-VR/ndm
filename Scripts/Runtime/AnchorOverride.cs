#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;

namespace WTF.BUDDYWORKS.NDM
{
    [AddComponentMenu("BUDDYWORKS/Anchor Override Changer")]
    public class AnchorOverride : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Drag the desired anchor override object here.")]
        public Transform anchorOverrideTransform;
        
        [Tooltip("Specify meshes to be excluded.")]
        public List<GameObject> excludedMeshes = new List<GameObject>();
    }

    public class AnchorOverrideProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => -100000000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            var anchorOverrideSetter = avatarGameObject.GetComponentInChildren<AnchorOverride>();
            var avatarDescriptor = avatarGameObject.GetComponent<VRCAvatarDescriptor>();
            if (anchorOverrideSetter?.anchorOverrideTransform == null || avatarDescriptor == null)
            {
                Debug.LogWarning(BWStrings.logInfo + "No valid anchor or avatar descriptor found.");
                return true;
            }
    
            Transform anchorOverride = anchorOverrideSetter.anchorOverrideTransform;
            var skinnedMeshes = avatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshRenderers = avatarDescriptor.GetComponentsInChildren<MeshRenderer>(true);
            var excludedMeshes = anchorOverrideSetter.excludedMeshes ?? new List<GameObject>();
    
            foreach (var renderer in skinnedMeshes.Concat<Renderer>(meshRenderers))
            {
                if (excludedMeshes.Any(excluded => excluded == renderer.gameObject))
                {
                    Debug.Log(BWStrings.logInfo + $"Skipping excluded mesh: {renderer.name}");
                    continue;
                }
                
                renderer.probeAnchor = anchorOverride;
            }
    
            Debug.Log(BWStrings.logSuccess + $"Applied anchor override to {skinnedMeshes.Length + meshRenderers.Length - excludedMeshes.Count} meshes.");
            return true;
        }
    }
}
#endif
