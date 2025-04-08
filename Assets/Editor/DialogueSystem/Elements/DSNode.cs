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
        }

        public virtual void Draw()
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
            
            RefreshExpandedState();
        }
    }
}
