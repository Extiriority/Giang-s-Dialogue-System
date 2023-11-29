using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GDSGraphSaveDataSO {
    [field: SerializeField] public string fileName { get; set; }
    [field: SerializeField] public List<GDSGroupSaveData> groups { get; set; }
    [field: SerializeField] public List<GDSNodeSaveData> nodes { get; set; }
    [field: SerializeField] public List<string> oldGroupNames { get; set; }
    [field: SerializeField] public List<string> oldUngroupedNodeNames { get; set; }
    [field: SerializeField] public GDSGraphView.SerializableDictionary<string, List<string>> oldGroupedNodeNames { get; set; }

    public void initialize(string fileName) {
        this.fileName = fileName;

        groups = new List<GDSGroupSaveData>();
        nodes = new List<GDSNodeSaveData>();
    }
}
