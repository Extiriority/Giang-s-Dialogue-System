/*
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public static class GDSIOUtility : MonoBehaviour
{
    private static GDSGraphView graphView;

    private static string graphFileName;
    private static string containerFolderPath;

    private static List<GDSNode> nodes;
    private static List<GDSGroup> groups;

    private static Dictionary<string, GDSDialogueGroupSO> createdDialogueGroups;
    private static Dictionary<string, GDSDialogueSO> createdDialogues;

    private static Dictionary<string, GDSGroup> loadedGroups;
    private static Dictionary<string, GDSNode> loadedNodes;

    public static void initialize(GDSGraphView dsGraphView, string graphName)
    {
        graphView = dsGraphView;

        graphFileName = graphName;
        containerFolderPath = $"Assets/DialogueSystem/Dialogues/{graphName}";

        nodes = new List<GDSNode>();
        groups = new List<GDSGroup>();

        createdDialogueGroups = new Dictionary<string, GDSDialogueGroupSO>();
        createdDialogues = new Dictionary<string, GDSDialogueSO>();

        loadedGroups = new Dictionary<string, GDSGroup>();
        loadedNodes = new Dictionary<string, GDSNode>();
    }

    #region save methods

    public static void save()
    {
        CreateDefaultFolders();

        GetElementsFromGraphView();

        GGDSGraphSaveDataSO graphData = createAsset<GGDSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", $"{graphFileName}Graph");

        graphData.initialize(graphFileName);

        GDSDialogueContainerSO dialogueContainer = createAsset<GDSDialogueContainerSO>(containerFolderPath, graphFileName);

        dialogueContainer.Initialize(graphFileName);

        SaveGroups(graphData, dialogueContainer);
        SaveNodes(graphData, dialogueContainer);

        saveAsset(graphData);
        saveAsset(dialogueContainer);
    }

    private static void SaveGroups(GGDSGraphSaveDataSO graphData, GDSDialogueContainerSO dialogueContainer)
    {
        List<string> groupNames = new List<string>();

        foreach (GDSGroup group in groups)
        {
            SaveGroupToGraph(group, graphData);
            SaveGroupToScriptableObject(group, dialogueContainer);

            groupNames.Add(group.title);
        }

        UpdateOldGroups(groupNames, graphData);
    }

    private static void SaveGroupToGraph(GDSGroup group, GGDSGraphSaveDataSO graphData)
    {
        GDSGroupSaveData groupData = new GDSGroupSaveData()
        {
            ID = group.ID,
            name = group.title,
            position = group.GetPosition().position
        };

        graphData.Groups.Add(groupData);
    }

    private static void SaveGroupToScriptableObject(GDSGroup group, GDSDialogueContainerSO dialogueContainer)
    {
        string groupName = group.title;

        createFolder($"{containerFolderPath}/Groups", groupName);
        createFolder($"{containerFolderPath}/Groups/{groupName}", "Dialogues");

        GDSDialogueGroupSO dialogueGroup = createAsset<GDSDialogueGroupSO>($"{containerFolderPath}/Groups/{groupName}", groupName);

        dialogueGroup.Initialize(groupName);

        createdDialogueGroups.Add(group.ID, dialogueGroup);

        dialogueContainer.DialogueGroups.Add(dialogueGroup, new List<GDSDialogueSO>());

        saveAsset(dialogueGroup);
    }

    private static void UpdateOldGroups(List<string> currentGroupNames, GDSGraphSaveDataSO graphData)
    {
        if (graphData.OldGroupNames != null && graphData.OldGroupNames.Count != 0)
        {
            List<string> groupsToRemove = graphData.OldGroupNames.Except(currentGroupNames).ToList();

            foreach (string groupToRemove in groupsToRemove)
            {
                removeFolder($"{containerFolderPath}/Groups/{groupToRemove}");
            }
        }

        graphData.OldGroupNames = new List<string>(currentGroupNames);
    }

    private static void SaveNodes(GDSGraphSaveDataSO graphData, GDSDialogueContainerSO dialogueContainer)
    {
        GDSGraphView.SerializableDictionary<string, List<string>> groupedNodeNames = new GDSGraphView.SerializableDictionary<string, List<string>>();
        List<string> ungroupedNodeNames = new List<string>();

        foreach (GDSNode node in nodes)
        {
            saveNodeToGraph(node, graphData);
            SaveNodeToScriptableObject(node, dialogueContainer);

            if (node.group != null)
            {
                groupedNodeNames.addItem(node.group.title, node.dialogueName);

                continue;
            }

            ungroupedNodeNames.Add(node.dialogueName);
        }

        updateDialoguesChoicesConnections();

        UpdateOldGroupedNodes(groupedNodeNames, graphData);
        updateOldUngroupedNodes(ungroupedNodeNames, graphData);
    }

    private static void saveNodeToGraph(GDSNode node, GDSGraphSaveDataSO graphData)
    {
        List<GDSChoiceSaveData> choices = cloneNodeChoices(node.Choices);

        GDSNodeSaveData nodeData = new GDSNodeSaveData()
        {
            ID = node.ID,
            name = node.dialogueName,
            choices = choices,
            text = node.text,
            groupID = node.group?.ID,
            dialogueType = node.dialogueType,
            position = node.GetPosition().position
        };

        graphData.nodes.Add(nodeData);
    }

    private static void SaveNodeToScriptableObject(GDSNode node, GDSDialogueContainerSO dialogueContainer)
    {
        GDSDialogueSO dialogue;

        if (node.group != null)
        {
            dialogue = createAsset<GDSDialogueSO>($"{containerFolderPath}/Groups/{node.group.title}/Dialogues", node.dialogueName);

            dialogueContainer.DialogueGroups.AddItem(createdDialogueGroups[node.group.ID], dialogue);
        }
        else
        {
            dialogue = createAsset<GDSDialogueSO>($"{containerFolderPath}/Global/Dialogues", node.dialogueName);

            dialogueContainer.UngroupedDialogues.Add(dialogue);
        }

        dialogue.Initialize(
            node.dialogueName,
            node.text,
            ConvertNodeChoicesToDialogueChoices(node.choices),
            node.dialogueType,
            node.isStartingNode()
        );

        createdDialogues.Add(node.ID, dialogue);

        saveAsset(dialogue);
    }

    #endregion
    
    private static List<GDSDialogueChoiceData> ConvertNodeChoicesToDialogueChoices(List<GDSChoiceSaveData> nodeChoices)
    {
        List<GDSDialogueChoiceData> dialogueChoices = new List<GDSDialogueChoiceData>();

        foreach (GDSChoiceSaveData nodeChoice in nodeChoices)
        {
            GDSDialogueChoiceData choiceData = new GDSDialogueChoiceData()
            {
                text = nodeChoice.text
            };

            dialogueChoices.Add(choiceData);
        }

        return dialogueChoices;
    }

    private static void updateDialoguesChoicesConnections() {
        foreach (GDSNode node in nodes) {
            GDSDialogueSO dialogue = createdDialogues[node.ID];

            for (int choiceIndex = 0; choiceIndex < node.choices.Count; ++choiceIndex) {
                GDSChoiceSaveData nodeChoice = node.choices[choiceIndex];

                if (string.IsNullOrEmpty(nodeChoice.nodeID)) {
                    continue;
                }

                dialogue.Choices[choiceIndex].NextDialogue = createdDialogues[nodeChoice.nodeID];

                saveAsset(dialogue);
            }
        }
    }

    private static void UpdateOldGroupedNodes(GDSGraphView.SerializableDictionary<string, List<string>> currentGroupedNodeNames, GDSGraphSaveDataSO graphData) {
        if (graphData.oldGroupedNodeNames != null && graphData.oldGroupedNodeNames.Count != 0) {
            foreach (KeyValuePair<string, List<string>> oldGroupedNode in graphData.OldGroupedNodeNames) {
                List<string> nodesToRemove = new List<string>();

                if (currentGroupedNodeNames.TryGetValue(oldGroupedNode.Key, out var nodeName)) {
                    nodesToRemove = oldGroupedNode.Value.Except(nodeName).ToList();
                }

                foreach (string nodeToRemove in nodesToRemove) {
                    removeAsset($"{containerFolderPath}/Groups/{oldGroupedNode.Key}/Dialogues", nodeToRemove);
                }
            }
        }

        graphData.oldGroupedNodeNames = new GDSGraphView.SerializableDictionary<string, List<string>>(currentGroupedNodeNames);
    }

    private static void updateOldUngroupedNodes(List<string> currentUngroupedNodeNames, GDSGraphSaveDataSO graphData) {
        if (graphData.oldUngroupedNodeNames != null && graphData.oldUngroupedNodeNames.Count != 0) {
            List<string> nodesToRemove = graphData.oldUngroupedNodeNames.Except(currentUngroupedNodeNames).ToList();

            foreach (string nodeToRemove in nodesToRemove) {
                removeAsset($"{containerFolderPath}/Global/Dialogues", nodeToRemove);
            }
        }

        graphData.oldUngroupedNodeNames = new List<string>(currentUngroupedNodeNames);
    }

    public static void load() {
        GDSGraphSaveDataSO graphData = loadAsset<GDSGraphSaveDataSO>("Assets/Editor/DialogueSystem/Graphs", graphFileName);

        if (graphData == null) {
            EditorUtility.DisplayDialog(
                "Could not find the file!",
                "The file at the following path could not be found:\n\n" +
                $"\"Assets/Editor/DialogueSystem/Graphs/{graphFileName}\".\n\n" +
                "Make sure you chose the right file and it's placed at the folder path mentioned above.",
                "Thanks!"
            );

            return;
        }

        GDSEditorWindow.updateFileName(graphData.fileName);

        LoadGroups(graphData.groups);
        loadNodes(graphData.nodes);
        LoadNodesConnections();
    }

    private static void LoadGroups(List<GDSGroupSaveData> groups)
    {
        foreach (GDSGroupSaveData groupData in groups)
        {
            GDSGroup group = graphView.createGroup(groupData.name, groupData.position);

            group.ID = groupData.ID;

            loadedGroups.Add(group.ID, group);
        }
    }

    private static void loadNodes(List<GDSNodeSaveData> nodes) {
        foreach (GDSNodeSaveData nodeData in nodes) {
            List<GDSChoiceSaveData> choices = cloneNodeChoices(nodeData.choices);

            GDSNode node = graphView.createNode(nodeData.name, nodeData.dialogueType, nodeData.position, false);

            node.ID = nodeData.ID;
            node.choices = choices;
            node.text = nodeData.text;

            node.Draw();

            graphView.AddElement(node);

            loadedNodes.Add(node.ID, node);

            if (string.IsNullOrEmpty(nodeData.groupID)) {
                continue;
            }

            GDSGroup group = loadedGroups[nodeData.groupID];
            node.group = group;

            group.AddElement(node);
        }
    }

    private static void LoadNodesConnections() {
        foreach (KeyValuePair<string, GDSNode> loadedNode in loadedNodes) {
            foreach (Port choicePort in loadedNode.Value.outputContainer.Children()) {
                GDSChoiceSaveData choiceData = (GDSChoiceSaveData) choicePort.userData;

                if (string.IsNullOrEmpty(choiceData.nodeID)) {
                    continue;
                }

                GDSNode nextNode = loadedNodes[choiceData.nodeID];
                Port nextNodeInputPort = (Port) nextNode.inputContainer.Children().First();
                Edge edge = choicePort.ConnectTo(nextNodeInputPort);

                graphView.AddElement(edge);

                loadedNode.Value.RefreshPorts();
            }
        }
    }

    #region Creation method

    private static void CreateDefaultFolders() {
        createFolder("Assets/Giangs Dialogue System/Editor/Dialogue System", "Graphs");

        createFolder("Assets/Giangs Dialogue System", "DialogueSystem");
        createFolder("Assets/Giangs Dialogue System/DialogueSystem", "Dialogues");

        createFolder("Assets/Giangs Dialogue System/Dialogues", graphFileName);
        createFolder(containerFolderPath, "Global");
        createFolder(containerFolderPath, "Groups");
        createFolder($"{containerFolderPath}/Global", "Dialogues");
    }

    #endregion
    
    private static void GetElementsFromGraphView()
    {
        Type groupType = typeof(GDSGroup);

        graphView.graphElements.ForEach(graphElement => {
            if (graphElement is GDSNode node) {
                nodes.Add(node);

                return;
            }

            if (graphElement.GetType() != groupType) return;
            GDSGroup group = (GDSGroup) graphElement;
            groups.Add(group);

            return;
        });
    }

    #region Utility methods

    public static void createFolder(string parentFolderPath, string newFolderName) {
        if (AssetDatabase.IsValidFolder($"{parentFolderPath}/{newFolderName}")) {
            return;
        }

        AssetDatabase.CreateFolder(parentFolderPath, newFolderName);
    }

    public static void removeFolder(string path) {
        FileUtil.DeleteFileOrDirectory($"{path}.meta");
        FileUtil.DeleteFileOrDirectory($"{path}/");
    }

    public static T createAsset<T>(string path, string assetName) where T : ScriptableObject {
        string fullPath = $"{path}/{assetName}.asset";

        T asset = loadAsset<T>(path, assetName);

        if (asset == null) {
            asset = ScriptableObject.CreateInstance<T>();

            AssetDatabase.CreateAsset(asset, fullPath);
        }

        return asset;
    }

    public static T loadAsset<T>(string path, string assetName) where T : ScriptableObject {
        string fullPath = $"{path}/{assetName}.asset";

        return AssetDatabase.LoadAssetAtPath<T>(fullPath);
    }

    public static void saveAsset(UnityEngine.Object asset) {
        EditorUtility.SetDirty(asset);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void removeAsset(string path, string assetName) {
        AssetDatabase.DeleteAsset($"{path}/{assetName}.asset");
    }

    private static List<GDSChoiceSaveData> cloneNodeChoices(List<GDSChoiceSaveData> nodeChoices) 
        => nodeChoices.Select(choice => new GDSChoiceSaveData() { text = choice.text, nodeID = choice.nodeID }).ToList();

    #endregion
}
*/
