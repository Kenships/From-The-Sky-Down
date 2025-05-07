using System.Collections.Generic;
using DialogueSystem.ScriptableObjects;
using UnityEditor;

namespace DialogueSystem.Inspectors
{
    using Utilities;
    [CustomEditor(typeof(DSDialogue))]
    public class DSInspector : Editor
    {
        /* Dialogue Scriptable Objects */
        private SerializedProperty _dialogueContainerProperty;
        private SerializedProperty _dialogueGroupProperty;
        private SerializedProperty _dialogueProperty;
        
        /* Filters */
        private SerializedProperty _groupedDialoguesProperty;
        private SerializedProperty _startingDialoguesOnlyProperty;
        
        /* Indices */
        private SerializedProperty _selectedDialogueGroupIndexProperty;
        private SerializedProperty _selectedDialogueIndexProperty;
        
        private void OnEnable()
        {
            _dialogueContainerProperty = serializedObject.FindProperty("_dialogueContainer");
            _dialogueGroupProperty = serializedObject.FindProperty("_dialogueGroup");
            _dialogueProperty = serializedObject.FindProperty("_dialogue");
            
            _groupedDialoguesProperty = serializedObject.FindProperty("_groupedDialogues");
            _startingDialoguesOnlyProperty = serializedObject.FindProperty("_startingDialoguesOnly");
            
            _selectedDialogueGroupIndexProperty = serializedObject.FindProperty("_selectedDialogueGroupIndex");
            _selectedDialogueIndexProperty = serializedObject.FindProperty("_selectedDialogueIndex");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawDialogueContainerArea();
            
            DSDialogueContainerSO dialogueContainer = _dialogueContainerProperty.objectReferenceValue as DSDialogueContainerSO;
            
            if(!dialogueContainer)
            {
                StopDrawing("Please select a Dialogue Container.");
                return;
            }
            DrawFiltersArea();
            
            bool startingDialoguesOnly = _startingDialoguesOnlyProperty.boolValue;

            List<string> dialogueNames;

            string dialogueFolderPath = $"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}";

            string dialogueInfoMessage;
            if (_groupedDialoguesProperty.boolValue)
            {
                List<string> dialogueGroupNames = dialogueContainer.GetDialogueGroupNames();

                if (dialogueGroupNames.Count == 0)
                {
                    StopDrawing("There are no dialogue groups in the selected Dialogue Container.");

                    return;
                }
                
                DrawDialogueGroupArea(dialogueContainer, dialogueGroupNames);
                
                DSDialogueGroupSO dialogueGroup = _dialogueGroupProperty.objectReferenceValue as DSDialogueGroupSO;

                dialogueNames = dialogueContainer.GetGroupedDialogueNames(dialogueGroup, startingDialoguesOnly);
                
                dialogueFolderPath += $"/Groups/{dialogueGroup.GroupName}/Dialogues";
                
                dialogueInfoMessage = "There are no " + (startingDialoguesOnly ? "starting" : "") + " Dialogues in this dialogue group.";
            }
            else
            {
                dialogueNames = dialogueContainer.GetUngroupedDialogueNames(startingDialoguesOnly);
                
                dialogueFolderPath += "/Global/Dialogues";
                
                dialogueInfoMessage = "There are no " + (startingDialoguesOnly ? "starting" : "") + " Ungrouped Dialogues in this Dialogue Container.";
            }
            
            if(dialogueNames.Count == 0)
            {
                StopDrawing(dialogueInfoMessage);
                
                return;
            }
            
            DrawDialogueArea(dialogueNames, dialogueFolderPath);

            serializedObject.ApplyModifiedProperties();
        }

        


        #region Draw Methods

        private void DrawDialogueContainerArea()
        {
            DSInspectorUtility.DrawHeader("Dialogue Container");
            
            _dialogueContainerProperty.DrawPropertyField();
            
            DSInspectorUtility.DrawSpace();
        }

        private void DrawFiltersArea()
        {
            DSInspectorUtility.DrawHeader("Filters");
            
            _groupedDialoguesProperty.DrawPropertyField();
            _startingDialoguesOnlyProperty.DrawPropertyField();
            
            DSInspectorUtility.DrawSpace();
        }

        private void DrawDialogueGroupArea(DSDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            DSInspectorUtility.DrawHeader("Dialogue Group");
            
            int oldSelectedIndex = _selectedDialogueGroupIndexProperty.intValue;
            
            DSDialogueGroupSO oldDialogueGroup = _dialogueGroupProperty.objectReferenceValue as DSDialogueGroupSO;

            bool isOldPropertyNull = !oldDialogueGroup;
            string oldPropertyName = isOldPropertyNull ? string.Empty : oldDialogueGroup.GroupName;

            UpdateIndexOnNamesListUpdate(dialogueGroupNames, _selectedDialogueGroupIndexProperty, oldSelectedIndex, oldPropertyName, isOldPropertyNull);
            
            _selectedDialogueGroupIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue Group", _selectedDialogueGroupIndexProperty.intValue, dialogueGroupNames.ToArray());
            
            string selectedDialogueGroupName = dialogueGroupNames[_selectedDialogueGroupIndexProperty.intValue];
            
            DSDialogueGroupSO selectedDialogueGroup = DSIOUtility.LoadAsset<DSDialogueGroupSO>($"Assets/DialogueSystem/Dialogues/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}", selectedDialogueGroupName);
            
            _dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;
            
            DSInspectorUtility.DrawDisabledFields(() => _dialogueGroupProperty.DrawPropertyField());
            
            DSInspectorUtility.DrawSpace();
        }

        

        private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            DSInspectorUtility.DrawHeader("Dialogue");
            
            int oldSelectedIndex = _selectedDialogueIndexProperty.intValue;
            
            DSDialogueSO oldDialogue = _dialogueProperty.objectReferenceValue as DSDialogueSO;
            
            bool isOldPropertyNull = !oldDialogue;
            string oldPropertyName = isOldPropertyNull ? string.Empty : oldDialogue.DialogueName;
            
            UpdateIndexOnNamesListUpdate(dialogueNames, _selectedDialogueIndexProperty, oldSelectedIndex, oldPropertyName, isOldPropertyNull);
            
            _selectedDialogueIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue", _selectedDialogueIndexProperty.intValue, dialogueNames.ToArray());
            
            string selectedDialogueName = dialogueNames[_selectedDialogueIndexProperty.intValue];
            
            DSDialogueSO selectedDialogue = DSIOUtility.LoadAsset<DSDialogueSO>(dialogueFolderPath, selectedDialogueName);
            
            _dialogueProperty.objectReferenceValue = selectedDialogue;
            
            DSInspectorUtility.DrawDisabledFields(() => _dialogueProperty.DrawPropertyField());
        }
        
        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            
            DSInspectorUtility.DrawHelpBox(reason, messageType);
            DSInspectorUtility.DrawSpace();
            DSInspectorUtility.DrawHelpBox("You need to select a dialogue for this component to work in runtime!", MessageType.Warning);
            
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Index Methods

        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty,
            int oldSelectedIndex, string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;

                return;
            }

            bool oldIndexIsValid = oldSelectedIndex > optionNames.Count - 1;
            bool oldNameIsValid = oldIndexIsValid || oldPropertyName != optionNames[oldSelectedIndex];

            if(oldNameIsValid)
            {
                if(optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);
                }
                else
                {
                    indexProperty.intValue = 0;
                }
            }
        }

        #endregion
    }
}
