using System;
using UnityEngine;

[Serializable]
public class GDSDialogueChoiceData {
    [field: SerializeField] public string text { get; set; }
    [field: SerializeField] public GDSDialogueSO nextDialogue { get; set; }
}
