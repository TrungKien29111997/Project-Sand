using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class ConfigGraphWindow : EditorWindow
{
    private ConfigGraphView graphView;
    private ConfigGraphSO graphSO;
    private string searchQuery = "";

    [MenuItem("Window/Config Graph")]
    public static void Open()
    {
        var wnd = GetWindow<ConfigGraphWindow>();
        wnd.titleContent = new GUIContent("Config Graph");
    }
    public static void Open(ConfigGraphSO so)
    {
        var wnd = GetWindow<ConfigGraphWindow>();
        wnd.titleContent = new GUIContent("Config Graph");
        wnd.graphSO = so;
        wnd.Show();
        wnd.LoadFromSO(so);
    }
    private void LoadFromSO(ConfigGraphSO so)
    {
        graphView.SetTargetSO(so);
        graphView.LoadFromSO();
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    void ConstructGraphView()
    {
        graphView = new ConfigGraphView
        {
            name = "Config Graph"
        };
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var btnNewNode = new Button(() => graphView.CreateNode()) { text = "New Node" };
        toolbar.Add(btnNewNode);

        var btnSave = new Button(() => graphView.SaveToSO()) { text = "Save" };
        toolbar.Add(btnSave);

        var btnLoad = new Button(() => graphView.LoadFromSO()) { text = "Load" };
        toolbar.Add(btnLoad);

        var objectField = new ObjectField("GraphSO")
        {
            objectType = typeof(ConfigGraphSO),
            allowSceneObjects = false
        };
        objectField.RegisterValueChangedCallback(evt =>
        {
            graphSO = evt.newValue as ConfigGraphSO;
            graphView.SetTargetSO(graphSO);
        });
        toolbar.Add(objectField);

        // Search
        var searchField = new ToolbarSearchField();
        searchField.RegisterValueChangedCallback(evt =>
        {
            searchQuery = evt.newValue;
            graphView.HighlightSearch(searchQuery);
        });
        toolbar.Add(searchField);

        rootVisualElement.Add(toolbar);
    }
}