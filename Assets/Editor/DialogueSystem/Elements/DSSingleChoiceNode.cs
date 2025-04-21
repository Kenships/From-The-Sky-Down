using DialogueSystem.Elements;
using DialogueSystem.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Windows;
    using Enumerations;
    public class DSSingleChoiceNode : DSNode
    {
        public override void Initialize(Vector2 position, DSGraphView dsGraphView)
        {
            base.Initialize(position, dsGraphView);
            
            DialogueType = DSDialogueType.SingleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "Next Dialogue"
            };
            
            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            foreach (var choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);
                
                choicePort.userData = choice;
                
                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }
    }
}
