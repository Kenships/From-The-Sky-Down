using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor.DialogueSystem.Windows
{
    public class DSGraphView : GraphView
    {
        public DSGraphView()
        {
            AddGridBackground();
            AddStyles();
        }
        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            
            Insert(0, gridBackground);
        }
        private void AddStyles()
        {
            StyleSheet styleSheet = EditorGUIUtility.Load("DialogueSystem/DSGraphViewStyles.uss") as StyleSheet;
        
            styleSheets.Add(styleSheet);
        }
    }
    
    
}
