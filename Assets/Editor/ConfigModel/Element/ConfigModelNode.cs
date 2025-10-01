using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace TrungKien.Window.Element
{
    public class ConfigModelNode : Node
    {
        public string noteName;
        public List<string> choices;

        public virtual void Init(Vector2 pos)
        {
            noteName = "Default";
            choices = new List<string>();
            SetPosition(new Rect(pos, Vector2.zero));
        }
        public virtual void Draw()
        {
            TextField titleNameTextField = EditorUtility.CreatTextField(noteName);
            titleContainer.Insert(0, titleNameTextField);

            Port inputPort = this.CreatePort("InputName", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            VisualElement customDataContaner = new VisualElement();
            Foldout textFoldout = EditorUtility.CreatFoldout("Custom Data");
            TextField textTextField = EditorUtility.CreateTextArea("Haha");
            textFoldout.Add(textTextField);
            customDataContaner.Add(textFoldout);
            extensionContainer.Add(customDataContaner);
        }
    }
}