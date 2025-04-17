using DialogueSystem.Elements;
using DialogueSystem.Utilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Elements
{
    using Enumerations;
    public class DSSingleChoiceNode : DSNode
    {
        public override void Initialize(Vector2 position)
        {
            base.Initialize(position);
            
            DialogueType = DSDialogueType.SingleChoice;
            
            Choices.Add("Next Dialogue");
        }

        public override void Draw()
        {
            base.Draw();

            foreach (var choice in Choices)
            {
                Port choicePort = this.CreatePort(choice);
                
                outputContainer.Add(choicePort);
            }
            
            RefreshExpandedState();
        }
    }
}
