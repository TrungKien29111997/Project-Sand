using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace TrungKien.Window
{
    public class ConfigModelEditorWindow : EditorWindow
    {
        [MenuItem("Window/TrungKien/ConfigModelEditorWindow")]
        public static void ShowWindow()
        {
            GetWindow<ConfigModelEditorWindow>("ConfigModelEditorWindow");
        }
        void OnEnable()
        {
            AddGraphView();
        }
        void AddGraphView()
        {
            ConfigModelGraphView graphView = new ConfigModelGraphView();
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);
        }
    }
}