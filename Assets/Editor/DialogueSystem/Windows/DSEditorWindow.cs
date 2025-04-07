using System;
using Editor.DialogueSystem.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

    

    private void AddGraphView()
    {
        DSGraphView graphView = new DSGraphView();
        
        graphView.StretchToParentSize();
        
        
        
        rootVisualElement.Add(graphView);
    }
    
    private void AddStyles()
    {
        StyleSheet styleSheet = EditorGUIUtility.Load("DialogueSystem/DSVariables.uss") as StyleSheet;
        
        rootVisualElement.styleSheets.Add(styleSheet);
    }
}
