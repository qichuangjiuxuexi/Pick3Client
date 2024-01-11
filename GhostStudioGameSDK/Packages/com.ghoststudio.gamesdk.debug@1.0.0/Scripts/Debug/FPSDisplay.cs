using UnityEngine;

namespace AppBase.Debugging
{
    /// <summary>
    /// FPS显示
    /// </summary>
    public class FPSDisplay : MonoBehaviour
    {
        private float deltaTime;

        protected void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
    
        protected void OnGUI()
        {
            int w = Screen.width, h = Screen.height;
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(0, 0, w, h * 2f / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h  / 40;
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            style.normal.textColor = fps > 40 ? new Color(1.0f, 1.0f, 0.0f, 1.0f) : new Color(1.0f, 0.0f, 0.0f, 1.0f);
            string text = $"{msec:0.0} ms ({fps:0.} fps)";
            GUI.Label(rect, text, style);
        }
    }
}