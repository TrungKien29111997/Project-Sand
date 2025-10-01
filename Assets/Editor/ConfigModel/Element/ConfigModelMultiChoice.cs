using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace TrungKien.Window.Element
{
    public class ConfigModelMultiChoice : ConfigModelNode
    {
        public override void Init(Vector2 pos)
        {
            base.Init(pos);
            choices.Add("New Choice");
        }
        public override void Draw()
        {
            base.Draw();
            Button addChoice = EditorUtility.CreateButton("+", () =>
            {
                Port port = CreatChoicePort("New Choice");
                choices.Add("New Choice");
                outputContainer.Add(port);
            });
            mainContainer.Insert(1, addChoice);
            foreach (string choice in choices)
            {
                Port port = CreatChoicePort(choice);
                outputContainer.Add(port);
            }
            RefreshExpandedState();
            RefreshPorts();
        }
        Port CreatChoicePort(string textName)
        {
            Port port = this.CreatePort("Out", Orientation.Horizontal, Direction.Output, Port.Capacity.Single);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.flexGrow = 1;

            Button deleteChoisebutton = EditorUtility.CreateButton("X");
            TextField choiceTextField = EditorUtility.CreatTextField(textName);
            choiceTextField.style.width = 200;

            row.Add(choiceTextField);
            row.Add(deleteChoisebutton);
            port.Add(row);
            return port;
        }
    }
}