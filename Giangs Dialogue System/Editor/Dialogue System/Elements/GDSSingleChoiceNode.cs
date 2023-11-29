using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GDSSingleChoiceNode : GDSNode {
    public override void initialize(GDSGraphView gdsGraphView, Vector2 position) {
        base.initialize(gdsGraphView, position);

        dialogueType = GDSDialogueType.SingleChoice;

        GDSChoiceSaveData choiceData = new GDSChoiceSaveData() {
            text = "Next dialogue"
        };
        
        choices.Add(choiceData);
    }

    public override void Draw() {   
        base.Draw();

        //output
        foreach (GDSChoiceSaveData choice in choices) {
            Port choicePort = this.createPort(choice.text);
            
            outputContainer.Add(choicePort);
            
            RefreshExpandedState();
        }
    }
}
