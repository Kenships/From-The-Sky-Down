using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Windows
{
    using Elements;
    using Enumerations;
    public class DSSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DSGraphView graphView;
        private Texture2D indentationIcon;
        public void Initialize(DSGraphView dsGraphView)
        {
            graphView = dsGraphView;
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0,0, Color.clear);
            indentationIcon.Apply();
        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
                new(new GUIContent("Single Choice", indentationIcon))
                {
                    level = 2,
                    userData = DSDialogueType.SingleChoice
                },
                new(new GUIContent("Multiple Choice", indentationIcon))
                {
                    level = 2,
                    userData = DSDialogueType.MultipleChoice
                },
                new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
                new(new GUIContent("Single Group", indentationIcon))
                {
                    level = 2,
                    userData = new Group()
                }
            };

            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 mousePosition = graphView.GetLocalMousePosition(context.screenMousePosition, true);
            switch (SearchTreeEntry.userData)
            {
                case DSDialogueType.SingleChoice:
                {
                    DSSingleChoiceNode singleChoiceNode = graphView.CreateNode(DSDialogueType.SingleChoice, mousePosition) as DSSingleChoiceNode;
                    
                    graphView.AddElement(singleChoiceNode);
                    
                    break;
                } 
                case DSDialogueType.MultipleChoice:
                {
                    DSMultipleChoiceNode multipleChoiceNode = graphView.CreateNode(DSDialogueType.MultipleChoice, mousePosition) as DSMultipleChoiceNode;
                    
                    graphView.AddElement(multipleChoiceNode);                   
                    
                    break;
                } 
                case Group _:
                {
                    DSGroup group = graphView.CreateGroup("DialogueGroup", mousePosition);
                    
                    graphView.AddElement(group);
                    
                    break;
                }
                default:
                {
                    return false;
                }
            }

            return true;
        }
    }
}
