using DialogueSystem.Elements;
using DialogueSystem.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Windows;
    using Enumerations;
    public class DSMultipleChoiceNode : DSNode
    {
        public override void Initialize(string nodeName, Vector2 position , DSGraphView dsGraphView)
        {
            base.Initialize(nodeName, position, dsGraphView);
            
            DialogueType = DSDialogueType.MultipleChoice;
            
            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "New Choice"
            };
            
            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            Button addChoiceButton = DSElementsUtility.CreateButton("Add Choice", () =>
            {
                DSChoiceSaveData choiceData = new DSChoiceSaveData()
                {
                    Text = "New Choice"
                };
                
                Choices.Add(choiceData);
                
                Port port = CreateChoicePort(choiceData);
                
                outputContainer.Add(port);
            });
            
            addChoiceButton.AddToClassList("ds-node__button");
            
            mainContainer.Insert(1, addChoiceButton);
            
            foreach (DSChoiceSaveData choice in Choices)
            {
                Port choicePort = CreateChoicePort(choice);
                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }

        #region Elements Creation
        private Port CreateChoicePort(object userData)
        {
            Port choicePort = this.CreatePort();
            
            choicePort.userData = userData;
            
            DSChoiceSaveData choiceData = userData as DSChoiceSaveData;

            Button deleteChoiceButton = DSElementsUtility.CreateButton("X", () =>
            {
                if(Choices.Count == 1) return;

                if (choicePort.connected)
                {
                    graphView.DeleteElements(choicePort.connections);
                }
                
                Choices.Remove(choiceData);
                
                graphView.RemoveElement(choicePort);
            });
                
            deleteChoiceButton.AddToClassList("ds-node__button");
                
            TextField choiceTextField = DSElementsUtility.CreateTextField(choiceData.Text, null, callback =>
            {
                choiceData.Text = callback.newValue;
            });
            
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
