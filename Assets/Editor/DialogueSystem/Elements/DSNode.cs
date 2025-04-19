using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;
    public class DSNode : Node
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public DSGroup Group { get; set; }
        
        private DSGraphView graphView;
        private Color defaultBackgroundColor;
        
        public virtual void Initialize(Vector2 position, DSGraphView dsGraphView)
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue Text.";
            
            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
            
            SetPosition(new Rect(position, Vector2.zero));
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            TextField dialogueNameTextField = DSElementsUtility.CreateTextField(DialogueName, callback => 
            {
                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);
                
                    DialogueName = callback.newValue;
                
                    graphView.AddUngroupedNode(this);
                    
                    return;
                }
                
                DSGroup currentGroup = Group;
                
                graphView.RemoveGroupedNode(this, Group);
                
                DialogueName = callback.newValue;
                
                graphView.AddGroupedNode(this, currentGroup);
            });

        dialogueNameTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__textfield__hidden", 
                "ds-node__filename-textfield");
            
            titleContainer.Insert(0, dialogueNameTextField);
        
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            
            inputContainer.Add(inputPort);
            
            VisualElement customDataContainer = new VisualElement();
            
            customDataContainer.AddClasses(
                "ds-node__custom-data-container");
            
            Foldout textFoldout = DSElementsUtility.CreateFoldout("Dialogue Text");

            TextField textField = DSElementsUtility.CreateTextArea(Text);
            
            textField.AddClasses(
                "ds-node__textfield", 
                "ds-node__quote-textfield");
            
            textFoldout.Add(textField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
            
            RefreshExpandedState();
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
    }
}
