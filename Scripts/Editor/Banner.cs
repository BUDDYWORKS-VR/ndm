using System.Diagnostics;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace WTF.BUDDYWORKS.NDM
{
    public static class BannerUtility
    {
        private static Texture2D banner;
        private static Color backgroundColor = new Color(0.992f, 0.855f, 0.051f); // #FDDA0D

        static BannerUtility()
        {
            string bannerPath = AssetDatabase.GUIDToAssetPath("24894187d778f32409be2f8f91f4ff4a");
            banner = AssetDatabase.LoadAssetAtPath<Texture2D>(bannerPath);
        }

        public static void DrawBanner()
        {
            if (banner == null) return;

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
    }
}
#endif