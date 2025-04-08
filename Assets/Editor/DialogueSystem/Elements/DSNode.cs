using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

        public void Initialize()
        {
            DialogueName = "DialogueName";
            Choices = new List<string>();
            Text = "Dialogue Text.";
        }

        public void Draw()
        {
            TextField dialogueNameTextField = new TextField()
            {
                value = DialogueName
            };
            titleContainer.Insert(0, dialogueNameTextField);

            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            
            inputPort.portName = "Dialogue Connection";
            
            inputContainer.Add(inputPort);
            
            VisualElement customDataContainer = new VisualElement();
            
            Foldout textFoldout = new Foldout()
            {
                text = "Dialogue Text"
            };
            
            TextField textField = new TextField()
            {
                value = Text
            };
            
            textFoldout.Add(textField);
            
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
            
        }
    }
}
