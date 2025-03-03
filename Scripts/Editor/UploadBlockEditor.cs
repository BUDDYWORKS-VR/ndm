using UnityEditor;

#if UNITY_EDITOR

namespace WTF.BUDDYWORKS.NDM
{
    [CustomEditor(typeof(BWUploadBlock))]
    public class UploadBlockEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            BannerUtility.DrawBanner();

            EditorGUILayout.HelpBox("This component lets you prevent uploads from happening at all, or allow it only when the editor is currently in a specific BuildTarget.",
                MessageType.Info);
            
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script"); // Hide the Script field
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif