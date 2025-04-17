using DialogueSystem.Utilities;
using UnityEditor;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using Utilities;
    public class DSEditorWindow : EditorWindow
    {
        [MenuItem("Window/DS/Dialogue Graph")] 
        public static void Open()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddStyles();
        }


        #region Elements Addition and Styles
        private void AddGraphView()
        {
            DSGraphView graphView = new DSGraphView();
        
            graphView.StretchToParentSize();
        
            rootVisualElement.Add(graphView);
        }
    
        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
        }
        #endregion
    }
}
