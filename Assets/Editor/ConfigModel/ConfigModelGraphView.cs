using System;
using System.Collections;
using System.Collections.Generic;
using TrungKien.Window.Element;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TrungKien.Window
{
    public class ConfigModelGraphView : GraphView
    {
        ConfigModelSearchWindow searchWindow;
        // contructor
        public ConfigModelGraphView()
        {
            AddGridBackGround();
            AddManipulators();
            AddSearchWindow();
            AddStyles();
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<ConfigModelSearchWindow>();
                searchWindow.Init(this);
            }

            nodeCreationRequest = ctx => SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), searchWindow);
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

        public ConfigModelNode CreateNote(Type type, Vector2 pos)
        {
            ConfigModelNode node = (ConfigModelNode)Activator.CreateInstance(type);
            node.Init(pos);
            node.Draw();
            return node;
        }

        // add Control to graph view
        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu<ConfigModelSingleChoice>("Add Single Choice"));
            this.AddManipulator(CreateNodeContextualMenu<ConfigModelMultiChoice>("Add Multi Choice"));
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("Group", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }

        public Group CreateGroup(string v, Vector2 localMousePosition)
        {
            Group group = new Group()
            {
                title = v
            };
            group.SetPosition(new Rect(localMousePosition, Vector2.zero));
            return group;
        }

        private IManipulator CreateNodeContextualMenu<T>(string actionTitle) where T : ConfigModelNode
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNote(typeof(T), GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );
            return contextualMenuManipulator;
        }

        // add grid back ground
        void AddGridBackGround()
        {
            GridBackground grid = new GridBackground();
            grid.StretchToParentSize();
            Insert(0, grid);
        }

        // style of grid
        void AddStyles()
        {
            StyleSheet styleSheet = (StyleSheet)EditorGUIUtility.Load("ConfigModelStyleSheet.uss");
            styleSheets.Add(styleSheet);
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }
    }
}