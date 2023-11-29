using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GDSDialogueGroupSO : ScriptableObject {
    [field: SerializeField] public string groupName { get; set; }

    public void initialize(string groupName) {
        this.groupName = groupName;
    }
}
