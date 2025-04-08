using DialogueSystem.Elements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Enumerations;
    public class DSMultipleChoiceNode : DSNode
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            
            DialogueType = DSDialogueType.MultipleChoice;
            
            Choices.Add("New Choice");
        }

        public override void Draw()
        {
            base.Draw();
            
            Button addChoiceButton = new Button()
            {
                text = "Add Choice"
            };
            
            addChoiceButton.AddToClassList("ds-node__button");
            
            mainContainer.Insert(1, addChoiceButton);
            
            foreach (var choice in Choices)
            {
                Port choicePort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

                choicePort.portName = "";
                
                Button deleteChoiceButton = new Button()
                {
                    text = "X"
                };
                
                addChoiceButton.AddToClassList("ds-node__button");
                
                TextField choiceTextField = new TextField()
                {
                    value = choice
                };
                
                choiceTextField.AddToClassList("ds-node__textfield");
                choiceTextField.AddToClassList("ds-node__choice-textfield");
                choiceTextField.AddToClassList("ds-node__textfield__hidden");
                
                choicePort.Add(choiceTextField);
                choicePort.Add(deleteChoiceButton);
                
                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }
    }
}
