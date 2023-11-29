using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GDSNode : Node {
    public string ID { get; set; }
    public string dialogueName { get; set; }
    public List<GDSChoiceSaveData> choices { get; set; }
    public string text { get; set; }
    public GDSDialogueType dialogueType { get; set; }
    public GDSGroup group { get; set; }

    protected GDSGraphView graphView;
    private Color defaultBackgroundColor;

    public virtual void initialize(GDSGraphView gdsGraphView, Vector2 position) {
        ID = Guid.NewGuid().ToString();
        dialogueName = "Dialogue title";
        choices = new List<GDSChoiceSaveData>();
        text = "Dialogue text.";

        graphView = gdsGraphView;
        defaultBackgroundColor = new Color(29f / 255f, 29 / 255f, 30 / 255f);
        
        SetPosition(new Rect(position, Vector2.zero));
        
        mainContainer.AddToClassList("gds-node__main-container");
        extensionContainer.AddToClassList("gds-node__extension-container");
    }

    #region Overrided methods

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
        evt.menu.AppendAction("Disconnect Input Ports", actionEvent => disconnectInputPorts());
        evt.menu.AppendAction("Disconnect Output Ports", actionEvent => disconnectOutputPorts());
        
        base.BuildContextualMenu(evt);
    }
    
    #endregion
    
    public virtual void Draw() {
        //Title
        var dialogueNameTextField = GDSElementUtility.createTextField(dialogueName, null, callback =>
        {
            if (group == null) {
                graphView.removeUngroupedNode(this);
                dialogueName = callback.newValue;
                graphView.addUngroupedNode(this);
                
                return;
            }

            GDSGroup currentGroup = group;
            
            graphView.removeGroupedNode(this, group);
            dialogueName = callback.newValue;
            graphView.addGroupedNode(this, currentGroup);
        });
        dialogueNameTextField.addClasses(
            "gds-node__text-field",
            "gds-node__filename-text-field", 
            "gds-node__text-field__hidden" 
            );
        
        titleContainer.Insert(0, dialogueNameTextField);

        //Input
        Port inputPort = this.createPort("In", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "In";
        inputContainer.Add(inputPort);
        
        //Container Dialogue
        var customDataContainer = new VisualElement();
        customDataContainer.AddToClassList("gds-node__custom-data-container");

        var foldout = GDSElementUtility.createFoldout("Dialogue");
        var textField = GDSElementUtility.createTextArea(text);

        textField.addClasses(
            "gds-node__text-field",
            "gds-node__quote-text-field"
        );
        
        foldout.Add(textField);
        customDataContainer.Add(foldout);
        extensionContainer.Add(customDataContainer);
    }

    #region Utility methods
    
    public void disconnectAllPorts() {
        disconnectInputPorts();
        disconnectOutputPorts();
    }

    private void disconnectInputPorts() => disconnectPorts(inputContainer);
    private void disconnectOutputPorts() => disconnectPorts(outputContainer);
    

    private void disconnectPorts(VisualElement container) {
        foreach (var visualElement in container.Children()) {
            var port = (Port)visualElement;
            if (!port.connected) {
                continue;
            }

            graphView.DeleteElements(port.connections);
        }
    }
    
    public void setErrorStyle(Color color)
    {
        mainContainer.style.backgroundColor = color;
    }

    public void resetStyle()
    {
        mainContainer.style.backgroundColor = defaultBackgroundColor;
    }

    #endregion
}
