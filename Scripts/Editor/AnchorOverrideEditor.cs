using UnityEditor;

#if UNITY_EDITOR

namespace WTF.BUDDYWORKS.NDM
{
    [CustomEditor(typeof(AnchorOverride))]
    public class AnchorOverrideEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            BannerUtility.DrawBanner();

            EditorGUILayout.HelpBox("This component lets you modify the Anchor Override for all your meshes OnBuild.",
                MessageType.Info);
            
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script"); // Hide the Script field
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif