using System;
using UnityEngine;

[Serializable]
public class GDSGroupSaveData {
    [field: SerializeField] public string ID { get; set; }
    [field: SerializeField] public string name { get; set; }
    [field: SerializeField] public Vector2 position { get; set; }
}
