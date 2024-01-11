using UnityEngine;

namespace Project.AppBase.ObjectAnimationSystem.Scripts.Editor
{
    public class EditorStyleConst
    {
        public static GUIStyle BoldFontStyle = new GUIStyle()
        {
            fontStyle =  FontStyle.Bold,
            fontSize = 12
        };

        public static GUIStyle SplitLineStyle = new GUIStyle()
        {
            wordWrap = false,
            margin = new RectOffset(0, 0, 10, 10),
            fontSize = 13,
            fontStyle =  FontStyle.Bold,
            normal = new GUIStyleState()
            {
                textColor = Color.black 
            }
        };

        public static Color MemberBGColor1 = new Color(0.2f,0.8f,0.4f,0.2f);
        public static Color MemberBGColor2 = new Color(0.8f,0.6f,0.4f,0.2f);

    }
}