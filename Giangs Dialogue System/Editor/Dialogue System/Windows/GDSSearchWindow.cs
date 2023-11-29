using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GDSSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private GDSGraphView graphView;
    private Texture2D indentationIcon;
    
    public void initialize(GDSGraphView gdsGraphView) {
        graphView = gdsGraphView;
        
        indentationIcon = new Texture2D(1, 1);
        indentationIcon.SetPixel(0, 0, Color.clear);
        indentationIcon.Apply();
    }
    
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {
        
        List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry> {
            new SearchTreeGroupEntry(new GUIContent("Create Element")),
            new SearchTreeGroupEntry(new GUIContent("Dialogue Node"), 1),
            new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon)) {
                level = 2,
                userData = GDSDialogueType.SingleChoice
            },
            new SearchTreeEntry(new GUIContent("Multiple Choice", indentationIcon)) {
                level = 2,
                userData = GDSDialogueType.MultipleChoice
            },
            new SearchTreeGroupEntry(new GUIContent("Dialogue Group"), 1),
            new SearchTreeEntry(new GUIContent("Single Group", indentationIcon)) {
                level = 2,
                userData = new Group()
            }
        };

        return searchTreeEntries;
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
        Vector2 localMousePosition = graphView.getLocalMousePosition(context.screenMousePosition, true);
        
        switch (searchTreeEntry.userData) {
            case GDSDialogueType.SingleChoice:
                GDSSingleChoiceNode singleChoiceNode = (GDSSingleChoiceNode) graphView.createNode(GDSDialogueType.SingleChoice, localMousePosition);
                graphView.AddElement(singleChoiceNode);
                return true;
            case GDSDialogueType.MultipleChoice:
                GDSMultipleChoiceNode multipleChoiceNode = (GDSMultipleChoiceNode) graphView.createNode(GDSDialogueType.MultipleChoice, localMousePosition);
                graphView.AddElement(multipleChoiceNode);
                return true;
            case GDSGroup _:
                graphView.createGroup("DialogueGroup", localMousePosition);
                return true;
            default:
                return false;
        }
    }
}
