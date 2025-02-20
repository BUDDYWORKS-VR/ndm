#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using VRC.SDKBase.Editor.BuildPipeline;
using System.Diagnostics;

namespace WTF.BUDDYWORKS.NDM
{
    [AddComponentMenu("BUDDYWORKS/Anchor Override Changer")]
    public class AnchorOverride : MonoBehaviour, IEditorOnly
    {
        [Tooltip("Drag the desired anchor override object here.")]
        public Transform anchorOverrideTransform;
    }

    [CustomEditor(typeof(AnchorOverride))]
    public class AnchorOverrideEditor : Editor
    {
        private Texture2D banner;
        private Color backgroundColor = new Color(0.992f, 0.855f, 0.051f); // #FDDA0D

        private void OnEnable()
        {
            string bannerPath = AssetDatabase.GUIDToAssetPath("24894187d778f32409be2f8f91f4ff4a");
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>(bannerPath);
        }

        public override void OnInspectorGUI()
        {
            if (banner)
            {
                float inspectorWidth = EditorGUIUtility.currentViewWidth;
                float bannerWidth = banner.width;
                float padding = (inspectorWidth - bannerWidth) / 2;

                Rect rect = GUILayoutUtility.GetRect(inspectorWidth, banner.height);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, inspectorWidth, rect.height), backgroundColor);
                
                if (GUI.Button(new Rect(rect.x + padding, rect.y, bannerWidth, rect.height), banner, GUIStyle.none))
                {
                    Process.Start(new ProcessStartInfo("https://buddyworks.wtf") { UseShellExecute = true });
                }
            }

            EditorGUILayout.HelpBox("This component lets you modify the Anchor Override for all your meshes OnBuild.",
                MessageType.Info);
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script"); // Hide the Script field
            serializedObject.ApplyModifiedProperties();
        }
    }

    public class AnchorOverrideProcessor : IVRCSDKPreprocessAvatarCallback
    {
        public static string logHeader = "<color=grey>[</color><color=#FDDA0D>BUDDYWORKS</color><color=grey>] </color>";
        public static string logAbort = logHeader + "<color=red>ERROR:</color> ";
        public static string logInfo = logHeader + "<color=white>Log:</color> ";
        public static string logSuccess = logHeader + "<color=green>OK:</color> ";
        
        public int callbackOrder => -100000000;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            AnchorOverride anchorOverrideSetter = avatarGameObject.GetComponentInChildren<AnchorOverride>();
            if (anchorOverrideSetter == null) return true;

            VRCAvatarDescriptor avatarDescriptor = avatarGameObject.GetComponent<VRCAvatarDescriptor>();
            if (avatarDescriptor == null) return true;

            Transform anchorOverride = anchorOverrideSetter.anchorOverrideTransform;
            if (anchorOverride == null)
            {
                UnityEngine.Debug.LogWarning(logInfo + "No anchor object assigned.");
                return true;
            }

            SkinnedMeshRenderer[] skinnedMeshes = avatarDescriptor.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            MeshRenderer[] meshRenderers = avatarDescriptor.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var skinnedMesh in skinnedMeshes)
            {
                skinnedMesh.probeAnchor = anchorOverride;
            }

            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.probeAnchor = anchorOverride;
            }

            UnityEngine.Debug.Log(logSuccess + $"Applied anchor override to {skinnedMeshes.Length} skinned meshes and {meshRenderers.Length} regular meshes.");
            return true;
        }
    }
}
#endif
