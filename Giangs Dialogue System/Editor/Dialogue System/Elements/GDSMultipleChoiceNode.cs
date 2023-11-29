using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GDSMultipleChoiceNode : GDSNode {
    public override void initialize(GDSGraphView gdsGraphView, Vector2 position) {
        base.initialize(gdsGraphView, position);

        dialogueType = GDSDialogueType.MultipleChoice;

        GDSChoiceSaveData choiceData = new GDSChoiceSaveData
        {
            text = "New choice"
        };
        
        choices.Add(choiceData);
    }

    public override void Draw() {
        base.Draw();

        //Main container
        Button addChoice = GDSElementUtility.createButton("Add choice", () => {
            GDSChoiceSaveData choiceData = new GDSChoiceSaveData
            {
                text = "New choice"
            };
            choices.Add(choiceData);
            
            Port choicePort = createChoicePort(choiceData); 
            
            outputContainer.Add(choicePort);
        });
        
        addChoice.AddToClassList("gds-node__button");
        
        mainContainer.Insert(1, addChoice);
        
        //output
        foreach (var choicePort in choices.Select(createChoicePort)) {
            outputContainer.Add(choicePort);
        }
        RefreshExpandedState();
    }

    private Port createChoicePort(object userData) {
        Port choicePort = this.createPort();

        choicePort.userData = userData;
        GDSChoiceSaveData choiceData = (GDSChoiceSaveData) userData;

        Button delete = GDSElementUtility.createButton("X", () => {
            if (choices.Count == 1) 
                return;
            
            if (choicePort.connected) {
                graphView.DeleteElements(choicePort.connections);
            }

            choices.Remove(choiceData);

            graphView.RemoveElement(choicePort);
        });
            
        delete.AddToClassList("gds-node__button");
            
        TextField textField = GDSElementUtility.createTextField(choiceData.text, null, callback => {
            choiceData.text = callback.newValue;
        });

        textField.addClasses(
            "gds-node__text-field",
            "gds-node__choice-text-field",
            "gds-node__text-field__hidden"
            );
            
        choicePort.Add(textField);
        choicePort.Add(delete);
        
        return choicePort;
    }
}
