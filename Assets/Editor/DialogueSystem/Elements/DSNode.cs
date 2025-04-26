using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Enumerations;
    using Utilities;
    using Windows;
    using Data.Save;
    public class DSNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public DSGroup Group { get; set; }
        
        protected DSGraphView graphView;
        
        private Color defaultBackgroundColor;
        
        public virtual void Initialize(string nodeName, Vector2 position, DSGraphView dsGraphView)
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = "Dialogue Text.";
            
            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);
            
            SetPosition(new Rect(position, Vector2.zero));
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }
        #region Overrided Methods
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (HasConnectedInputPorts())
            {
                evt.menu.AppendAction("Disconnect Input Ports", action => DisconnectInputPorts());
            }
            else
            {
                evt.menu.AppendAction("Disconnect Input Ports", action => DisconnectInputPorts(), DropdownMenuAction.Status.Disabled);
            }
            if (HasConnectedOutputPorts())
            {
                evt.menu.AppendAction("Disconnect Output Ports", action => DisconnectOutputPorts());
            }
            else
            {
                evt.menu.AppendAction("Disconnect Output Ports", action => DisconnectOutputPorts(), DropdownMenuAction.Status.Disabled);
            }
            
            base.BuildContextualMenu(evt);
        }
        #endregion
        public virtual void Draw()
        {
            TextField dialogueNameTextField = DSElementsUtility.CreateTextField(DialogueName, null, callback => 
            {
                TextField target = callback.target as TextField;

                target.value = callback.newValue.RemoveWhiteSpaces().RemoveSpecialCharacters();
                
                if(string.IsNullOrEmpty(target.value))
                {
                    if (!string.IsNullOrEmpty(DialogueName))
                    {
                        ++graphView.NamesErrorCount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(DialogueName))
                    {
                        --graphView.NamesErrorCount;
                    }
                }
                
                if (Group == null)
                {
                    graphView.RemoveUngroupedNode(this);
                
                    DialogueName = target.value;
                
                    graphView.AddUngroupedNode(this);
                    
                    return;
                }
                
                DSGroup currentGroup = Group;
                
                graphView.RemoveGroupedNode(this, Group);
                
                DialogueName = target.value;
                
                graphView.AddGroupedNode(this, currentGroup);
            });
        
        dialogueNameTextField.AddClasses(
                "ds-node__textfield",
                "ds-node__textfield__hidden", 
                "ds-node__filename-textfield");
            
            titleContainer.Insert(0, dialogueNameTextField);
        
            Port inputPort = this.CreatePort("Dialogue Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            
            inputContainer.Add(inputPort);
            
            VisualElement customDataContainer = new VisualElement();
            
            customDataContainer.AddClasses(
                "ds-node__custom-data-container");
            
            Foldout textFoldout = DSElementsUtility.CreateFoldout("Dialogue Text");
            
            TextField textField = DSElementsUtility.CreateTextArea(Text, null, callback =>
            {
                Text = callback.newValue;
            });
            
            textField.AddClasses(
                "ds-node__textfield", 
                "ds-node__quote-textfield");
            
            textFoldout.Add(textField);
            customDataContainer.Add(textFoldout);
            extensionContainer.Add(customDataContainer);
            
            RefreshExpandedState();
        }
        #region Utility Methods

        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }
        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }
        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (port.connected)
                {
                    graphView.DeleteElements(port.connections);
                }
            }
        }

        private bool HasConnectedInputPorts()
        {
            return HasConnectedPorts(inputContainer);
        }
        private bool HasConnectedOutputPorts()
        {
            return HasConnectedPorts(outputContainer);
        }

        private bool HasConnectedPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (port.connected) return true;
            }
            
            return false;
        }

        public bool IsStartNode()
        {
            Port inputport = inputContainer.Children().First() as Port;

            return !inputport.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }
        #endregion
    }
}
