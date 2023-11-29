using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GDSGraphView : GraphView
{
    private GDSEditorWindow editorWindow;
    private GDSSearchWindow searchWindow;

    private SerializableDictionary<string, GDSNodeErrorData> ungroupedNodes;
    private SerializableDictionary<string, GDSGroupErrorData> groups;
    private SerializableDictionary<Group, SerializableDictionary<string, GDSNodeErrorData>> groupedNodes;

    private int nameErrorsAmount;

    public int NameErrorsAmount
    {
        get => nameErrorsAmount;

        set {
            nameErrorsAmount = value;

            switch (nameErrorsAmount) {
                case 0:
                    editorWindow.enableSaving();
                    break;
                case 1:
                    editorWindow.disableSaving();
                    break;
            }
        }
    }

    
    public GDSGraphView(GDSEditorWindow gdsEditorWindow)
    {
        editorWindow = gdsEditorWindow;

        ungroupedNodes = new SerializableDictionary<string, GDSNodeErrorData>();
        groups = new SerializableDictionary<string, GDSGroupErrorData>();
        groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, GDSNodeErrorData>>();
        
        addManipulators();
        addSearchWindow();
        addGridBackground();
        
        onElementsDeleted();
        onGroupElementsAdded();
        onGroupElementsRemoved();
        onGroupRenamed();
        onGraphViewChanged();
        
        addStyles();
    }

    #region Overrided Methods

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach(port =>
        {
            if (startPort == port) return;
            if (startPort.node == port.node) return;
            if (startPort.direction == port.direction) return;

            compatiblePorts.Add(port);
        });

        return compatiblePorts;
    }

    #endregion

    #region Manipulators

    private void addManipulators()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        this.AddManipulator(createNodeContextualMenu("Add node (Single Choice)", GDSDialogueType.SingleChoice));
        this.AddManipulator(createNodeContextualMenu("Add node (Multiple Choice)", GDSDialogueType.MultipleChoice));

        this.AddManipulator(createGroupContextualMenu());
    }

    private IManipulator createGroupContextualMenu()
    {
        var contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Add Group",
                actionEvent => createGroup("Dialogue group title",
                    getLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
        );

        return contextualMenuManipulator;
    }
    
    private IManipulator createNodeContextualMenu(string actionTitle, GDSDialogueType dialogueType)
    {
        var contextualMenuManipulator = new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction(actionTitle,
                actionEvent => AddElement(createNode(dialogueType,
                    getLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
        );

        return contextualMenuManipulator;
    }

    #endregion

    #region Elements creation

    public void createGroup(string title, Vector2 localMousePosition) {
        var group = new GDSGroup(title, localMousePosition);

        addGroup(group);
        AddElement(group);

        foreach (var node in selection.OfType<GDSNode>()) {
            group.AddElement(node);
        }
    }

    public GDSNode createNode(GDSDialogueType dialogueType, Vector2 position)
    {
        GDSNode node = dialogueType switch
        {
            GDSDialogueType.SingleChoice => new GDSSingleChoiceNode(),
            GDSDialogueType.MultipleChoice => new GDSMultipleChoiceNode(),
            _ => throw new ArgumentOutOfRangeException(nameof(dialogueType), dialogueType, null)
        };

        node.initialize(this, position);
        node.Draw();

        addUngroupedNode(node);

        return node;
    }
    
    #endregion

    #region Repeated Elements
    
    public void addUngroupedNode(GDSNode node) {
        string nodeName = node.dialogueName;

        if (!ungroupedNodes.ContainsKey(nodeName)) {
            GDSNodeErrorData nodeErrorData = new GDSNodeErrorData();

            nodeErrorData.nodes.Add(node);
            ungroupedNodes.Add(nodeName, nodeErrorData);

            return;
        }

        List<GDSNode> ungroupedNodesList = ungroupedNodes[nodeName].nodes;

        ungroupedNodesList.Add(node);

        Color errorColor = ungroupedNodes[nodeName].errorData.Color;

        node.setErrorStyle(errorColor);

        if (ungroupedNodesList.Count != 2) return;
        ++NameErrorsAmount;

        ungroupedNodesList[0].setErrorStyle(errorColor);
    }
    
    public void removeUngroupedNode(GDSNode node) {
        string nodeName = node.dialogueName;

        List<GDSNode> ungroupedNodesList = ungroupedNodes[nodeName].nodes;

        ungroupedNodesList.Remove(node);

        node.resetStyle();

        switch (ungroupedNodesList.Count) {
            case 1:
                --NameErrorsAmount;
                ungroupedNodesList[0].resetStyle();
                return;
            case 0:
                ungroupedNodes.Remove(nodeName);
                break;
        }
    }
    
    private void addGroup(GDSGroup group) {
        string groupName = group.title;

        if (!groups.ContainsKey(groupName)) {
            GDSGroupErrorData groupErrorData = new GDSGroupErrorData();

            groupErrorData.groups.Add(group);
            groups.Add(groupName, groupErrorData);

            return;
        }

        List<GDSGroup> groupsList = groups[groupName].groups;
        groupsList.Add(group);

        Color errorColor = groups[groupName].errorData.Color;
        group.setErrorStyle(errorColor);

        if (groupsList.Count != 2) return;
        ++NameErrorsAmount;
        groupsList[0].setErrorStyle(errorColor);
    }
    
    public void addGroupedNode(GDSNode node, GDSGroup group)
    {
        string nodeName = node.dialogueName;

        node.group = group;

        if (!groupedNodes.ContainsKey(group)) {
            groupedNodes.Add(group, new SerializableDictionary<string, GDSNodeErrorData>());
        }

        if (!groupedNodes[group].ContainsKey(nodeName)) {
            GDSNodeErrorData nodeErrorData = new GDSNodeErrorData();

            nodeErrorData.nodes.Add(node);
            groupedNodes[group].Add(nodeName, nodeErrorData);

            return;
        }

        List<GDSNode> groupedNodesList = groupedNodes[group][nodeName].nodes;

        groupedNodesList.Add(node);

        Color errorColor = groupedNodes[group][nodeName].errorData.Color;

        node.setErrorStyle(errorColor);

        if (groupedNodesList.Count != 2) return;
        ++NameErrorsAmount;
        groupedNodesList[0].setErrorStyle(errorColor);
    }
    
    private void removeGroup(GDSGroup group) {
        string oldGroupName = group.oldTitle;

        List<GDSGroup> groupsList = groups[oldGroupName].groups;

        groupsList.Remove(group);
        group.resetStyle();

        switch (groupsList.Count) {
            case 1:
                --NameErrorsAmount;
                groupsList[0].resetStyle();
                return;
            case 0:
                groups.Remove(oldGroupName);
                break;
        }
    }
    
    public void removeGroupedNode(GDSNode node, Group group)
    {
        string nodeName = node.dialogueName;

        node.group = null;

        List<GDSNode> groupedNodesList = groupedNodes[group][nodeName].nodes;

        groupedNodesList.Remove(node);

        node.resetStyle();

        switch (groupedNodesList.Count) {
            case 1:
                --NameErrorsAmount;
                groupedNodesList[0].resetStyle();
                return;
            case 0:
                groupedNodes[group].Remove(nodeName);
                if (groupedNodes[group].Count == 0) {
                    groupedNodes.Remove(group);
                }
                break;
        }
    }
    
    #endregion

    #region Callbacks

    private void onElementsDeleted() {
        deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(GDSGroup);
                Type edgeType = typeof(Edge);

                List<GDSGroup> groupsToDelete = new List<GDSGroup>();
                List<GDSNode> nodesToDelete = new List<GDSNode>();
                List<Edge> edgesToDelete = new List<Edge>();

                foreach (var selectedElement in selection.Cast<GraphElement>()) {
                    if (selectedElement is GDSNode node) {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if (selectedElement.GetType() == edgeType) {
                        Edge edge = (Edge) selectedElement;

                        edgesToDelete.Add(edge);
                        continue;
                    }

                    if (selectedElement.GetType() != groupType) {
                        continue;
                    }

                    GDSGroup group = (GDSGroup) selectedElement;

                    groupsToDelete.Add(group);
                }

                foreach (GDSGroup groupToDelete in groupsToDelete) {
                    List<GDSNode> groupNodes = groupToDelete.containedElements.OfType<GDSNode>().ToList();

                    groupToDelete.RemoveElements(groupNodes);
                    removeGroup(groupToDelete);
                    RemoveElement(groupToDelete);
                }

                DeleteElements(edgesToDelete);

                foreach (GDSNode nodeToDelete in nodesToDelete) {
                    nodeToDelete.group?.RemoveElement(nodeToDelete);
                    removeUngroupedNode(nodeToDelete);
                    nodeToDelete.disconnectAllPorts();
                    RemoveElement(nodeToDelete);
                }
            };
    }

    private void onGroupElementsAdded() {
        elementsAddedToGroup = (group, elements) => {
            foreach (GraphElement element in elements) {
                if (!(element is GDSNode node)) {
                    continue;
                }

                GDSGroup nodeGroup = (GDSGroup) group;

                removeUngroupedNode(node);
                addGroupedNode(node, nodeGroup);
            }
        };
    }
    
    private void onGroupElementsRemoved()
    {
        elementsRemovedFromGroup = (group, elements) =>
        {
            foreach (GraphElement element in elements)
            {
                if (!(element is GDSNode))
                {
                    continue;
                }
                
                GDSNode node = (GDSNode) element;

                removeGroupedNode(node, group);
                addUngroupedNode(node);
            }
        };
    }
    
    private void onGroupRenamed() {
        groupTitleChanged = (group, newTitle) => {
            GDSGroup gdsGroup = (GDSGroup) group;

            gdsGroup.title = newTitle;

            if (string.IsNullOrEmpty(gdsGroup.title)) {
                if (!string.IsNullOrEmpty(gdsGroup.oldTitle)) {
                    ++NameErrorsAmount;
                }
            }
            else {
                if (string.IsNullOrEmpty(gdsGroup.oldTitle)) {
                    --NameErrorsAmount;
                }
            }

            removeGroup(gdsGroup);
            gdsGroup.oldTitle = gdsGroup.title;
            addGroup(gdsGroup);
        };
    }
    
    private void onGraphViewChanged() {
        graphViewChanged = (changes) => {
            if (changes.edgesToCreate != null) {
                foreach (Edge edge in changes.edgesToCreate) {
                    GDSNode nextNode = (GDSNode) edge.input.node;
                    GDSChoiceSaveData choiceData = (GDSChoiceSaveData) edge.output.userData;

                    choiceData.nodeID = nextNode.ID;
                }
            }

            if (changes.elementsToRemove != null) {
                Type edgeType = typeof(Edge);

                foreach (var choiceData in from element in changes.elementsToRemove where element.GetType() == edgeType select (Edge) element into edge select (GDSChoiceSaveData) edge.output.userData) {
                    choiceData.nodeID = "";
                }
            }

            return changes;
        };
    }

    #endregion

    #region Styles/UI implementation

    private void addSearchWindow()
    {
        if (searchWindow == null)
        {
            searchWindow = ScriptableObject.CreateInstance<GDSSearchWindow>();
            searchWindow.initialize(this);
        }

        nodeCreationRequest = context =>
            SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
    }

    private void addGridBackground()
    {
        var gridBackground = new GridBackground();
        gridBackground.StretchToParentSize();
        Insert(0, gridBackground);
    }


    private void addStyles()
    {
        this.addStyleSheets(
            "Assets/Giangs Dialogue System/EditorUSS/GDSGraphViewStyles.uss",
            "Assets/Giangs Dialogue System/EditorUSS/GDSNodeStyles.uss"
        );
    }

    #endregion

    #region utilities

    public Vector2 getLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
    {
        Vector2 worldMousePosition = mousePosition;
        if (isSearchWindow)
        {
            worldMousePosition -= editorWindow.position.position;
        }

        Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

        return localMousePosition;
    }

    #endregion

    #region Serializable Dictionary

     public class SerializableDictionary
    {
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : SerializableDictionary, IDictionary<TKey, TValue>,
        ISerializationCallbackReceiver
    {
        [SerializeField] private List<SerializableKeyValuePair> list = new List<SerializableKeyValuePair>();

        [Serializable]
        public struct SerializableKeyValuePair
        {
            public TKey key;
            public TValue value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }

            public void setValue(TValue value)
            {
                this.value = value;
            }
        }

        private Dictionary<TKey, uint> KeyPositions => keyPositions.Value;
        private Lazy<Dictionary<TKey, uint>> keyPositions;

        public SerializableDictionary()
        {
            keyPositions = new Lazy<Dictionary<TKey, uint>>(makeKeyPositions);
        }

        public SerializableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            keyPositions = new Lazy<Dictionary<TKey, uint>>(makeKeyPositions);

            if (dictionary == null)
            {
                throw new ArgumentException("The passed dictionary is null.");
            }

            foreach (KeyValuePair<TKey, TValue> pair in dictionary)
            {
                Add(pair.Key, pair.Value);
            }
        }

        private Dictionary<TKey, uint> makeKeyPositions()
        {
            int numEntries = list.Count;

            Dictionary<TKey, uint> result = new Dictionary<TKey, uint>(numEntries);

            for (int i = 0; i < numEntries; ++i)
            {
                result[list[i].key] = (uint)i;
            }

            return result;
        }

        public void OnBeforeSerialize()
        {
            // Method intentionally left empty.
        }

        public void OnAfterDeserialize()
        {
            // After deserialization, the key positions might be changed
            keyPositions = new Lazy<Dictionary<TKey, uint>>(makeKeyPositions);
        }

        #region IDictionary

        public TValue this[TKey key]
        {
            get => list[(int)KeyPositions[key]].value;
            set
            {
                if (KeyPositions.TryGetValue(key, out uint index))
                {
                    list[(int)index].setValue(value);
                }
                else
                {
                    KeyPositions[key] = (uint)list.Count;

                    list.Add(new SerializableKeyValuePair(key, value));
                }
            }
        }

        public ICollection<TKey> Keys => list.Select(tuple => tuple.key).ToArray();
        public ICollection<TValue> Values => list.Select(tuple => tuple.value).ToArray();

        public void Add(TKey key, TValue value)
        {
            if (KeyPositions.ContainsKey(key))
            {
                throw new ArgumentException("An element with the same key already exists in the dictionary.");
            }

            KeyPositions[key] = (uint)list.Count;

            list.Add(new SerializableKeyValuePair(key, value));
        }

        public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (KeyPositions.TryGetValue(key, out uint index))
            {
                Dictionary<TKey, uint> kp = KeyPositions;

                kp.Remove(key);

                list.RemoveAt((int)index);

                int numEntries = list.Count;

                for (uint i = index; i < numEntries; i++)
                {
                    kp[list[(int)i].key] = i;
                }

                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (KeyPositions.TryGetValue(key, out uint index))
            {
                value = list[(int)index].value;

                return true;
            }

            value = default;

            return false;
        }

        #endregion

        #region ICollection

        public int Count => list.Count;
        public bool IsReadOnly => false;

        public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

        public void Clear()
        {
            list.Clear();
            KeyPositions.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int numKeys = list.Count;

            if (array.Length - arrayIndex < numKeys)
            {
                throw new ArgumentException("arrayIndex");
            }

            for (int i = 0; i < numKeys; ++i, ++arrayIndex)
            {
                SerializableKeyValuePair entry = list[i];

                array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

        #endregion

        #region IEnumerable

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return list.Select(ToKeyValuePair).GetEnumerator();

            KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
            {
                return new KeyValuePair<TKey, TValue>(skvp.key, skvp.value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }

    #endregion
    
}
