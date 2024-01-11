using UnityEditor;
using UnityEngine;

namespace AppBase.UI.Editor
{
    [InitializeOnLoad]
    public class UIBindingHighlight
    {
        static UIBindingHighlight()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        }

        private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (gameObject != null &&
                gameObject.TryGetComponent<UIBinding>(out var binding) &&
                gameObject.transform.parent != null &&
                gameObject.transform.parent.name != "Canvas (Environment)" &&
                binding.IsBindingNameValid())
            {
                var name = string.IsNullOrEmpty(binding.BindingName) ? gameObject.name : binding.BindingName;
                var style = new GUIStyle
                {
                    normal = new GUIStyleState
                    {
                        textColor = binding is UIView ? Color.yellow : Color.red
                    }
                };
                var labelWidth = style.CalcSize(new GUIContent(gameObject.name)).x + 22f;
                EditorGUI.LabelField(new Rect(selectionRect.x + labelWidth, selectionRect.y, selectionRect.width - labelWidth, selectionRect.height), $"↔️ {name}", style);
            }
        }
    }
}