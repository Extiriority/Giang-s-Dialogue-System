using System;
using UnityEngine;

[Serializable]
public class GDSChoiceSaveData {
    [field: SerializeField] public string text { get; set; }
    [field: SerializeField] public string nodeID { get; set; }
}
