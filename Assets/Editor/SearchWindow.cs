using System.Collections;
using System.Collections.Generic;
using TrungKien.Window.Element;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace TrungKien.Window
{
    public class ConfigModelSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        ConfigModelGraphView graphView;
        Texture2D indentationIcon;
        public void Init(ConfigModelGraphView graphView)
        {
            this.graphView = graphView;
            indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, Color.clear);
            indentationIcon.Apply();
        }
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> searchTreeEntries = new List<SearchTreeEntry>()
            {
                new SearchTreeGroupEntry(new GUIContent("Create Element")),
                new SearchTreeGroupEntry(new GUIContent("Node"), 1),
                new SearchTreeEntry(new GUIContent("Single Choice", indentationIcon))
                {
                    level = 2,
                    userData = typeof(ConfigModelSingleChoice)
                },
                new SearchTreeEntry(new GUIContent("Multi Choice", indentationIcon))
                {
                    level = 2,
                    userData = typeof(ConfigModelMultiChoice)
                },
                new SearchTreeGroupEntry(new GUIContent("Group"), 1),
                new SearchTreeEntry(new GUIContent("Group", indentationIcon))
                {
                    level = 2,
                    userData = typeof(Group)
                }
            };
            return searchTreeEntries;
        }

        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            Vector2 localMousePosition = graphView.GetLocalMousePosition(context.screenMousePosition);

            if (SearchTreeEntry.userData is System.Type nodeType && typeof(ConfigModelNode).IsAssignableFrom(nodeType))
            {
                ConfigModelNode node = graphView.CreateNote(nodeType, localMousePosition);
                graphView.AddElement(node);
                return true;
            }
            else if (SearchTreeEntry.userData is System.Type groupType && groupType == typeof(Group))
            {
                Group group = graphView.CreateGroup("Group", localMousePosition);
                graphView.AddElement(group);
                return true;
            }
            return false;
        }
    }
}