using DialogueSystem.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using Utilities;
    public class DSEditorWindow : EditorWindow
    {
        private DSGraphView graphView;
        private readonly string defaultFileName = "DialogueFile";
        private Button saveButton;
        private TextField fileNameTextField;
        [MenuItem("Window/DS/Dialogue Graph")] 
        public static void Open()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        private void CreateGUI()
        {
            AddGraphView();
            AddToolBar();
            AddStyles();
        }
        #region Elements Addition and Styles
        private void AddToolBar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = DSElementsUtility.CreateTextField(defaultFileName, "File Name: ", callBack =>
            {
                fileNameTextField.value = callBack.newValue.RemoveWhiteSpaces().RemoveSpecialCharacters();
            });

            saveButton = DSElementsUtility.CreateButton("Save", () => Save());
            
            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);    
            toolbar.AddStyleSheets("DialogueSystem/DSToolbarStyles.uss");
            
            rootVisualElement.Add(toolbar);
        }
        
        private void AddGraphView()
        {
            graphView = new DSGraphView(this);
        
            graphView.StretchToParentSize();
        
            rootVisualElement.Add(graphView);
        }
    
        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DialogueSystem/DSVariables.uss");
        }
        #endregion
        #region Toolbar Actions
        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog(
                    "Invalid File Name",
                    "Please ensure the file name you've entered is valid.",
                    "Exit"
                );
                return;
            }
            
            DSIOUtility.Initialize(graphView, fileNameTextField.value);
            DSIOUtility.Save();
        }
        #endregion
        #region Utility Methods

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }
        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }
        

        #endregion
    }
}
