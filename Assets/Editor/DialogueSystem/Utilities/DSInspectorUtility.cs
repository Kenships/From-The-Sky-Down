using System;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem.Utilities
{
    public static class DSInspectorUtility
    {
        public static void DrawDisabledFields(Action action)
        {
            EditorGUI.BeginDisabledGroup(true);
            
            action.Invoke();
            
            EditorGUI.EndDisabledGroup();
        }
        public static void DrawHeader(string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        public static void DrawHelpBox(string message, MessageType messageType = MessageType.Info, bool wide = true)
        {
            EditorGUILayout.HelpBox(message, messageType, wide);
        }
        
        public static void DrawPropertyField(this SerializedProperty property)
        {
            EditorGUILayout.PropertyField(property);
        }

        public static int DrawPopup(string label, SerializedProperty property, string[] options)
        {
            EditorGUILayout.LabelField(label);
            return EditorGUILayout.Popup(property.intValue, options);
        }
        
        public static int DrawPopup(string label, int selectedIndex, string[] options)
        {
            EditorGUILayout.LabelField(label);
            return EditorGUILayout.Popup(selectedIndex, options);
        }
        
        public static void DrawSpace(int space = 4)
        {
            EditorGUILayout.Space(space);
        }
    }
}
