using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GDSNodeSaveData {
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public string name { get; set; }
    [field: SerializeField] public string text { get; set; }
    [field: SerializeField] public List<GDSChoiceSaveData> choices { get; set; }
    [field: SerializeField] public string groupID { get; set; }
    [field: SerializeField] public GDSDialogueType dialogueType { get; set; }
    [field: SerializeField] public Vector2 position { get; set; }
}
