using System;
using System.Collections;
using System.Collections.Generic;
using TrungKien.Window.Element;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.GlobalVariables;
using UnityEngine.UIElements;
namespace TrungKien.Window
{
#if UNITY_EDITOR
    public static class EditorUtility
    {
        public static TextField CreatTextField(string title = null, EventCallback<ChangeEvent<string>> callback = null)
        {
            TextField textField = new TextField(title)
            {
                value = title
            };
            if (callback != null)
            {
                textField.RegisterValueChangedCallback(callback);
            }
            return textField;
        }
        public static TextField CreateTextArea(string title = null, EventCallback<ChangeEvent<string>> callback = null)
        {
            TextField textArea = CreatTextField(title, callback);
            textArea.multiline = true;
            return textArea;
        }
        public static Foldout CreatFoldout(string type, bool collape = false)
        {
            Foldout foldout = new Foldout()
            {
                text = type,
                value = !collape
            };
            return foldout;
        }
        public static Button CreateButton(string title, Action onClick = null)
        {
            Button button = new Button(onClick)
            {
                text = title
            };
            return button;
        }
        public static Port CreatePort(this ConfigModelNode node, string portName = "", Orientation orientation = Orientation.Horizontal, Direction direction = Direction.Input, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = node.InstantiatePort(orientation, direction, capacity, typeof(bool));
            port.portName = portName;
            return port;
        }
    }
#endif
}