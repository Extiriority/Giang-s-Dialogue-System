using System.Collections.Generic;
using UnityEngine;

public class GDSDialogueSO : ScriptableObject {
    [field: SerializeField] public string dialogueName { get; set; }
    [field: SerializeField] [field: TextArea] public string text { get; set; }
    [field: SerializeField] public List<GDSDialogueChoiceData> choices { get; set; }
    [field: SerializeField] public GDSDialogueType dialogueType { get; set; }
    [field: SerializeField] public bool isStartingDialogue { get; set; }

    public void initialize(string dialogueName, string text, List<GDSDialogueChoiceData> choices, GDSDialogueType dialogueType, bool isStartingDialogue)
    {
        this.dialogueName = dialogueName;
        this.text = text;
        this.choices = choices;
        this.dialogueType = dialogueType;
        this.isStartingDialogue = isStartingDialogue;
    }
}
