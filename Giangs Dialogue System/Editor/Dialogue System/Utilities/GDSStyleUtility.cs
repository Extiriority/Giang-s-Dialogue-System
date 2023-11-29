using UnityEditor;
using UnityEngine.UIElements;

public static class GDSStyleUtility {
    
    public static VisualElement addClasses(this VisualElement element, params string[] classNames) {
        foreach (string className in classNames) {
            element.AddToClassList(className);
        }

        return element;
    }
    public static VisualElement addStyleSheets(this VisualElement element, params string[] styleSheetNames)
    {
        foreach (string styleSheetName in styleSheetNames) {
            var styleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetName);
            element.styleSheets.Add(styleSheet);
        }

        return element;
    }
}
