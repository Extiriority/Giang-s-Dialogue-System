using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GDSDialogueContainerSO : ScriptableObject
{
    [field: SerializeField] public string fileName { get; set; }
    [field: SerializeField] public SerializableDictionary<GDSDialogueGroupSO, List<GDSDialogueSO>> dialogueGroups { get; set; }
    [field: SerializeField] public List<GDSDialogueSO> ungroupedDialogues { get; set; }

    public void initialize(string fileName) {
        this.fileName = fileName;

        dialogueGroups = new SerializableDictionary<GDSDialogueGroupSO, List<GDSDialogueSO>>();
        ungroupedDialogues = new List<GDSDialogueSO>();
    }

    public List<string> getDialogueGroupNames() => dialogueGroups.Keys.Select(dialogueGroup => dialogueGroup.groupName).ToList();
    

    public List<string> getGroupedDialogueNames(GDSDialogueGroupSO dialogueGroup, bool startingDialoguesOnly) {
        List<GDSDialogueSO> groupedDialogues = dialogueGroups[dialogueGroup];

        return (from groupedDialogue in groupedDialogues where !startingDialoguesOnly || groupedDialogue.isStartingDialogue select groupedDialogue.dialogueName).ToList();
    }

    public List<string> getUngroupedDialogueNames(bool startingDialoguesOnly) =>
         (from ungroupedDialogue in ungroupedDialogues where !startingDialoguesOnly || ungroupedDialogue.isStartingDialogue select ungroupedDialogue.dialogueName).ToList();
    
}
