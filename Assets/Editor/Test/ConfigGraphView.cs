using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization.Plugins.XLIFF.V12;

public class ConfigGraphView : GraphView
{
    private ConfigGraphSO graphSO;

    public ConfigGraphView()
    {
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();
    }
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> compatiblePorts = new List<Port>();
        ports.ForEach(p =>
        {
            if (p != startPort && p.node != startPort.node && p.direction != startPort.direction)
            {
                compatiblePorts.Add(p);
            }
        });
        return compatiblePorts;
    }


    public void SetTargetSO(ConfigGraphSO so) => graphSO = so;

    // ✅ Tạo node với title random unique
    public void CreateNode()
    {
        var node = new ConfigNode();
        node.capabilities |= Capabilities.Movable | Capabilities.Deletable;
        var model = new ConfigGraphSO.ConfigModel();
        model.id = 0;
        model.title = node.title;
        node.userData = model;

        SubCreateNode(node, model, GenerateUniqueTitle(), Vector2.zero);

        AddElement(node);
    }

    private string GenerateUniqueTitle()
    {
        var existingTitles = nodes.OfType<Node>().Select(n => n.title).ToHashSet();
        System.Random rnd = new System.Random();
        string title;
        do { title = rnd.Next(1, 100000).ToString(); }
        while (existingTitles.Contains(title));
        return title;
    }

    // ✅ Save Graph
    public void SaveToSO()
    {
        if (graphSO == null) return;

        graphSO.nodes.Clear();
        graphSO.edges.Clear();

        Dictionary<int, string> idSet = new();

        foreach (var n in nodes.OfType<Node>())
        {
            var model = (ConfigGraphSO.ConfigModel)n.userData;
            model.position = n.GetPosition().position;

            // lấy id trong IntegerField
            foreach (var c in n.extensionContainer.Children())
                if (c is IntegerField f) model.id = f.value;

            // check trùng ID
            if (idSet.ContainsKey(model.id))
            {
                Debug.LogError($"❌ Node ID {model.id} bị trùng! (Title={n.title}__{idSet[model.id]})");
                return;
            }
            idSet.Add(model.id, model.title);

            model.listChildrenId = new List<int>();
            model.title = n.title;
            graphSO.nodes.Add(model);
        }

        foreach (var e in edges.ToList())
        {
            if (e.output?.node != null && e.input?.node != null)
            {
                var parent = (ConfigGraphSO.ConfigModel)e.output.node.userData;
                var child = (ConfigGraphSO.ConfigModel)e.input.node.userData;

                graphSO.edges.Add(new ConfigGraphSO.EdgeData
                {
                    parentId = parent.id,
                    childId = child.id
                });
                parent.listChildrenId.Add(child.id);
            }
        }

        EditorUtility.SetDirty(graphSO);
        AssetDatabase.SaveAssets();
        Debug.Log("✅ Graph saved");
    }

    // ✅ Load Graph
    public void LoadFromSO()
    {
        if (graphSO == null) return;

        // Xóa toàn bộ element cũ
        DeleteElements(graphElements.ToList());

        // Lookup để lưu Node theo ID
        Dictionary<int, Node> nodeLookup = new Dictionary<int, Node>();

        // --- Tạo Node ---
        foreach (var m in graphSO.nodes)
        {
            var node = new ConfigNode()
            {
                title = m.title,
                userData = m
            };

            node.nodeId = m.id;
            SubCreateNode(node, m, m.title, m.position);

            AddElement(node);
            nodeLookup[m.id] = node;
        }

        // --- Tạo Edge ---
        foreach (var e in graphSO.edges)
        {
            if (nodeLookup.ContainsKey(e.parentId) && nodeLookup.ContainsKey(e.childId))
            {
                var parentNode = nodeLookup[e.parentId];
                var childNode = nodeLookup[e.childId];

                var outPort = parentNode.outputContainer.Q<Port>();
                var inPort = childNode.inputContainer.Q<Port>();

                if (outPort == null || inPort == null)
                {
                    Debug.LogWarning($"⚠️ Không tìm thấy port cho edge {e.parentId} -> {e.childId}");
                    continue;
                }

                var edge = new Edge
                {
                    output = outPort,
                    input = inPort
                };

                outPort.Connect(edge);
                inPort.Connect(edge);

                AddElement(edge);
            }
        }
    }

    void SubCreateNode(ConfigNode node, ConfigGraphSO.ConfigModel model, string title, Vector2 position)
    {
        node.graphSO = graphSO;
        node.title = title;
        node.SetPosition(new Rect(position, new Vector2(200, 150)));

        // Input & Output
        var inputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(int));
        inputPort.portName = "In";
        node.inputContainer.Add(inputPort);

        var outputPort = node.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(int));
        outputPort.portName = "Out";
        node.outputContainer.Add(outputPort);

        // ID Field
        var intField = new IntegerField("ID") { value = model.id };
        intField.RegisterValueChangedCallback(evt =>
        {
            model.id = evt.newValue;
            node.nodeId = evt.newValue;
        });
        node.extensionContainer.Add(intField);

        node.RefreshExpandedState();
        node.RefreshPorts();
    }


    // ✅ Search node theo Title hoặc ID
    public void HighlightSearch(string query)
    {
        foreach (var n in nodes.OfType<Node>())
            n.RemoveFromClassList("searchHighlight");

        if (string.IsNullOrEmpty(query)) return;

        foreach (var n in nodes.OfType<Node>())
        {
            var model = (ConfigGraphSO.ConfigModel)n.userData;
            if (n.title == query || model.id.ToString() == query)
            {
                Debug.Log($"🔍 Found node: Title={n.title}, ID={model.id}, Pos={n.GetPosition().position}");
                n.AddToClassList("searchHighlight");
                this.FrameSelection(); // focus
            }
        }
    }
}

public class ConfigNode : Node
{
    public ConfigGraphSO graphSO;
    public int nodeId;
    public ConfigNode() { }

    public override void OnSelected()
    {
        base.OnSelected();
        // ✅ Bắn event cho đúng ScriptableObject
        graphSO?.InvokeNodeSelectedAction(nodeId);
        //Debug.Log($"📌 Node {title} được chọn, ID = {nodeId}");
    }
}
