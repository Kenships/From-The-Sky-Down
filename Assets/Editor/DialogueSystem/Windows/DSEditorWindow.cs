using System.IO;
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
        private Button minimapButton;
        private static TextField fileNameTextField;
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

            saveButton = DSElementsUtility.CreateButton("Save", Save);

            Button loadButton = DSElementsUtility.CreateButton("Load", Load);
            Button clearButton = DSElementsUtility.CreateButton("Clear", Clear);
            Button resetButton = DSElementsUtility.CreateButton("Reset", ResetGraph);
            
            minimapButton = DSElementsUtility.CreateButton("Minimap", ToggleMinimap);
            
            
            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);  
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(minimapButton);
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
        private void Load()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", "Assets/Editor/DialogueSystem/Graphs", "asset");

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            
            Clear();
            
            DSIOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.Load(Path.GetDirectoryName(filePath).TrimFilePathToAssetPath());
        }
        private void Clear()
        {
            graphView.ClearGraph();
        }
        
        private void ResetGraph()
        {
            Clear();
            UpdateFileName(defaultFileName);
        }
        
        private void ToggleMinimap()
        {
            graphView.ToggleMiniMap();
            
            minimapButton.ToggleInClassList("ds-toolbar__button__selected");
        }
        
        
        #endregion
        #region Utility Methods

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

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
