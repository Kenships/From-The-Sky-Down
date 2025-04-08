using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Enumerations;
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
            TextField dialogueNameTextField = new TextField()
            {
                value = DialogueName
            };

            dialogueNameTextField.AddToClassList("ds-node__textfield");
            dialogueNameTextField.AddToClassList("ds-node__textfield__hidden");
            dialogueNameTextField.AddToClassList("ds-node__filename-textfield");
            

            titleContainer.Insert(0, dialogueNameTextField);

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            
            inputPort.portName = "Dialogue Connection";
            
            inputContainer.Add(inputPort);
            
            VisualElement customDataContainer = new VisualElement();
            
            customDataContainer.AddToClassList("ds-node__custom-data-container");
            
            Foldout textFoldout = new Foldout()
            {
                text = "Dialogue Text"
            };
            
            TextField textField = new TextField()
            {
                value = Text
            };
            
            textField.AddToClassList("ds-node__textield");
            textField.AddToClassList("ds-node__quote-textfield");
            
            textFoldout.Add(textField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
            
            RefreshExpandedState();
        }
    }
}
