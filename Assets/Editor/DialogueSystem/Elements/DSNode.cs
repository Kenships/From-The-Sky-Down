using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Enumerations;
    using Utilities;
    public class DSNode : Node
    {
        public string DialogueName { get; set; }
        public List<string> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }

        public virtual void Initialize(Vector2 position)
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue Text.";
            
            SetPosition(new Rect(position, Vector2.zero));
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {
            TextField dialogueNameTextField = DSElementsUtility.CreateTextField(DialogueName);
            
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
    }
}
