using DialogueSystem.Elements;
using DialogueSystem.Utilities;
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

            Button addChoiceButton = DSElementsUtility.CreateButton("Add Choice", () =>
            {
                Port port = CreateChoicePort("New Choice");
                Choices.Add("New Choice");
                outputContainer.Add(port);
            });
            
            addChoiceButton.AddToClassList("ds-node__button");
            
            mainContainer.Insert(1, addChoiceButton);
            
            foreach (var choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }

        #region Elements Creation
        private Port CreateChoicePort(string choice)
        {
            Port choicePort = this.CreatePort();

            Button deleteChoiceButton = DSElementsUtility.CreateButton("X");
                
            deleteChoiceButton.AddToClassList("ds-node__button");
                
            TextField choiceTextField = DSElementsUtility.CreateTextField(choice);
            
            choiceTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__textfield__hidden");
                
            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);

            return choicePort;
        }
        #endregion
    }
}
