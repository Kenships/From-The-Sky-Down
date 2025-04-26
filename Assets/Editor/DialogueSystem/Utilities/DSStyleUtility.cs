using UnityEditor;
using UnityEngine.UIElements;

namespace DialogueSystem.Utilities
{
    public static class DSStyleUtility
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (var className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }
        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (var styleName in styleSheetNames)
            {
                StyleSheet styleSheet = EditorGUIUtility.Load(styleName) as StyleSheet;
        
                element.styleSheets.Add(styleSheet);
            }

            return element;
        }
    }
}
