using System;
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
            
            mainContainer.Insert(3, addChoiceButton);
            
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
            
            TextField choiceWeightingTextField = DSElementsUtility.CreateTextField(choiceData.Weighting.ToString(), null, callback =>
            {
                string firstDigit = callback.newValue.OnlyFirstDigit();
                
                TextField target = callback.target as TextField;
                target.value = firstDigit;

                if (!string.IsNullOrEmpty(firstDigit))
                {
                    choiceData.Weighting = int.Parse(firstDigit);
                    if (choiceData.Weighting > 0)
                    {
                        target.style.borderRightWidth = 4;
                        target.style.borderRightColor = new StyleColor(ColorSlider(choiceData.Weighting));
                    }
                    else
                    {
                        target.style.borderRightWidth = 0;
                        target.style.borderRightColor = new StyleColor(Color.clear);
                    }
                    
                }
            });

            choiceWeightingTextField.maxLength = 1;
            if (choiceData.Weighting > 0)
            {
                choiceWeightingTextField.style.borderRightWidth = 4;
                choiceWeightingTextField.style.borderRightColor = new StyleColor(ColorSlider(choiceData.Weighting));
            }
            else
            {
                choiceWeightingTextField.style.borderRightWidth = 0;
                choiceWeightingTextField.style.borderRightColor = new StyleColor(ColorSlider(choiceData.Weighting));
            }
            
            choiceTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__choice-textfield",
                "ds-node__textfield__hidden"
            );
            
            choicePort.Add(choiceWeightingTextField);    
            choicePort.Add(choiceTextField);
            choicePort.Add(deleteChoiceButton);
            

            return choicePort;
        }
        #endregion
        
        private static Color32 ColorSlider(int value)
        {
            switch (value - 1)
            {
                
                case 0:
                    return new Color32(0,156,26, 255);
                case 1:
                    return new Color32(100,227,95, 255);
                case 2:
                    return new Color32(180,255,180, 255);
                case 3:
                    return new Color32(205,181,255, 255);
                case 4:
                    return new Color32(167,150,232, 255);
                case 5:
                    return new Color32(137,109,235, 255);
                case 6:
                    return new Color32(255,193,0, 255);
                case 7:
                    return new Color32(255,154,0, 255);
                case 8:
                    return new Color32(255,0,0, 255);
                default:
                    return new Color(0, 0, 0);
            }
            
        }
    }
}
